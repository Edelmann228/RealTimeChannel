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