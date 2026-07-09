using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for the FinalEstimateViewModel — verifying data loading,
/// calculations against all 5 Excel reference files, happy paths, sad paths,
/// negative scenarios, and boundary conditions.
/// </summary>
public class FinalEstimateViewModel_Tests
{
    #region Helpers

    private MainViewModel CreateMainVm() => new();

    private ComponentRowViewModel AddComponent(MainViewModel vm, ComponentType type, ComponentSize size, ChangeType change, int count)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = count;
        return row;
    }

    private void ClearCollaboration(MainViewModel vm)
    {
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
    }

    private FinalEstimateViewModel LoadFinalEstimate(MainViewModel vm)
    {
        var finalVm = new FinalEstimateViewModel();
        finalVm.LoadFromMainViewModel(vm);
        return finalVm;
    }

    #endregion

    #region Happy Path — Basic Loading

    [Fact]
    public void FinalVm_LoadsProjectHeader_FromMainVm()
    {
        var vm = CreateMainVm();
        vm.ProjectName = "Test Project";
        vm.ChangeOrderId = "CO-12345";
        vm.ProjectDescription = "Test Description";
        vm.EstimatedBy = "John Doe";
        vm.ReviewedBy = "Jane Smith";

        var final = LoadFinalEstimate(vm);

        Assert.Equal("Test Project", final.ProjectName);
        Assert.Equal("CO-12345", final.ChangeOrderId);
        Assert.Equal("Test Description", final.ProjectDescription);
        Assert.Equal("John Doe", final.EstimatedBy);
        Assert.Equal("Jane Smith", final.ReviewedBy);
    }

    [Fact]
    public void FinalVm_LoadsComponentCount()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.Change, 2);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(2, final.ComponentCount);
    }

    [Fact]
    public void FinalVm_LoadsComponentRows()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        Assert.Single(final.ComponentRows);
        Assert.Equal("PowerBuilderWindows", final.ComponentRows[0].ComponentType);
        Assert.Equal("New", final.ComponentRows[0].ChangeType);
        Assert.Equal("Large", final.ComponentRows[0].Size);
        Assert.Equal(3, final.ComponentRows[0].Count);
        Assert.Equal(125.00m, final.ComponentRows[0].BaseHours);
        Assert.Equal(375.00m, final.ComponentRows[0].TotalHours);
    }

    [Fact]
    public void FinalVm_HasData_TrueWhenComponentsExist()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        Assert.True(final.HasData);
    }

    [Fact]
    public void FinalVm_HasData_FalseWhenEmpty()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        var final = LoadFinalEstimate(vm);

        Assert.False(final.HasData);
    }

    #endregion

    #region Happy Path — Derived Tasks (Calculated/Adjusted/Total)

    [Fact]
    public void FinalVm_DevelopmentBreakdown_Correct()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 2);
        vm.DevelopmentAdjustedHours = 10m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(250.00m, final.DevelopmentCalculated); // 125 * 2
        Assert.Equal(10m, final.DevelopmentAdjusted);
        Assert.Equal(260.00m, final.DevelopmentTotal);
    }

    [Fact]
    public void FinalVm_SystemTestingBreakdown_Standard30Percent()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 2);
        vm.SystemTestingAdjustedHours = 5m;

        var final = LoadFinalEstimate(vm);

        // SysTest = ROUNDUP((250+0) * 0.30, 2) = ROUNDUP(75, 2) = 75.00 (exact)
        // Wait: effectiveDev = 250 + 0 = 250, SysTest = ROUNDUP(250 * 0.30) = 75.00
        Assert.Equal(75.00m, final.SystemTestingCalculated);
        Assert.Equal(5m, final.SystemTestingAdjusted);
        Assert.Equal(80.00m, final.SystemTestingTotal);
    }

    [Fact]
    public void FinalVm_AllDerivedTasks_PopulatedCorrectly()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        // Dev = 125
        Assert.Equal(125.00m, final.DevelopmentCalculated);
        // SysTest = ROUNDUP(125 * 0.30) = ROUNDUP(37.5) = 37.50
        Assert.Equal(37.50m, final.SystemTestingCalculated);
        // Analysis = ROUNDUP((125 + 37.50) * 0.05) = ROUNDUP(8.125) = 8.13
        Assert.Equal(8.13m, final.AnalysisCalculated);
        // BizDesign = ROUNDUP((125 + 37.50) * 0.15) = ROUNDUP(24.375) = 24.38
        Assert.Equal(24.38m, final.BusinessDesignCalculated);
        // Promotion = ROUNDUP(125 * 0.05) = ROUNDUP(6.25) = 6.25
        Assert.Equal(6.25m, final.PromotionCalculated);
        // BaDoc = ROUNDUP(125 * 0.05) = 6.25
        Assert.Equal(6.25m, final.BaSystemDocCalculated);
        // ProdVal = ROUNDUP(37.50 * 0.20) = ROUNDUP(7.50) = 7.50
        Assert.Equal(7.50m, final.ProductionValidationCalculated);
    }

    #endregion

    #region Happy Path — PM Effort

    [Fact]
    public void FinalVm_PMEffort_MatchesPercentage()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        vm.PmEffortPercentage = 15m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(15m, final.PmEffortPercentage);
        Assert.True(final.PmEffortCalculated > 0);
        Assert.Equal(final.PmEffortCalculated, final.PmEffortTotal); // No adjustments
    }

    [Theory]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void FinalVm_PMEffort_AtVariousPercentages(int pm)
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.PmEffortPercentage = (decimal)pm;

        var final = LoadFinalEstimate(vm);

        Assert.Equal((decimal)pm, final.PmEffortPercentage);
        Assert.True(final.PmEffortCalculated > 0);
    }

    #endregion

    #region Happy Path — Collaboration Breakdown

    [Fact]
    public void FinalVm_CollaborationBreakdown_LoadsCorrectly()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.TaskName = "WPRs";
        wprs.NumberOfMeetings = 10;
        wprs.MeetingDurationMinutes = 30;
        wprs.NumberOfParticipants = 4;
        wprs.ParticipantPrepTimeMinutes = 30;

        var final = LoadFinalEstimate(vm);

        // WPRs = 10 * (30/60 + 30/60) * 4 = 10 * 1 * 4 = 40
        Assert.Equal(40m, final.WprsCalculated);
        Assert.Equal(40m, final.WprsTotal);
        Assert.Equal(40m, final.CollaborationTotalHours);
    }

    [Fact]
    public void FinalVm_CollaborationAdjusted_Included()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 5;
        wprs.MeetingDurationMinutes = 60;
        wprs.NumberOfParticipants = 3;
        wprs.ParticipantPrepTimeMinutes = 0;

        vm.WprsAdjustedHours = 10m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(15m, final.WprsCalculated); // 5 * 1 * 3
        Assert.Equal(10m, final.WprsAdjusted);
        Assert.Equal(25m, final.WprsTotal);
    }

    [Fact]
    public void FinalVm_CollaborationRows_LoadedFromSource()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var item = vm.CollaborationItems[^1];
        item.TaskName = "Client Meetings";
        item.CollabType = CollaborationType.ClientMeetings;
        item.NumberOfMeetings = 7;
        item.MeetingDurationMinutes = 60;
        item.NumberOfParticipants = 3;
        item.ParticipantPrepTimeMinutes = 60;

        var final = LoadFinalEstimate(vm);

        Assert.Single(final.CollaborationRows);
        Assert.Equal("Client Meetings", final.CollaborationRows[0].TaskName);
        Assert.Equal(7, final.CollaborationRows[0].NumberOfMeetings);
        Assert.Equal(42m, final.CollaborationRows[0].TotalHours); // 7 * 2 * 3
    }

    #endregion

    #region Happy Path — Totals

    [Fact]
    public void FinalVm_GrandTotal_MatchesMainVm()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 2);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(vm.GrandTotalHours, final.GrandTotalHours);
        Assert.Equal(vm.SubtotalHours, final.SubtotalHours);
        Assert.Equal(vm.TShirtSize, final.TShirtSize);
    }

    [Fact]
    public void FinalVm_SubtotalIncludesTimeForEstimates()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.TimeForEstimates = 50m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(50m, final.TimeForEstimates);
        Assert.Equal(vm.SubtotalHours, final.SubtotalHours);
    }

    [Fact]
    public void FinalVm_SubtotalIncludesActualHours()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.TotalActualHours = 100m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(100m, final.TotalActualHours);
        Assert.Equal(vm.SubtotalHours, final.SubtotalHours);
    }

    #endregion

    #region Happy Path — Role Breakout

    [Fact]
    public void FinalVm_RoleBreakout_MatchesMainVm()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        vm.TimeForEstimates = 50m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(vm.BaRoleHours, final.BaRoleHours);
        Assert.Equal(vm.SeRoleHours, final.SeRoleHours);
        Assert.Equal(vm.TesterRoleHours, final.TesterRoleHours);
        Assert.Equal(vm.PmRoleHours, final.PmRoleHours);
        Assert.Equal(vm.CollaborationRoleHours, final.CollaborationRoleHours);
    }

    [Fact]
    public void FinalVm_TotalRoleHours_SumOfAllRoles()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        decimal expected = final.BaRoleHours + final.SeRoleHours + final.TesterRoleHours
                         + final.PmRoleHours + final.CollaborationRoleHours;
        Assert.Equal(expected, final.TotalRoleHours);
    }

    #endregion

    #region Happy Path — Assumptions

    [Fact]
    public void FinalVm_Assumptions_LoadedFromSource()
    {
        var vm = CreateMainVm();
        vm.SeAssumptions = "SE will be proficient";
        vm.BaAssumptions = "BA has domain knowledge";
        vm.CollaborationAssumptions = "Weekly meetings";
        vm.GeneralAssumptions = "No scope changes";

        var final = LoadFinalEstimate(vm);

        Assert.Equal("SE will be proficient", final.SeAssumptions);
        Assert.Equal("BA has domain knowledge", final.BaAssumptions);
        Assert.Equal("Weekly meetings", final.CollaborationAssumptions);
        Assert.Equal("No scope changes", final.GeneralAssumptions);
    }

    [Fact]
    public void FinalVm_AdjustedHoursComments_Loaded()
    {
        var vm = CreateMainVm();
        vm.AdjustedHoursComments = "Reduced due to reusable code";

        var final = LoadFinalEstimate(vm);

        Assert.Equal("Reduced due to reusable code", final.AdjustedHoursComments);
    }

    #endregion

    #region Happy Path — Test Case Info

    [Fact]
    public void FinalVm_TestCaseInfo_LoadedCorrectly()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 50;
        vm.TestCasesMedium = 30;
        vm.TestCasesComplex = 20;
        vm.TestCasesVeryComplex = 10;
        vm.TestCaseIterations = 2m;

        var final = LoadFinalEstimate(vm);

        Assert.True(final.UseTestCases);
        Assert.Equal(50m, final.TestCasesSimple);
        Assert.Equal(30m, final.TestCasesMedium);
        Assert.Equal(20m, final.TestCasesComplex);
        Assert.Equal(10m, final.TestCasesVeryComplex);
        Assert.Equal(2m, final.TestCaseIterations);
    }

    [Fact]
    public void FinalVm_SystemTestingMethod_TestCases()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCasesComplex = 3;
        vm.TestCasesVeryComplex = 1;
        vm.TestCaseIterations = 1.5m;

        var final = LoadFinalEstimate(vm);

        Assert.Contains("Test Cases", final.SystemTestingMethod);
        Assert.Contains("S:10", final.SystemTestingMethod);
    }

    [Fact]
    public void FinalVm_SystemTestingMethod_Standard()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.UseTestCasesForEstimate = false;

        var final = LoadFinalEstimate(vm);

        Assert.Equal("Standard (30% of Development)", final.SystemTestingMethod);
    }

    #endregion

    #region Excel Verification — CO 23327 002

    [Fact]
    public void FinalVm_CO23327_002_FullPipeline_GrandTotal5262()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        // Components from CO 23327 002
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 53);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2.5m;
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // Collaboration
        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 20;
        wprs.MeetingDurationMinutes = 15;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 7;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 3;
        client.ParticipantPrepTimeMinutes = 60;

        vm.AddCollaborationItemCommand.Execute(null);
        var intern = vm.CollaborationItems[^1];
        intern.CollabType = CollaborationType.InternalMeetings;
        intern.NumberOfMeetings = 3;
        intern.MeetingDurationMinutes = 15;
        intern.NumberOfParticipants = 5;
        intern.ParticipantPrepTimeMinutes = 60;

        var final = LoadFinalEstimate(vm);

        // Verify all values match Excel
        Assert.Equal(596.5625m, final.DevelopmentCalculated);
        Assert.Equal(2517.46m, final.SystemTestingCalculated);
        Assert.Equal(125.00m, final.WprsCalculated);
        Assert.Equal(42.00m, final.ClientMeetingsCalculated);
        Assert.Equal(18.75m, final.InternalMeetingsCalculated);
        Assert.Equal(1.17m, final.BaSystemDocAdjusted);
        Assert.Equal(129m, final.TimeForEstimates);
        Assert.Equal(5262m, final.GrandTotalHours);
        Assert.Equal("XL5", final.TShirtSize);
        Assert.Equal(3, final.ComponentCount);
    }

    [Fact]
    public void FinalVm_CO23327_002_RoleBreakout_AllPositive()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 53);
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesComplex = 75;
        vm.TestCaseIterations = 2.5m;
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        var final = LoadFinalEstimate(vm);

        Assert.True(final.BaRoleHours > 0);
        Assert.True(final.SeRoleHours > 0);
        Assert.True(final.TesterRoleHours > 0);
        Assert.True(final.PmRoleHours > 0);
    }

    #endregion

    #region Excel Verification — CO 23810 002

    [Fact]
    public void FinalVm_CO23810_002_DevelopmentTotal()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // Dev = 60.63*2 + 85 + 115*3 = 121.26 + 85 + 345 = 551.26
        Assert.Equal(551.26m, final.DevelopmentCalculated);
        Assert.Equal(551.26m, final.DevelopmentTotal);
    }

    [Fact]
    public void FinalVm_CO23810_002_SystemTesting30Percent()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // SysTest = ROUNDUP(551.26 * 0.30, 2) = ROUNDUP(165.378) = 165.38
        Assert.Equal(165.38m, final.SystemTestingCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_Analysis()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // Analysis = ROUNDUP((551.26 + 165.38) * 0.05) = ROUNDUP(35.832) = 35.84
        Assert.Equal(35.84m, final.AnalysisCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_BusinessDesign()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // BizDesign = ROUNDUP((551.26 + 165.38) * 0.15) = ROUNDUP(107.496) = 107.50
        Assert.Equal(107.50m, final.BusinessDesignCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_Promotion()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // Promotion = ROUNDUP(551.26 * 0.05) = ROUNDUP(27.563) = 27.57
        Assert.Equal(27.57m, final.PromotionCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_BaSystemDoc()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // BaDoc = ROUNDUP(551.26 * 0.05) = 27.57
        Assert.Equal(27.57m, final.BaSystemDocCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_ProductionValidation()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        // ProdVal = ROUNDUP(165.38 * 0.20) = ROUNDUP(33.076) = 33.08
        Assert.Equal(33.08m, final.ProductionValidationCalculated);
    }

    [Fact]
    public void FinalVm_CO23810_002_GrandTotal()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        var final = LoadFinalEstimate(vm);

        Assert.True(final.GrandTotalHours > 0);
        Assert.Equal(vm.GrandTotalHours, final.GrandTotalHours);
    }

    #endregion

    #region Excel Verification — CO 23810 003

    [Fact]
    public void FinalVm_CO23810_003_SingleLargeComponent()
    {
        // CO 23810 003 scenario with a single large component
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);

        var final = LoadFinalEstimate(vm);

        // Dev = 294.40 * 2 = 588.80
        Assert.Equal(588.80m, final.DevelopmentCalculated);
        // SysTest = ROUNDUP(588.80 * 0.30) = ROUNDUP(176.64) = 176.64
        Assert.Equal(176.64m, final.SystemTestingCalculated);
    }

    #endregion

    #region Excel Verification — CO 23869 002

    [Fact]
    public void FinalVm_CO23869_002_MixedComponents()
    {
        // CO 23869 002 with varied component types
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 1);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change, 5);

        var final = LoadFinalEstimate(vm);

        // Dev = 90*2 + 100 + 4.6875*5 = 180 + 100 + 23.4375 = 303.4375
        Assert.Equal(303.4375m, final.DevelopmentCalculated);
        Assert.True(final.GrandTotalHours > 0);
    }

    #endregion

    #region Excel Verification — CO 24407

    [Fact]
    public void FinalVm_CO24407_LargeProject_DevelopmentTotal()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        var final = LoadFinalEstimate(vm);

        decimal expected = 125m * 3 + 100m * 5 + 294.40m * 2 + 4.0625m * 10 + 31.875m * 4 + 51m * 6;
        Assert.Equal(expected, final.DevelopmentCalculated);
    }

    [Fact]
    public void FinalVm_CO24407_LargeProject_AllDerivedPositive()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        var final = LoadFinalEstimate(vm);

        Assert.True(final.SystemTestingCalculated > 0);
        Assert.True(final.AnalysisCalculated > 0);
        Assert.True(final.BusinessDesignCalculated > 0);
        Assert.True(final.PromotionCalculated > 0);
        Assert.True(final.BaSystemDocCalculated > 0);
        Assert.True(final.ProductionValidationCalculated > 0);
        Assert.True(final.PmEffortCalculated > 0);
    }

    [Fact]
    public void FinalVm_CO24407_LargeProject_WithFullCollaboration()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);

        // Full collaboration
        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 15;
        wprs.MeetingDurationMinutes = 30;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 10;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 4;
        client.ParticipantPrepTimeMinutes = 30;

        var final = LoadFinalEstimate(vm);

        // WPRs = 15 * (0.5 + 1) * 5 = 15 * 1.5 * 5 = 112.5
        Assert.Equal(112.5m, final.WprsCalculated);
        // Client = 10 * (1 + 0.5) * 4 = 10 * 1.5 * 4 = 60
        Assert.Equal(60m, final.ClientMeetingsCalculated);
        Assert.Equal(172.5m, final.CollaborationTotalHours);
    }

    #endregion

    #region Excel Verification — PROMISe Estimating Tool

    [Fact]
    public void FinalVm_PROMISe_SupportModules_BaseHours()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 10);

        var final = LoadFinalEstimate(vm);

        // Dev = 9.6875 * 10 = 96.875
        Assert.Equal(96.875m, final.DevelopmentCalculated);
    }

    [Fact]
    public void FinalVm_PROMISe_AllComponentTypes_DevTotal()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        // Exercise all component types
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New, 1);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        decimal expected = 25m + 17m + 46m + 5m + 5.9375m + 20m + 50m + 15m;
        // MISC Small New
        expected += WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Small, ChangeType.New);

        Assert.Equal(expected, final.DevelopmentCalculated);
        Assert.Equal(9, final.ComponentCount);
    }

    #endregion

    #region Negative Path — Empty/Zero Data

    [Fact]
    public void FinalVm_NoComponents_AllZeros()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(0m, final.DevelopmentCalculated);
        Assert.Equal(0m, final.SystemTestingCalculated);
        Assert.Equal(0m, final.AnalysisCalculated);
        Assert.Equal(0m, final.BusinessDesignCalculated);
        Assert.Equal(0m, final.PromotionCalculated);
        Assert.Equal(0m, final.BaSystemDocCalculated);
        Assert.Equal(0m, final.ProductionValidationCalculated);
        Assert.Equal(0m, final.PmEffortCalculated);
        Assert.Equal(0m, final.GrandTotalHours);
        Assert.Equal("—", final.TShirtSize);
    }

    [Fact]
    public void FinalVm_ComponentWithZeroCount_ZeroDev()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        var row = AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 0);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(0m, final.DevelopmentCalculated);
    }

    [Fact]
    public void FinalVm_NoCollaboration_ZeroCollabHours()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(0m, final.WprsCalculated);
        Assert.Equal(0m, final.ClientMeetingsCalculated);
        Assert.Equal(0m, final.InternalMeetingsCalculated);
        Assert.Equal(0m, final.AutomationTestCalculated);
        Assert.Equal(0m, final.ConsultantMentorCalculated);
        Assert.Equal(0m, final.CollaborationTotalHours);
    }

    #endregion

    #region Negative Path — Negative Adjustments

    [Fact]
    public void FinalVm_NegativeDevAdjustment_DecreasesTotal()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        vm.DevelopmentAdjustedHours = -50m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(125m, final.DevelopmentCalculated);
        Assert.Equal(-50m, final.DevelopmentAdjusted);
        Assert.Equal(75m, final.DevelopmentTotal);
    }

    [Fact]
    public void FinalVm_NegativeAdjustments_CascadeDownstream()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);

        // Without adjustment: SysTest = ROUNDUP(125 * 0.30) = 37.50
        decimal sysTestBefore = vm.SystemTestingHours;
        vm.DevelopmentAdjustedHours = -50m;
        // With adjustment: effectiveDev = 75, SysTest = ROUNDUP(75 * 0.30) = 22.50
        decimal sysTestAfter = vm.SystemTestingHours;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(22.50m, final.SystemTestingCalculated);
        Assert.True(final.SystemTestingCalculated < sysTestBefore);
    }

    [Fact]
    public void FinalVm_AllNegativeAdjustments_StillLoads()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.DevelopmentAdjustedHours = -10m;
        vm.SystemTestingAdjustedHours = -5m;
        vm.AnalysisAdjustedHours = -2m;
        vm.BusinessDesignAdjustedHours = -3m;
        vm.PromotionAdjustedHours = -1m;
        vm.BaSystemDocAdjustedHours = -1m;
        vm.ProductionValidationAdjustedHours = -1m;
        vm.ProjectManagementAdjustedHours = -5m;

        var final = LoadFinalEstimate(vm);

        // Should load without error, all fields populated
        Assert.Equal(-10m, final.DevelopmentAdjusted);
        Assert.Equal(-5m, final.SystemTestingAdjusted);
        Assert.Equal(-2m, final.AnalysisAdjusted);
    }

    [Fact]
    public void FinalVm_NegativeCollabAdjustment()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 5;
        wprs.MeetingDurationMinutes = 60;
        wprs.NumberOfParticipants = 3;
        wprs.ParticipantPrepTimeMinutes = 0;

        vm.WprsAdjustedHours = -5m;

        var final = LoadFinalEstimate(vm);

        Assert.Equal(15m, final.WprsCalculated);
        Assert.Equal(-5m, final.WprsAdjusted);
        Assert.Equal(10m, final.WprsTotal);
    }

    #endregion

    #region Boundary — T-Shirt Size Boundaries

    [Theory]
    [InlineData(25, "Small")]       // 0-99 = Small
    [InlineData(100, "Medium")]     // 100-299
    [InlineData(300, "Large")]      // 300-749
    [InlineData(750, "X-Large")]    // 750-999
    [InlineData(1000, "XL1")]       // 1000-1999
    [InlineData(2000, "XL2")]       // 2000-2999
    [InlineData(3000, "XL3")]       // 3000-3999
    [InlineData(5000, "XL5")]       // 5000-5999
    public void FinalVm_TShirtSize_AtBoundary(int grandTotal, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize((decimal)grandTotal));
    }

    #endregion

    #region Boundary — Very Large Estimates

    [Fact]
    public void FinalVm_VeryLargeProject_HighComponentCount()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);

        // 20 large PB Windows New + 10 large Programs New
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 20);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 10);

        var final = LoadFinalEstimate(vm);

        // Dev = 125*20 + 294.40*10 = 2500 + 2944 = 5444
        Assert.Equal(5444m, final.DevelopmentCalculated);
        // Grand total with all derived tasks will be very high (XL8 range)
        Assert.True(final.GrandTotalHours >= 8000m);
    }

    #endregion

    #region Boundary — Minimal Single Component

    [Fact]
    public void FinalVm_MinimalComponent_SmallestBaseHours()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        // SupportModules Small Change = 4.0625
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 1);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(4.0625m, final.DevelopmentCalculated);
        Assert.True(final.GrandTotalHours > 0);
        Assert.Equal("Small", final.TShirtSize);
    }

    #endregion

    #region Reload Test — Multiple Loads Replace Data

    [Fact]
    public void FinalVm_ReloadFromDifferentSource_ReplacesData()
    {
        var vm1 = CreateMainVm();
        ClearCollaboration(vm1);
        AddComponent(vm1, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm1.ProjectName = "Project A";

        var vm2 = CreateMainVm();
        ClearCollaboration(vm2);
        AddComponent(vm2, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 5);
        vm2.ProjectName = "Project B";

        var finalVm = new FinalEstimateViewModel();
        finalVm.LoadFromMainViewModel(vm1);
        Assert.Equal("Project A", finalVm.ProjectName);

        finalVm.LoadFromMainViewModel(vm2);
        Assert.Equal("Project B", finalVm.ProjectName);
        Assert.Equal(625m, finalVm.DevelopmentCalculated); // 125*5
    }

    [Fact]
    public void FinalVm_Reload_ClearsOldComponentRows()
    {
        var vm1 = CreateMainVm();
        ClearCollaboration(vm1);
        AddComponent(vm1, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm1, ComponentType.MISC, ComponentSize.Medium, ChangeType.New, 1);
        AddComponent(vm1, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 1);

        var vm2 = CreateMainVm();
        ClearCollaboration(vm2);
        AddComponent(vm2, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);

        var finalVm = new FinalEstimateViewModel();
        finalVm.LoadFromMainViewModel(vm1);
        Assert.Equal(3, finalVm.ComponentRows.Count);

        finalVm.LoadFromMainViewModel(vm2);
        Assert.Single(finalVm.ComponentRows);
    }

    #endregion

    #region Integration — Full Pipeline End-to-End

    [Fact]
    public void FinalVm_FullPipeline_AllFieldsPopulated()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        vm.ProjectName = "Integration Test";
        vm.ChangeOrderId = "CO-99999";
        vm.ProjectDescription = "Full pipeline test";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        vm.PmEffortPercentage = 12m;
        vm.SeAssumptions = "Test SE";
        vm.BaAssumptions = "Test BA";
        vm.GeneralAssumptions = "Test General";

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 3);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.TaskName = "WPRs";
        wprs.NumberOfMeetings = 10;
        wprs.MeetingDurationMinutes = 30;
        wprs.NumberOfParticipants = 3;
        wprs.ParticipantPrepTimeMinutes = 30;

        vm.DevelopmentAdjustedHours = 5m;
        vm.TimeForEstimates = 20m;
        vm.TotalActualHours = 50m;

        var final = LoadFinalEstimate(vm);

        // Header
        Assert.Equal("Integration Test", final.ProjectName);
        Assert.Equal("CO-99999", final.ChangeOrderId);
        Assert.Equal(12m, final.PmEffortPercentage);

        // Components
        Assert.Equal(2, final.ComponentCount);
        Assert.Equal(2, final.ComponentRows.Count);

        // Calculations populated
        Assert.True(final.DevelopmentCalculated > 0);
        Assert.True(final.SystemTestingCalculated > 0);
        Assert.True(final.GrandTotalHours > 0);

        // Adjustments
        Assert.Equal(5m, final.DevelopmentAdjusted);
        Assert.Equal(20m, final.TimeForEstimates);
        Assert.Equal(50m, final.TotalActualHours);

        // Role breakout
        Assert.True(final.BaRoleHours > 0);
        Assert.True(final.SeRoleHours > 0);
        Assert.True(final.TotalRoleHours > 0);

        // Assumptions
        Assert.Equal("Test SE", final.SeAssumptions);
        Assert.Equal("Test BA", final.BaAssumptions);
    }

    #endregion

    #region Test Cases Formula — Excel Verification

    [Fact]
    public void FinalVm_TestCasesFormula_CO23327_MatchesExcel()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 53);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesComplex = 75;
        vm.TestCaseIterations = 2.5m;

        var final = LoadFinalEstimate(vm);

        // Row 31: Simple=2.1925, Complex=8.76
        // mainHours = 125*2.1925 + 75*8.76 = 274.0625 + 657 = 931.0625
        // Row 32: Simple=1.5675, Complex=7.51
        // defectHours = (125*1.5675 + 75*7.51) * 0.1 = (195.9375 + 563.25) * 0.1 = 75.91875
        // Total = (931.0625 + 75.91875) * 2.5 = 1006.98125 * 2.5 = 2517.453125
        // RoundUp = 2517.46
        Assert.Equal(2517.46m, final.SystemTestingCalculated);
    }

    [Fact]
    public void FinalVm_TestCasesFormula_WithIterations1()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 5;
        vm.TestCasesComplex = 3;
        vm.TestCasesVeryComplex = 1;
        vm.TestCaseIterations = 1m;

        var final = LoadFinalEstimate(vm);

        // Verify formula gives expected result
        const decimal r31S = 2.1925m, r31M = 4.065m, r31C = 8.76m, r31VC = 14.38m;
        const decimal r32S = 1.5675m, r32M = 3.44m, r32C = 7.51m, r32VC = 13.13m;
        decimal main = 10m * r31S + 5m * r31M + 3m * r31C + 1m * r31VC;
        decimal defect = (10m * r32S + 5m * r32M + 3m * r32C + 1m * r32VC) * 0.1m;
        decimal expected = MainViewModel.RoundUp((main + defect) * 1m);

        Assert.Equal(expected, final.SystemTestingCalculated);
    }

    [Fact]
    public void FinalVm_TestCasesFormula_IterationsZero_UsesMin1()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCaseIterations = 0m; // Math.Max(1, 0) = 1

        var final = LoadFinalEstimate(vm);

        // With 0 iterations, uses max(1, 0) = 1
        Assert.True(final.SystemTestingCalculated > 0);
    }

    #endregion

    #region Multiple Loads Stability

    [Fact]
    public void FinalVm_MultipleLoads_StableResults()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);

        var final = new FinalEstimateViewModel();

        // Load multiple times — results should be identical
        final.LoadFromMainViewModel(vm);
        decimal gt1 = final.GrandTotalHours;

        final.LoadFromMainViewModel(vm);
        decimal gt2 = final.GrandTotalHours;

        final.LoadFromMainViewModel(vm);
        decimal gt3 = final.GrandTotalHours;

        Assert.Equal(gt1, gt2);
        Assert.Equal(gt2, gt3);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void FinalVm_EmptyProjectName_LoadsAsEmpty()
    {
        var vm = CreateMainVm();
        vm.ProjectName = "";

        var final = LoadFinalEstimate(vm);

        Assert.Equal("", final.ProjectName);
    }

    [Fact]
    public void FinalVm_ActualHoursAsOfDate_Null_Handled()
    {
        var vm = CreateMainVm();
        vm.ActualHoursAsOfDate = null;

        var final = LoadFinalEstimate(vm);

        Assert.Null(final.ActualHoursAsOfDate);
    }

    [Fact]
    public void FinalVm_ActualHoursAsOfDate_Set_Loaded()
    {
        var vm = CreateMainVm();
        vm.ActualHoursAsOfDate = new DateTime(2025, 6, 15);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(new DateTime(2025, 6, 15), final.ActualHoursAsOfDate);
    }

    [Fact]
    public void FinalVm_ComponentLineNumbers_Sequential()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Medium, ChangeType.New, 1);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Small, ChangeType.New, 1);

        var final = LoadFinalEstimate(vm);

        Assert.Equal(1, final.ComponentRows[0].LineNumber);
        Assert.Equal(2, final.ComponentRows[1].LineNumber);
        Assert.Equal(3, final.ComponentRows[2].LineNumber);
    }

    [Fact]
    public void FinalVm_EstimateDate_IsCurrentDate()
    {
        var vm = CreateMainVm();
        var final = LoadFinalEstimate(vm);

        Assert.Equal(DateTime.Now.Date, final.EstimateDate.Date);
    }

    #endregion

    #region Cascading Calculation Verification

    [Fact]
    public void FinalVm_AdjustmentCascade_SystemTestingDependsOnEffectiveDev()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        // Dev = 125, adjustment = +25, effectiveDev = 150
        vm.DevelopmentAdjustedHours = 25m;

        var final = LoadFinalEstimate(vm);

        // SysTest = ROUNDUP(150 * 0.30) = ROUNDUP(45) = 45.00
        Assert.Equal(45.00m, final.SystemTestingCalculated);
    }

    [Fact]
    public void FinalVm_AdjustmentCascade_AnalysisDependsOnEffectiveDevAndSysTest()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        vm.DevelopmentAdjustedHours = 25m; // effectiveDev = 150
        vm.SystemTestingAdjustedHours = 10m; // effectiveSysTest = 45 + 10 = 55

        var final = LoadFinalEstimate(vm);

        // Analysis = ROUNDUP((150 + 55) * 0.05) = ROUNDUP(10.25) = 10.25
        Assert.Equal(10.25m, final.AnalysisCalculated);
    }

    [Fact]
    public void FinalVm_PMEffort_BasedOnAllEffectiveTasks()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        vm.PmEffortPercentage = 10m;

        var final = LoadFinalEstimate(vm);

        // PM should be 10% of the sum of all effective tasks
        decimal allEffective = final.DevelopmentTotal + final.SystemTestingTotal
                             + final.AnalysisTotal + final.BusinessDesignTotal
                             + final.PromotionTotal + final.BaSystemDocTotal
                             + final.ProductionValidationTotal;
        decimal expectedPM = MainViewModel.RoundUp(allEffective * 0.10m);
        Assert.Equal(expectedPM, final.PmEffortCalculated);
    }

    #endregion

    #region Consistency — Final Matches Initial

    [Fact]
    public void FinalVm_GrandTotal_AlwaysMatchesMainVm_SmallProject()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 1);

        var final = LoadFinalEstimate(vm);
        Assert.Equal(vm.GrandTotalHours, final.GrandTotalHours);
    }

    [Fact]
    public void FinalVm_GrandTotal_AlwaysMatchesMainVm_LargeProject()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 10);

        var final = LoadFinalEstimate(vm);
        Assert.Equal(vm.GrandTotalHours, final.GrandTotalHours);
    }

    [Fact]
    public void FinalVm_GrandTotal_AlwaysMatchesMainVm_WithAdjustments()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        vm.DevelopmentAdjustedHours = 50m;
        vm.SystemTestingAdjustedHours = -10m;
        vm.TimeForEstimates = 30m;
        vm.TotalActualHours = 80m;

        var final = LoadFinalEstimate(vm);
        Assert.Equal(vm.GrandTotalHours, final.GrandTotalHours);
    }

    [Fact]
    public void FinalVm_SubtotalHours_AlwaysMatchesMainVm()
    {
        var vm = CreateMainVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        vm.PmEffortPercentage = 18m;

        var final = LoadFinalEstimate(vm);
        Assert.Equal(vm.SubtotalHours, final.SubtotalHours);
    }

    #endregion
}
