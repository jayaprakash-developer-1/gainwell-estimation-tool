using Gainwell.EstimationTool.Data;

namespace Gainwell.EstimationTool.Services;

/// <summary>
/// Core calculation engine for the Initial Estimate pipeline.
/// Implements the 10-step cascading calculation that matches the Excel PROMISe tool exactly.
/// Extracted from InitialEstimateViewModel for testability and single-responsibility.
/// </summary>
public static class CalculationEngine
{
    /// <summary>
    /// Excel ROUNDUP(x, 2) — always rounds away from zero at the 3rd decimal place.
    /// Uses decimal arithmetic for financial precision.
    /// </summary>
    public static decimal RoundUp(decimal value)
    {
        if (value == 0) return 0;
        decimal shifted = value * 100m;
        decimal truncated = Math.Truncate(shifted);
        if (shifted > truncated)
            return (truncated + 1m) / 100m;
        else if (shifted < truncated)
            return (truncated - 1m) / 100m;
        return truncated / 100m;
    }

    /// <summary>
    /// MROUND — rounds to the nearest multiple (used by BA Detailed Estimate).
    /// </summary>
    public static decimal MRound(decimal value, decimal multiple = 0.25m) =>
        multiple == 0 ? value : Math.Round(value / multiple, MidpointRounding.AwayFromZero) * multiple;

    /// <summary>
    /// Calculates the complete Initial Estimate pipeline from inputs and returns all derived values.
    /// Each step uses "effective" values (calculated + adjusted) from upstream tasks.
    /// </summary>
    public static CalculationResult Calculate(CalculationInput input)
    {
        var result = new CalculationResult();

        // Step 1: Development = sum of all component hours
        result.TotalDevelopmentHours = input.DevelopmentHours;

        // Effective Development = component hours + adjustment
        decimal effectiveDev = input.DevelopmentHours + input.DevelopmentAdjustedHours;

        // Step 2: System Testing (depends on effectiveDev)
        if (input.UseTestCasesForEstimate)
        {
            const decimal r31Simple = 2.1925m, r31Medium = 4.065m, r31Complex = 8.76m, r31VeryComplex = 14.38m;
            const decimal r32Simple = 1.5675m, r32Medium = 3.44m, r32Complex = 7.51m, r32VeryComplex = 13.13m;
            decimal mainHours = input.TestCasesSimple * r31Simple + input.TestCasesMedium * r31Medium
                              + input.TestCasesComplex * r31Complex + input.TestCasesVeryComplex * r31VeryComplex;
            decimal defectHours = (input.TestCasesSimple * r32Simple + input.TestCasesMedium * r32Medium
                                + input.TestCasesComplex * r32Complex + input.TestCasesVeryComplex * r32VeryComplex) * 0.1m;
            result.SystemTestingHours = RoundUp((mainHours + defectHours) * Math.Max(1m, input.TestCaseIterations));
        }
        else
        {
            result.SystemTestingHours = RoundUp(effectiveDev * 0.30m);
        }
        decimal effectiveSysTest = result.SystemTestingHours + input.SystemTestingAdjustedHours;

        // Step 3: Analysis (depends on effectiveDev + effectiveSysTest)
        result.AnalysisHours = RoundUp((effectiveDev + effectiveSysTest) * 0.05m);
        decimal effectiveAnalysis = result.AnalysisHours + input.AnalysisAdjustedHours;

        // Step 4: Business Design (depends on effectiveDev + effectiveSysTest)
        result.BusinessDesignHours = RoundUp((effectiveDev + effectiveSysTest) * 0.15m);
        decimal effectiveBizDesign = result.BusinessDesignHours + input.BusinessDesignAdjustedHours;

        // Step 5: Promotion (depends on effectiveDev)
        result.PromotionHours = RoundUp(effectiveDev * 0.05m);
        decimal effectivePromotion = result.PromotionHours + input.PromotionAdjustedHours;

        // Step 6: BA System Documentation (depends on effectiveDev)
        result.BaSystemDocHours = RoundUp(effectiveDev * 0.05m);
        decimal effectiveBaSysDoc = result.BaSystemDocHours + input.BaSystemDocAdjustedHours;

        // Step 7: Production Validation (depends on effectiveSysTest)
        result.ProductionValidationHours = RoundUp(effectiveSysTest * 0.20m);
        decimal effectiveProdVal = result.ProductionValidationHours + input.ProductionValidationAdjustedHours;

        // Step 8: PM Effort (depends on all effective task hours)
        decimal allEffectiveTasks = effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                                  + effectivePromotion + effectiveBaSysDoc + effectiveProdVal;
        result.ProjectManagementHours = RoundUp(allEffectiveTasks * (input.PmEffortPercentage / 100m));
        decimal effectivePM = result.ProjectManagementHours + input.ProjectManagementAdjustedHours;

        // Step 9: Per-type collaboration totals
        result.WprsTotalHours = input.WprsHours + input.WprsAdjustedHours;
        result.ClientMeetingsTotalHours = input.ClientMeetingsHours + input.ClientMeetingsAdjustedHours;
        result.InternalMeetingsTotalHours = input.InternalMeetingsHours + input.InternalMeetingsAdjustedHours;
        result.AutomationTestCollabTotalHours = input.AutomationTestCollabHours + input.AutomationTestCollabAdjustedHours;
        result.ConsultantMentorTotalHours = input.ConsultantMentorHours + input.ConsultantMentorAdjustedHours;

        decimal effectiveCollab = result.WprsTotalHours + result.ClientMeetingsTotalHours
                                + result.InternalMeetingsTotalHours + result.AutomationTestCollabTotalHours
                                + result.ConsultantMentorTotalHours;

        // Per-task effective totals
        result.DevelopmentTotalHours = effectiveDev;
        result.SystemTestingTotalHours = effectiveSysTest;
        result.AnalysisTotalHours = effectiveAnalysis;
        result.BusinessDesignTotalHours = effectiveBizDesign;
        result.PromotionTotalHours = effectivePromotion;
        result.BaSystemDocTotalHours = effectiveBaSysDoc;
        result.ProductionValidationTotalHours = effectiveProdVal;
        result.ProjectManagementTotalHours = effectivePM;
        result.CollaborationTotalHours = effectiveCollab;

        // Step 10: Subtotal
        result.SubtotalHours = RoundUp(effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                             + effectivePromotion + effectiveBaSysDoc + effectiveProdVal
                             + effectivePM + effectiveCollab + input.TimeForEstimates + input.TotalActualHours);

        // Grand Total = Subtotal rounded up to whole number
        if (input.ComponentCount == 0 && effectiveDev == 0m)
        {
            result.GrandTotalHours = 0m;
            result.TShirtSize = "—";
        }
        else
        {
            result.GrandTotalHours = Math.Ceiling(result.SubtotalHours);
            result.TShirtSize = WeightedValues.GetTShirtSize(result.GrandTotalHours);
        }

        // Role Breakout
        result.BaRoleHours = RoundUp(effectiveAnalysis / 2m + effectiveBizDesign + effectiveBaSysDoc
                           + effectiveProdVal + input.TotalActualHours / 2m + input.TimeForEstimates / 2m);
        result.SeRoleHours = RoundUp(effectiveDev + effectiveAnalysis / 2m + effectivePromotion
                           + input.TotalActualHours / 2m + input.TimeForEstimates / 2m);
        result.TesterRoleHours = effectiveSysTest;
        result.PmRoleHours = effectivePM;
        result.CollaborationRoleHours = effectiveCollab;

        return result;
    }
}

/// <summary>
/// Inputs to the calculation engine — all values needed for the 10-step pipeline.
/// </summary>
public record CalculationInput
{
    public decimal DevelopmentHours { get; init; }
    public int ComponentCount { get; init; }
    public bool UseTestCasesForEstimate { get; init; }
    public decimal TestCasesSimple { get; init; }
    public decimal TestCasesMedium { get; init; }
    public decimal TestCasesComplex { get; init; }
    public decimal TestCasesVeryComplex { get; init; }
    public decimal TestCaseIterations { get; init; }
    public decimal PmEffortPercentage { get; init; }

    // Adjusted hours per task
    public decimal DevelopmentAdjustedHours { get; init; }
    public decimal SystemTestingAdjustedHours { get; init; }
    public decimal AnalysisAdjustedHours { get; init; }
    public decimal BusinessDesignAdjustedHours { get; init; }
    public decimal PromotionAdjustedHours { get; init; }
    public decimal BaSystemDocAdjustedHours { get; init; }
    public decimal ProductionValidationAdjustedHours { get; init; }
    public decimal ProjectManagementAdjustedHours { get; init; }

    // Per-type collaboration calculated hours
    public decimal WprsHours { get; init; }
    public decimal ClientMeetingsHours { get; init; }
    public decimal InternalMeetingsHours { get; init; }
    public decimal AutomationTestCollabHours { get; init; }
    public decimal ConsultantMentorHours { get; init; }

    // Per-type collaboration adjusted hours
    public decimal WprsAdjustedHours { get; init; }
    public decimal ClientMeetingsAdjustedHours { get; init; }
    public decimal InternalMeetingsAdjustedHours { get; init; }
    public decimal AutomationTestCollabAdjustedHours { get; init; }
    public decimal ConsultantMentorAdjustedHours { get; init; }

    // Additional
    public decimal TimeForEstimates { get; init; }
    public decimal TotalActualHours { get; init; }
}

/// <summary>
/// All calculated outputs from the 10-step pipeline.
/// </summary>
public record CalculationResult
{
    public decimal TotalDevelopmentHours { get; set; }
    public decimal SystemTestingHours { get; set; }
    public decimal AnalysisHours { get; set; }
    public decimal BusinessDesignHours { get; set; }
    public decimal PromotionHours { get; set; }
    public decimal BaSystemDocHours { get; set; }
    public decimal ProductionValidationHours { get; set; }
    public decimal ProjectManagementHours { get; set; }
    public decimal SubtotalHours { get; set; }
    public decimal GrandTotalHours { get; set; }
    public string TShirtSize { get; set; } = "—";

    // Per-task effective totals
    public decimal DevelopmentTotalHours { get; set; }
    public decimal SystemTestingTotalHours { get; set; }
    public decimal AnalysisTotalHours { get; set; }
    public decimal BusinessDesignTotalHours { get; set; }
    public decimal PromotionTotalHours { get; set; }
    public decimal BaSystemDocTotalHours { get; set; }
    public decimal ProductionValidationTotalHours { get; set; }
    public decimal ProjectManagementTotalHours { get; set; }
    public decimal CollaborationTotalHours { get; set; }

    // Per-type collaboration totals
    public decimal WprsTotalHours { get; set; }
    public decimal ClientMeetingsTotalHours { get; set; }
    public decimal InternalMeetingsTotalHours { get; set; }
    public decimal AutomationTestCollabTotalHours { get; set; }
    public decimal ConsultantMentorTotalHours { get; set; }

    // Role breakout
    public decimal BaRoleHours { get; set; }
    public decimal SeRoleHours { get; set; }
    public decimal TesterRoleHours { get; set; }
    public decimal PmRoleHours { get; set; }
    public decimal CollaborationRoleHours { get; set; }
}
