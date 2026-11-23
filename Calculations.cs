using RealTimeChannel;

public static class Calculations
{
    private const double MsPerSecond = 1000.0;

    /// <summary>Время передачи = размер / скорость.</summary>
    public static double TransmissionTime(Message msg, Channel channel)
        => msg.SizeBits / channel.BitrateBps * MsPerSecond;

    /// <summary>Задержка доставки без конкуренции = передача + распространение + обработка.</summary>
    public static double DeliveryDelay(Message msg, Channel channel)
        => TransmissionTime(msg, channel)
           + channel.PropagationDelayMs
           + channel.ProcessingDelayMs;