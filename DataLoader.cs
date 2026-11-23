using System.Globalization;
using System.Threading.Channels;

namespace RealTimeChannel;

/// Чтение параметров канала и списка сообщений из CSV-файлов.
public static class DataLoader
{
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;