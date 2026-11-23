namespace RealTimeChannel;


/// Сообщение реального времени.
/// Приоритет: чем МЕНЬШЕ число, тем ВЫШЕ приоритет (1 — высший).

public sealed record Message(
    string Name,
    int Priority,
    int SizeBits,
    double PeriodMs,
    double DeadlineMs);

/// Параметры канала передачи данных.
public sealed record Channel(
    double BitrateBps,
    double PropagationDelayMs,
    double ProcessingDelayMs);

/// Результат расчёта по одному сообщению .
public sealed record MessageResult(
    Message Message,
    double TransmissionMs,
    double DelayMs,
    double ArbitrationMs,
    bool MeetsDeadline);