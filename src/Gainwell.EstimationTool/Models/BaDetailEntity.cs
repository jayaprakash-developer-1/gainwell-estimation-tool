namespace Gainwell.EstimationTool.Models;

/// <summary>
/// Persists a single BA Considerations row (WTC section, PV section, or standalone fields).
/// Foreign key to PROJECT_ESTIMATES via PROJECT_ID.
/// </summary>
public class BaDetailEntity
{
    public int BaDetailId { get; set; }

    public string ProjectId { get; set; } = string.Empty;

    /// <summary>Section: "WTC", "Regression", "PV", or "Standalone"</summary>
    public string Section { get; set; } = string.Empty;

    /// <summary>Row index within the section (0-based)</summary>
    public int RowIndex { get; set; }

    /// <summary>Experience level: "NewToArea", "Proficient", "Expert" (or empty for standalone)</summary>
    public string ExperienceLevel { get; set; } = string.Empty;

    /// <summary>Task type identifier (e.g., "UnderstandingRequirements", "WriteSystemTestCases", "Iteration", "DataPrep", etc.)</summary>
    public string TaskType { get; set; } = string.Empty;

    public decimal SimpleCount { get; set; }
    public decimal ModerateCount { get; set; }
    public decimal ComplexCount { get; set; }
    public decimal VeryComplexCount { get; set; }
    public decimal ManualAdjHours { get; set; }

    /// <summary>For PV rows: which complexity radio is selected ("Simple","Moderate","Complex","VeryComplex", or empty)</summary>
    public string PvSelection { get; set; } = string.Empty;

    /// <summary>For standalone fields (BDD, SysDoc, CommPlan, etc.)</summary>
    public decimal StandaloneValue { get; set; }

    /// <summary>BA Estimate By name</summary>
    public string BaEstimateBy { get; set; } = string.Empty;

    // Navigation
    public ProjectEntity Project { get; set; } = null!;
}
