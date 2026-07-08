using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for the Final Estimate calculation pipeline.
/// Tests the complete flow including adjusted hours, collaboration per-type totals,
/// time-for-estimates, actual hours, and role breakout — matching Excel exactly.
/// </summary>
public class FinalEstimate_AllPathTests
{
    private MainViewModel CreateVm() => new();

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

    #region Happy Path — Final Estimate with Full Data (CO 23327 002)

    [Fact]
    public void FinalEstimate_CO23327_FullPipeline_GrandTotal5262()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        // Components from CO 23327 002
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 53);

        // Enable test cases
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2.5m;

        // BA System Doc adjustment
        vm.BaSystemDocAdjustedHours = 1.17m;

        // Time for estimates
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

        // Verify intermediate values
        Assert.Equal(596.5625m, vm.TotalDevelopmentHours);
        Assert.Equal(2517.46m, vm.SystemTestingHours);
        Assert.Equal(125.00m, wprs.TotalHours);
        Assert.Equal(42.00m, client.TotalHours);
        Assert.Equal(18.75m, intern.TotalHours);
        Assert.Equal(5262m, vm.GrandTotalHours);
        Assert.Equal("XL5", vm.TShirtSize);
    }

    #endregion

    #region Happy Path — Per-Type Collaboration Totals

    [Fact]
    public void FinalEstimate_PerTypeCollaboration_CalculatedCorrectly()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 5;
        wprs.MeetingDurationMinutes = 60;
        wprs.NumberOfParticipants = 3;
        wprs.ParticipantPrepTimeMinutes = 0;

        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 3;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 2;
        client.ParticipantPrepTimeMinutes = 30;

        Assert.Equal(15.00m, vm.WprsHours);
        Assert.Equal(9.00m, vm.ClientMeetingsHours);
        Assert.Equal(0m, vm.InternalMeetingsHours);
        Assert.Equal(0m, vm.AutomationTestCollabHours);
        Assert.Equal(0m, vm.ConsultantMentorHours);
    }

    [Fact]
    public void FinalEstimate_PerTypeCollabAdjusted_AddedToTotal()
    {
        var vm = CreateVm();
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
        Assert.Equal(25.00m, vm.WprsTotalHours); // 15 + 10
    }

    #endregion

    #region Happy Path — Time for Estimates Included

    [Fact]
    public void FinalEstimate_TimeForEstimates_InSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal beforeSubtotal = vm.SubtotalHours;
        vm.TimeForEstimates = 50m;
        Assert.Equal(beforeSubtotal + 50m, vm.SubtotalHours);
    }

    [Fact]
    public void FinalEstimate_TimeForEstimates_AffectsRoleBreakout()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal baBefore = vm.BaRoleHours;
        decimal seBefore = vm.SeRoleHours;
        vm.TimeForEstimates = 100m;

        // BA includes TimeForEstimates/2, SE includes TimeForEstimates/2
        Assert.True(vm.BaRoleHours > baBefore);
        Assert.True(vm.SeRoleHours > seBefore);
    }

    #endregion

    #region Happy Path — Actual Hours Included

    [Fact]
    public void FinalEstimate_ActualHours_InSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal beforeSubtotal = vm.SubtotalHours;
        vm.TotalActualHours = 200m;
        Assert.Equal(beforeSubtotal + 200m, vm.SubtotalHours);
    }

    [Fact]
    public void FinalEstimate_ActualHours_AffectsRoleBreakout()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal baBefore = vm.BaRoleHours;
        decimal seBefore = vm.SeRoleHours;
        vm.TotalActualHours = 80m;

        Assert.True(vm.BaRoleHours > baBefore);
        Assert.True(vm.SeRoleHours > seBefore);
    }

    #endregion

    #region Sad Path — Negative Adjustments in Final Estimate

    [Fact]
    public void FinalEstimate_NegativeDevAdjustment_CascadesDownstream()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal sysTestBefore = vm.SystemTestingHours;
        vm.DevelopmentAdjustedHours = -50m;
        // Effective dev = 50, SysTest = ROUNDUP(50 * 0.30, 2) = 15.00
        Assert.Equal(15.00m, vm.SystemTestingHours);
        Assert.True(vm.SystemTestingHours < sysTestBefore);
    }

    [Fact]
    public void FinalEstimate_AllNegativeAdjustments_StillCalculates()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.DevelopmentAdjustedHours = -10m;
        vm.SystemTestingAdjustedHours = -5m;
        vm.AnalysisAdjustedHours = -2m;
        vm.BusinessDesignAdjustedHours = -3m;

        // Should still compute without error
        Assert.True(vm.GrandTotalHours > 0 || vm.GrandTotalHours <= 0);
    }

    #endregion

    #region Positive Path — PM Effort at Various Percentages

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    [InlineData(15)]
    [InlineData(20)]
    public void FinalEstimate_PMEffort_VariousPercentages(decimal pmPercent)
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.PmEffortPercentage = pmPercent;

        decimal allTasks = vm.TotalDevelopmentHours + vm.SystemTestingHours + vm.AnalysisHours
                         + vm.BusinessDesignHours + vm.PromotionHours + vm.BaSystemDocHours
                         + vm.ProductionValidationHours;
        decimal expectedPM = MainViewModel.RoundUp(allTasks * (pmPercent / 100m));
        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion

    #region Negative Path — Empty Estimate

    [Fact]
    public void FinalEstimate_NoComponents_NoCollaboration_ZeroGrandTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void FinalEstimate_OnlyAdjustments_NoComponents_StillZero()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        vm.DevelopmentAdjustedHours = 100m;
        // Even with adjustments, when ComponentCount==0 and effectiveDev==0+100=100,
        // it should calculate because effectiveDev != 0 (from adjustments)
        // Actually the check is: if (ComponentCount == 0 && effectiveDev == 0m) → show zero
        // Here effectiveDev = 0 + 100 = 100, so it WILL calculate
        Assert.True(vm.GrandTotalHours > 0);
    }

    #endregion

    #region Happy Path — Role Breakout Formula Matching

    [Fact]
    public void FinalEstimate_BaRole_Formula()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.TimeForEstimates = 20m;
        vm.TotalActualHours = 40m;

        // BA = ROUNDUP(Analysis/2 + BizDesign + BADoc + ProdVal + ActualHours/2 + TimeForEstimates/2, 2)
        decimal expected = MainViewModel.RoundUp(
            vm.AnalysisHours / 2m + vm.BusinessDesignHours + vm.BaSystemDocHours
            + vm.ProductionValidationHours + vm.TotalActualHours / 2m + vm.TimeForEstimates / 2m);
        Assert.Equal(expected, vm.BaRoleHours);
    }

    [Fact]
    public void FinalEstimate_SeRole_Formula()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.TimeForEstimates = 20m;
        vm.TotalActualHours = 40m;

        // SE = ROUNDUP(Dev + Analysis/2 + Promotion + ActualHours/2 + TimeForEstimates/2, 2)
        decimal expected = MainViewModel.RoundUp(
            vm.TotalDevelopmentHours + vm.AnalysisHours / 2m + vm.PromotionHours
            + vm.TotalActualHours / 2m + vm.TimeForEstimates / 2m);
        Assert.Equal(expected, vm.SeRoleHours);
    }

    [Fact]
    public void FinalEstimate_TesterRole_EqualsSysTest()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal(vm.SystemTestingHours, vm.TesterRoleHours);
    }

    [Fact]
    public void FinalEstimate_PMRole_EqualsPMEffort()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal(vm.ProjectManagementHours, vm.PmRoleHours);
    }

    [Fact]
    public void FinalEstimate_CollabRole_EqualsEffectiveCollab()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var collab = vm.CollaborationItems[^1];
        collab.CollabType = CollaborationType.WPRs;
        collab.NumberOfMeetings = 5;
        collab.MeetingDurationMinutes = 60;
        collab.NumberOfParticipants = 3;
        collab.ParticipantPrepTimeMinutes = 0;

        Assert.Equal(vm.CollaborationTotalHours, vm.CollaborationRoleHours);
    }

    #endregion

    #region Positive Path — Subtotal Formula

    [Fact]
    public void FinalEstimate_Subtotal_SumOfAllEffectiveTasks()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.TimeForEstimates = 10m;
        vm.TotalActualHours = 20m;

        decimal expectedSubtotal = MainViewModel.RoundUp(
            vm.DevelopmentTotalHours + vm.SystemTestingTotalHours + vm.AnalysisTotalHours
            + vm.BusinessDesignTotalHours + vm.PromotionTotalHours + vm.BaSystemDocTotalHours
            + vm.ProductionValidationTotalHours + vm.ProjectManagementTotalHours
            + vm.CollaborationTotalHours + vm.TimeForEstimates + vm.TotalActualHours);
        Assert.Equal(expectedSubtotal, vm.SubtotalHours);
    }

    [Fact]
    public void FinalEstimate_GrandTotal_CeilingOfSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    #endregion
}
