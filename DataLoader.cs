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
