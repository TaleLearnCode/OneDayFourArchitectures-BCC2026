namespace DomsGarage.Models;

/// <summary>
/// Tracks a car's current state in the shop.
/// Used during the walkthrough to illustrate shared state across all features.
/// Anti-pattern note: every service that touches Car depends on this enum — no walls.
/// </summary>
public enum CarStatus
{
    InGarage,        // Checked in, work not yet started
    InProgress,      // Active job(s) open
    ReadyForPickup   // All jobs closed — set automatically by JobService
}
