using System.Globalization;
using System.Text;
using RealTimeChannel;

Console.OutputEncoding = Encoding.UTF8;
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

// Использование: RealTimeChannel [messages.csv] [channel.csv]
string messagesPath = ResolvePath(args.Length > 0 ? args[0] : "data/messages.csv");
string channelPath = ResolvePath(args.Length > 1 ? args[1] : "data/channel.csv");

try
{
    Channel channel = DataLoader.LoadChannel(channelPath);
    List<Message> messages = DataLoader.LoadMessages(messagesPath);

    // Порядок вывода: от высшего приоритета к низшему
    var ordered = messages.OrderBy(m => m.Priority).ToList();

    var results = ordered.Select(m =>
    {
        double arbitration = Calculations.ArbitrationTime(m, ordered, channel);
        return new MessageResult(
            m,
            Calculations.TransmissionTime(m, channel),
            Calculations.DeliveryDelay(m, channel),
            arbitration,
            Calculations.MeetsDeadline(m, arbitration));
    }).ToList();

    PrintChannel(channel);
    PrintTable(results);
    PrintConclusion(results, ordered, channel);
    return 0;
}
catch (Exception ex) when (ex is FormatException or FileNotFoundException)
{
    Console.Error.WriteLine($"Ошибка: {ex.Message}");
    return 1;
}

// ---------------------------------------------------------------------------

static string ResolvePath(string path)
{
    if (File.Exists(path) || Path.IsPathRooted(path))
        return path;

    // Запуск не из корня проекта — ищем рядом с exe
    string alt = Path.Combine(AppContext.BaseDirectory, path);
    return File.Exists(alt) ? alt : path;
}
static void PrintChannel(Channel c)
{
    Console.WriteLine("=== Параметры канала ===");
    Console.WriteLine($"Скорость передачи:        {c.BitrateBps:N0} бит/с".Replace(',', ' '));
    Console.WriteLine($"Задержка распространения: {c.PropagationDelayMs:0.###} мс");
    Console.WriteLine($"Задержка обработки:       {c.ProcessingDelayMs:0.###} мс");
    Console.WriteLine();
}

static void PrintTable(IReadOnlyList<MessageResult> results)
{
    Console.WriteLine("=== Результаты расчёта ===");

    string[] headers =
    {
        "Сообщение", "Приоритет", "Размер, бит", "Время передачи, мс",
        "Задержка доставки, мс", "Доставка с арбитражем, мс", "Дедлайн, мс", "Уложилось"
    };

    var rows = results.Select(r => new[]
    {
        r.Message.Name,
        r.Message.Priority.ToString(),
        r.Message.SizeBits.ToString(),
        r.TransmissionMs.ToString("0.###"),
        r.DelayMs.ToString("0.###"),
        r.ArbitrationMs.ToString("0.###"),
        r.Message.DeadlineMs.ToString("0.###"),
        r.MeetsDeadline ? "да" : "НЕТ"
    }).ToList();

    int[] widths = headers.Select((h, i) =>
        Math.Max(h.Length, rows.Max(row => row[i].Length))).ToArray();

    string Line(string[] cells) =>
        "| " + string.Join(" | ", cells.Select((c, i) => c.PadRight(widths[i]))) + " |";

    string separator = "+-" + string.Join("-+-", widths.Select(w => new string('-', w))) + "-+";

    Console.WriteLine(separator);
    Console.WriteLine(Line(headers));
    Console.WriteLine(separator);
    foreach (var row in rows)
        Console.WriteLine(Line(row));
    Console.WriteLine(separator);
    Console.WriteLine();
}
static void PrintConclusion(
    IReadOnlyList<MessageResult> results,
    IReadOnlyList<Message> messages,
    Channel channel)
{
    double jitterFree = Calculations.Jitter(results.Select(r => r.DelayMs));
    double jitterArb = Calculations.Jitter(results.Select(r => r.ArbitrationMs));
    double utilization = Calculations.Utilization(messages, channel);

    Console.WriteLine("=== Заключение ===");
    Console.WriteLine($"Джиттер без конкуренции:       {jitterFree:0.###} мс");
    Console.WriteLine($"Джиттер при арбитраже:         {jitterArb:0.###} мс");
    Console.WriteLine($"Загрузка канала:               {utilization * 100:0.#} %");

    var worst = results.MaxBy(r => r.ArbitrationMs)!;
    var best = results.MinBy(r => r.ArbitrationMs)!;
    double extra = worst.ArbitrationMs - worst.DelayMs;

    Console.WriteLine();
    Console.WriteLine(
        $"Приоритетный арбитраж: наихудшее время доставки у {worst.Message.Name} " +
        $"(приоритет {worst.Message.Priority}) — {worst.ArbitrationMs:0.###} мс; " +
        $"из них {extra:0.###} мс — ожидание более приоритетных сообщений. " +
        $"У {best.Message.Name} (приоритет {best.Message.Priority}) доставка занимает " +
        $"{best.ArbitrationMs:0.###} мс.");

    var failed = results.Where(r => !r.MeetsDeadline).ToList();
    if (failed.Count == 0)
    {
        Console.WriteLine("Все сообщения укладываются в свои дедлайны.");
    }
    else
    {
        Console.WriteLine("Дедлайн НЕ выдерживают: " +
            string.Join(", ", failed.Select(r =>
                $"{r.Message.Name} ({r.ArbitrationMs:0.###} мс > {r.Message.DeadlineMs:0.###} мс)")) + ".");
    }

    if (utilization > 1.0)
        Console.WriteLine("Внимание: загрузка канала превышает 100 % — расписание невыполнимо.");
}