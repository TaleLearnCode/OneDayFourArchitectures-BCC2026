namespace TheCircuit.SharedKernel.Enums;

/// <summary>
/// Race infraction types for penalty system.
/// Used by Penalties module to categorize rule violations.
/// Each type carries semantic meaning for racing federation rules.
/// </summary>
public enum PenaltyType
{
    Speeding = 1,      // Exceeded track speed limit
    CourseCut = 2,     // Cut across track boundary or non-racing line
    Contact = 3,       // Collision with another racer
    Conduct = 4        // Unsportsmanlike conduct or language
}
