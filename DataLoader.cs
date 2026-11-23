using System.Globalization;
using System.Threading.Channels;

namespace RealTimeChannel;

/// Чтение параметров канала и списка сообщений из CSV-файлов.
public static class DataLoader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
 
    /// Формат: заголовок "parameter,value", далее строки
    /// bitrate_bps / propagation_delay_ms / processing_delay_ms.
    public static Channel LoadChannel(string path)
    {
        var values = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);

        foreach (var (line, number) in ReadDataLines(path))
        {
            var parts = line.Split(',');
            if (parts.Length < 2)
                throw new FormatException($"{path}, строка {number}: ожидается «параметр,значение».");

            values[parts[0].Trim()] = ParseDouble(parts[1], path, number);
        }

        double Get(string key) => values.TryGetValue(key, out var v)
            ? v
            : throw new FormatException($"{path}: не найден параметр «{key}».");

        var channel = new Channel(
            Get("bitrate_bps"),
            Get("propagation_delay_ms"),
            Get("processing_delay_ms"));

        if (channel.BitrateBps <= 0)
            throw new FormatException($"{path}: скорость передачи должна быть > 0.");
        if (channel.PropagationDelayMs < 0 || channel.ProcessingDelayMs < 0)
            throw new FormatException($"{path}: задержки не могут быть отрицательными.");

        return channel;
    }
    /// <summary>
    /// Формат: заголовок "name,priority,size_bits,period_ms,deadline_ms".
    /// Если deadline_ms пуст — дедлайн равен периоду.
    /// </summary>
    public static List<Message> LoadMessages(string path)
    {
        var result = new List<Message>();

        foreach (var (line, number) in ReadDataLines(path))
        {
            var p = line.Split(',');
            if (p.Length < 4)
                throw new FormatException(
                    $"{path}, строка {number}: ожидается name,priority,size_bits,period_ms[,deadline_ms].");

            string name = p[0].Trim();
            int priority = ParseInt(p[1], path, number);
            int sizeBits = ParseInt(p[2], path, number);
            double period = ParseDouble(p[3], path, number);
            double deadline = p.Length > 4 && !string.IsNullOrWhiteSpace(p[4])
                ? ParseDouble(p[4], path, number)
                : period;

            if (sizeBits <= 0 || period <= 0 || deadline <= 0)
                throw new FormatException($"{path}, строка {number}: размер, период и дедлайн должны быть > 0.");

            result.Add(new Message(name, priority, sizeBits, period, deadline));
        }

        if (result.Count == 0)
            throw new FormatException($"{path}: нет ни одного сообщения.");

        return result;
    }
    /// <summary>Строки файла без заголовка, пустых строк и комментариев (#).</summary>
    private static IEnumerable<(string Line, int Number)> ReadDataLines(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Файл не найден: {path}");

        var lines = File.ReadAllLines(path);
        for (int i = 1; i < lines.Length; i++) // i = 0 — заголовок
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
                continue;
            yield return (line, i + 1);
        }
    }