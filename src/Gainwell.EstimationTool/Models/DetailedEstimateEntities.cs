namespace Gainwell.EstimationTool.Models;

/// <summary>
/// Persisted SE component-type row from the Detailed Estimate Development grid.
/// Each row represents one of the 11 component types with aggregate totals.
/// </summary>
public class DetailedSeComponentEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string ComponentType { get; set; } = string.Empty;
    public int SimpleTotal { get; set; }
    public int ModerateTotal { get; set; }
    public int ComplexTotal { get; set; }
    public decimal HoursTotal { get; set; }
    public decimal AdjustedExpLevel { get; set; }
    public decimal AdjustedHrs { get; set; }
    public decimal GrandTotal { get; set; }
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted SE module entry (drill-down detail for each component type row).
/// </summary>
public class DetailedSeModuleEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string ExperienceLevel { get; set; } = string.Empty;
    public string AssociatedRequirement { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public string ComponentStatus { get; set; } = string.Empty;
    public int SimpleCount { get; set; }
    public int ModerateCount { get; set; }
    public int ComplexCount { get; set; }
    public decimal ComplexityTotal { get; set; }
    public decimal AdjustedExpLevel { get; set; }
    public decimal AdjustedHrs { get; set; }
    public decimal GrandTotal { get; set; }
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted BA test case row from the Detailed Estimate BA Considerations grid.
/// Covers System Testing rows (21 rows × 3 experience groups) + Regression row.
/// </summary>
public class DetailedBaTestCaseEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public bool IsInfoRow { get; set; }
    public decimal SimpleCount { get; set; }
    public decimal ModerateCount { get; set; }
    public decimal ComplexCount { get; set; }
    public decimal VeryComplexCount { get; set; }
    public decimal ManualAdjHours { get; set; }
    public string GridType { get; set; } = "TestCases"; // "TestCases" or "Regression"
    public string Notes { get; set; } = string.Empty;
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted BA Production Validation row from the Detailed Estimate.
/// </summary>
public class DetailedBaValidationEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public int SimpleCount { get; set; }
    public int ModerateCount { get; set; }
    public int ComplexCount { get; set; }
    public int VeryComplexCount { get; set; }
    public decimal ManualAdjHours { get; set; }
    public string Notes { get; set; } = string.Empty;
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted Consultant/Mentor row from the Detailed Estimate Collaboration tab.
/// </summary>
public class DetailedConsultantEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int LineNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Hours { get; set; }
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted Collaboration meeting fields from the Detailed Estimate Collaboration tab.
/// One row per meeting type (WPR, Client, Internal).
/// </summary>
public class DetailedCollabMeetingEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public string MeetingType { get; set; } = string.Empty; // "WPR", "Client", "Internal"
    public decimal MeetingCount { get; set; }
    public decimal MeetingHours { get; set; }
    public decimal Attendees { get; set; }
    public decimal PrepHours { get; set; }
    public decimal AdjustedMeeting { get; set; }
    public decimal AdjustedPrep { get; set; }
    public ProjectEntity? Project { get; set; }
}

/// <summary>
/// Persisted miscellaneous Detailed Estimate fields (Promotion, Doc, PM Reserve, etc.)
/// that don't fit in PROJECT_ESTIMATES columns.
/// </summary>
public class DetailedMiscFieldsEntity
{
    public int Id { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public decimal PromotionHours { get; set; }
    public decimal SystemDocHours { get; set; }
    public decimal PmReservePercentage { get; set; }
    public decimal CreateDetailEstHours { get; set; }
    public decimal CreateFinalEstHours { get; set; }
    public decimal PmEffortHours { get; set; }
    public decimal RemainingBddHours { get; set; }
    public decimal SysDocProdValHours { get; set; }
    public decimal BaSysDocHours { get; set; }
    public decimal CommPlanHours { get; set; }
    public string SeAdjustedComment { get; set; } = string.Empty;
    public string BaAdjustedComment { get; set; } = string.Empty;
    public string CollabAdjustedComment { get; set; } = string.Empty;
    public ProjectEntity? Project { get; set; }
}
