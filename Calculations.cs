using RealTimeChannel;

public static class Calculations
{
    private const double MsPerSecond = 1000.0;

    /// Время передачи = размер / скорость.
    public static double TransmissionTime(Message msg, Channel channel)
        => msg.SizeBits / channel.BitrateBps * MsPerSecond;

    /// Задержка доставки без конкуренции = передача + распространение + обработка.
    public static double DeliveryDelay(Message msg, Channel channel)
        => TransmissionTime(msg, channel)
           + channel.PropagationDelayMs
           + channel.ProcessingDelayMs;
  
    /// Наихудшее время доставки при приоритетном арбитраже: к задержке доставки
    /// добавляется время передачи всех более приоритетных сообщений
    /// (меньшее число = выше приоритет), попавших в окно ожидания.
   
    public static double ArbitrationTime(Message msg, IReadOnlyList<Message> allMsgs, Channel channel)
    {
        double waiting = allMsgs
            .Where(other => other.Priority < msg.Priority)
            .Sum(other => TransmissionTime(other, channel));

        return waiting + DeliveryDelay(msg, channel);
    }
    /// Джиттер как разброс времени доставки: max − min.
    public static double Jitter(IEnumerable<double> delays)
    {
        var list = delays.ToList();
        return list.Count == 0 ? 0.0 : list.Max() - list.Min();
    }

    /// Укладывается ли время доставки в дедлайн сообщения.
    public static bool MeetsDeadline(Message msg, double deliveryTimeMs)
        => deliveryTimeMs <= msg.DeadlineMs;

    /// Загрузка канала: сумма (время передачи / период) по всем сообщениям.
    public static double Utilization(IEnumerable<Message> msgs, Channel channel)
        => msgs.Sum(m => TransmissionTime(m, channel) / m.PeriodMs);
}