namespace DomsGarage.Models;

/// <summary>
/// An item in Dom's parts inventory.
/// Standalone — no job-to-parts relationship in the scaffold (no JobPart join entity).
/// Walkthrough note: mechanics browse inventory manually. Keeps entity count low.
/// Anti-pattern note: one DbContext, one table — a Parts team and a Jobs team both
/// depend on this single schema. At scale, schema changes cause deployment contention.
/// </summary>
public class Part
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int StockQuantity { get; set; }
    public decimal UnitCost { get; set; }
}
