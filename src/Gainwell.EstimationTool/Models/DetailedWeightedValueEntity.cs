namespace Gainwell.EstimationTool.Models;

/// <summary>
/// Database entity for storing SE detailed weighted values.
/// Each row represents hours for a specific (ComponentType, TaskPhase, ComponentStatus, Complexity) combination.
/// </summary>
public class DetailedSeWeightedValueEntity
{
    public int Id { get; set; }
    public ComponentType ComponentType { get; set; }
    public SeTaskPhase TaskPhase { get; set; }
    public ComponentStatus ComponentStatus { get; set; }
    public Complexity Complexity { get; set; }
    public decimal Hours { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = "System";
}

/// <summary>
/// Database entity for storing BA detailed weighted values.
/// Each row represents hours for a specific (Category, TaskType, Complexity) combination.
/// </summary>
public class DetailedBaWeightedValueEntity
{
    public int Id { get; set; }
    public BaCategory Category { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public BaComplexity Complexity { get; set; }
    public decimal Hours { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = "System";
}

/// <summary>
/// Database entity for storing experience level multipliers.
/// </summary>
public class ExperienceLevelEntity
{
    public int Id { get; set; }
    public EstimateRole Role { get; set; }
    public ExperienceLevel Level { get; set; }
    public decimal Multiplier { get; set; }
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    public string ModifiedBy { get; set; } = "System";
}
