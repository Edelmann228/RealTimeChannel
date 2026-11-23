namespace RealTimeChannel;


/// Сообщение реального времени.
/// Приоритет: чем МЕНЬШЕ число, тем ВЫШЕ приоритет (1 — высший).

public sealed record Message(
    string Name,
    int Priority,
    int SizeBits,
    double PeriodMs,
    double DeadlineMs);

/// <summary>Параметры канала передачи данных.</summary>
public sealed record Channel(
    double BitrateBps,
    double PropagationDelayMs,
    double ProcessingDelayMs);