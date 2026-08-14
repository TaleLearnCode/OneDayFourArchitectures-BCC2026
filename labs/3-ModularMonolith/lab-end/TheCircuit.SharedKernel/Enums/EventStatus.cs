namespace TheCircuit.SharedKernel.Enums;

/// <summary>
/// Race event lifecycle status.
/// Used by Events module to track event progress.
/// </summary>
public enum EventStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}
