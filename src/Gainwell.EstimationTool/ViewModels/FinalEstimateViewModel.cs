using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.Services;

namespace Gainwell.EstimationTool.ViewModels;

/// <summary>
/// ViewModel for the Final Estimate tab — a read-only consolidated summary
/// of the complete estimation, matching the Excel "Final Estimate" sheet.
/// Pulls all data from the MainViewModel and presents it in a presentation-ready format.
/// </summary>
public partial class FinalEstimateViewModel : ObservableObject
{
    // === Project Header (read-only display) ===
    [ObservableProperty] private string _projectName = string.Empty;
    [ObservableProperty] private string _changeOrderId = string.Empty;
    [ObservableProperty] private string _projectDescription = string.Empty;
    [ObservableProperty] private string _estimatedBy = string.Empty;
    [ObservableProperty] private string _reviewedBy = string.Empty;
    [ObservableProperty] private DateTime _estimateDate = DateTime.Now;

    // === Component Summary ===
    [ObservableProperty] private int _componentCount;
    [ObservableProperty] private decimal _totalDevelopmentHours;

    // === Derived Task Calculations (Calculated | Adjusted | Total) ===
    [ObservableProperty] private decimal _developmentCalculated;
    [ObservableProperty] private decimal _developmentAdjusted;
    [ObservableProperty] private decimal _developmentTotal;

    [ObservableProperty] private decimal _systemTestingCalculated;
    [ObservableProperty] private decimal _systemTestingAdjusted;
    [ObservableProperty] private decimal _systemTestingTotal;

    [ObservableProperty] private decimal _analysisCalculated;
    [ObservableProperty] private decimal _analysisAdjusted;
    [ObservableProperty] private decimal _analysisTotal;

    [ObservableProperty] private decimal _businessDesignCalculated;
    [ObservableProperty] private decimal _businessDesignAdjusted;
    [ObservableProperty] private decimal _businessDesignTotal;

    [ObservableProperty] private decimal _promotionCalculated;
    [ObservableProperty] private decimal _promotionAdjusted;
    [ObservableProperty] private decimal _promotionTotal;

    [ObservableProperty] private decimal _baSystemDocCalculated;
    [ObservableProperty] private decimal _baSystemDocAdjusted;
    [ObservableProperty] private decimal _baSystemDocTotal;

    [ObservableProperty] private decimal _productionValidationCalculated;
    [ObservableProperty] private decimal _productionValidationAdjusted;
    [ObservableProperty] private decimal _productionValidationTotal;

    [ObservableProperty] private decimal _pmEffortCalculated;
    [ObservableProperty] private decimal _pmEffortAdjusted;
    [ObservableProperty] private decimal _pmEffortTotal;
    [ObservableProperty] private decimal _pmEffortPercentage;

    // === Collaboration Breakdown ===
    [ObservableProperty] private decimal _wprsCalculated;
    [ObservableProperty] private decimal _wprsAdjusted;
    [ObservableProperty] private decimal _wprsTotal;

    [ObservableProperty] private decimal _clientMeetingsCalculated;
    [ObservableProperty] private decimal _clientMeetingsAdjusted;
    [ObservableProperty] private decimal _clientMeetingsTotal;

    [ObservableProperty] private decimal _internalMeetingsCalculated;
    [ObservableProperty] private decimal _internalMeetingsAdjusted;
    [ObservableProperty] private decimal _internalMeetingsTotal;

    [ObservableProperty] private decimal _automationTestCalculated;
    [ObservableProperty] private decimal _automationTestAdjusted;
    [ObservableProperty] private decimal _automationTestTotal;

    [ObservableProperty] private decimal _consultantMentorCalculated;
    [ObservableProperty] private decimal _consultantMentorAdjusted;
    [ObservableProperty] private decimal _consultantMentorTotal;

    [ObservableProperty] private decimal _collaborationTotalHours;

    // === Additional Items ===
    [ObservableProperty] private decimal _timeForEstimates;
    [ObservableProperty] private decimal _totalActualHours;
    [ObservableProperty] private DateTime? _actualHoursAsOfDate;

    // === Totals ===
    [ObservableProperty] private decimal _subtotalHours;
    [ObservableProperty] private decimal _grandTotalHours;
    [ObservableProperty] private string _tShirtSize = "—";

    // === Role Breakout ===
    [ObservableProperty] private decimal _baRoleHours;
    [ObservableProperty] private decimal _seRoleHours;
    [ObservableProperty] private decimal _testerRoleHours;
    [ObservableProperty] private decimal _pmRoleHours;
    [ObservableProperty] private decimal _collaborationRoleHours;
    [ObservableProperty] private decimal _totalRoleHours;

    // === Assumptions ===
    [ObservableProperty] private string _seAssumptions = string.Empty;
    [ObservableProperty] private string _baAssumptions = string.Empty;
    [ObservableProperty] private string _collaborationAssumptions = string.Empty;
    [ObservableProperty] private string _generalAssumptions = string.Empty;
    [ObservableProperty] private string _adjustedHoursComments = string.Empty;

    // === Test Case Info ===
    [ObservableProperty] private bool _useTestCases;
    [ObservableProperty] private decimal _testCasesSimple;
    [ObservableProperty] private decimal _testCasesMedium;
    [ObservableProperty] private decimal _testCasesComplex;
    [ObservableProperty] private decimal _testCasesVeryComplex;
    [ObservableProperty] private decimal _testCaseIterations;

    // === Component Details (for the component summary table) ===
    public ObservableCollection<FinalEstimateComponentRow> ComponentRows { get; } = new();

    // === Collaboration Details ===
    public ObservableCollection<FinalEstimateCollaborationRow> CollaborationRows { get; } = new();

    /// <summary>
    /// Loads all data from the MainViewModel to produce the Final Estimate summary.
    /// </summary>
    public void LoadFromMainViewModel(InitialEstimateViewModel source)
    {
        ProjectMapper.MapToFinalEstimate(source, this);

        // Assumptions
        SeAssumptions = source.SeAssumptions;
        BaAssumptions = source.BaAssumptions;
        CollaborationAssumptions = source.CollaborationAssumptions;
        GeneralAssumptions = source.GeneralAssumptions;
        AdjustedHoursComments = source.AdjustedHoursComments;

        // Test case info
        UseTestCases = source.UseTestCasesForEstimate;
        TestCasesSimple = source.TestCasesSimple;
        TestCasesMedium = source.TestCasesMedium;
        TestCasesComplex = source.TestCasesComplex;
        TestCasesVeryComplex = source.TestCasesVeryComplex;
        TestCaseIterations = source.TestCaseIterations;

        // Component details
        ComponentRows.Clear();
        int lineNum = 1;
        foreach (var c in source.Components)
        {
            ComponentRows.Add(new FinalEstimateComponentRow
            {
                LineNumber = lineNum++,
                RequirementId = c.RequirementId,
                ComponentType = c.ComponentType.ToString(),
                ChangeType = c.ChangeType.ToString(),
                Size = c.Size.ToString(),
                Count = c.Count,
                BaseHours = c.BaseHoursPerUnit,
                TotalHours = c.TotalHours
            });
        }

        // Collaboration details
        CollaborationRows.Clear();
        foreach (var item in source.CollaborationItems)
        {
            CollaborationRows.Add(new FinalEstimateCollaborationRow
            {
                TaskName = item.TaskName,
                NumberOfMeetings = item.NumberOfMeetings,
                DurationMinutes = item.MeetingDurationMinutes,
                Participants = item.NumberOfParticipants,
                PrepTimeMinutes = item.ParticipantPrepTimeMinutes,
                TotalHours = item.TotalHours
            });
        }
    }

    /// <summary>
    /// Checks whether the estimate has enough data to display.
    /// </summary>
    public bool HasData => ComponentCount > 0 || GrandTotalHours > 0;

    /// <summary>
    /// System Testing method label for display.
    /// </summary>
    public string SystemTestingMethod => UseTestCases
        ? $"Test Cases (S:{TestCasesSimple} M:{TestCasesMedium} C:{TestCasesComplex} VC:{TestCasesVeryComplex} x{TestCaseIterations})"
        : "Standard (30% of Development)";
}

/// <summary>
/// Read-only row for the component summary table in the Final Estimate.
/// </summary>
public class FinalEstimateComponentRow
{
    public int LineNumber { get; set; }
    public string RequirementId { get; set; } = string.Empty;
    public string ComponentType { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal BaseHours { get; set; }
    public decimal TotalHours { get; set; }
}

/// <summary>
/// Read-only row for the collaboration summary table in the Final Estimate.
/// </summary>
public class FinalEstimateCollaborationRow
{
    public string TaskName { get; set; } = string.Empty;
    public int NumberOfMeetings { get; set; }
    public int DurationMinutes { get; set; }
    public int Participants { get; set; }
    public int PrepTimeMinutes { get; set; }
    public decimal TotalHours { get; set; }
}
