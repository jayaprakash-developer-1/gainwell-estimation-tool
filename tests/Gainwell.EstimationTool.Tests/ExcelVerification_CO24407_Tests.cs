using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verification tests for "CO 24407 Final Estimate V1.0.xlsm".
/// Tests large-scale estimates with many components and full collaboration.
/// </summary>
public class ExcelVerification_CO24407_Tests
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

    #region CO 24407 — Component Base Hours Verification

    [Fact]
    public void CO24407_ProgramsDBStoredProcs_Large_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New);
        Assert.Equal(294.40m, result);
    }

    [Fact]
    public void CO24407_ProgramsDBStoredProcs_Large_Change_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change);
        Assert.Equal(235.525m, result);
    }

    [Fact]
    public void CO24407_PowerBuilderWindows_Large_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New);
        Assert.Equal(125.00m, result);
    }

    [Fact]
    public void CO24407_PowerBuilderWindows_Large_Change_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change);
        Assert.Equal(100.00m, result);
    }

    #endregion

    #region CO 24407 — Large Scale Estimate

    [Fact]
    public void CO24407_LargeProject_DevelopmentTotal()
    {
        // Large project with many components
        // 3× PB Windows Large New (125ea) + 5× PB Windows Large Change (100ea)
        // + 2× Programs Large New (294.40ea) + 10× SupportModules Small Change (4.0625ea)
        // + 4× DBManipulation Large New (31.875ea) + 6× Reports Medium New (51ea)
        // Dev = 375 + 500 + 588.80 + 40.625 + 127.5 + 306 = 1937.925
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        decimal expected = 125m * 3 + 100m * 5 + 294.40m * 2 + 4.0625m * 10 + 31.875m * 4 + 51m * 6;
        Assert.Equal(expected, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void CO24407_LargeProject_AllDerivedTasks()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        decimal dev = vm.TotalDevelopmentHours;
        decimal sysTest = MainViewModel.RoundUp(dev * 0.30m);
        decimal analysis = MainViewModel.RoundUp((dev + sysTest) * 0.05m);
        decimal bizDesign = MainViewModel.RoundUp((dev + sysTest) * 0.15m);
        decimal promotion = MainViewModel.RoundUp(dev * 0.05m);
        decimal baDoc = MainViewModel.RoundUp(dev * 0.05m);
        decimal prodVal = MainViewModel.RoundUp(sysTest * 0.20m);

        Assert.Equal(sysTest, vm.SystemTestingHours);
        Assert.Equal(analysis, vm.AnalysisHours);
        Assert.Equal(bizDesign, vm.BusinessDesignHours);
        Assert.Equal(promotion, vm.PromotionHours);
        Assert.Equal(baDoc, vm.BaSystemDocHours);
        Assert.Equal(prodVal, vm.ProductionValidationHours);
    }

    [Fact]
    public void CO24407_LargeProject_PMCalculation()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        decimal allTasks = vm.TotalDevelopmentHours + vm.SystemTestingHours + vm.AnalysisHours
                         + vm.BusinessDesignHours + vm.PromotionHours + vm.BaSystemDocHours
                         + vm.ProductionValidationHours;
        decimal expectedPM = MainViewModel.RoundUp(allTasks * 0.15m);
        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    [Fact]
    public void CO24407_LargeProject_GrandTotal_XL3()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 5);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 10);
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 4);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 6);

        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
        // Dev ~1937.93, so grand total should be in XL2-XL3 range
        Assert.True(vm.GrandTotalHours >= 2000m);
    }

    #endregion

    #region CO 24407 — With Full Collaboration

    [Fact]
    public void CO24407_WithCollaboration_AllTypes()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 2);

        // Add WPRs
        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 15;
        wprs.MeetingDurationMinutes = 30;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        // WPRs = 15 × (30/60 + 60/60) × 5 = 15 × 1.5 × 5 = 112.5
        Assert.Equal(112.50m, wprs.TotalHours);

        // Add Client Meetings
        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 10;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 4;
        client.ParticipantPrepTimeMinutes = 30;

        // Client = 10 × (60/60 + 30/60) × 4 = 10 × 1.5 × 4 = 60
        Assert.Equal(60.00m, client.TotalHours);

        Assert.Equal(172.50m, vm.TotalCollaborationHours);
    }

    [Fact]
    public void CO24407_CollaborationIncludedInSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal subtotalBefore = vm.SubtotalHours;

        vm.AddCollaborationItemCommand.Execute(null);
        var collab = vm.CollaborationItems[^1];
        collab.CollabType = CollaborationType.WPRs;
        collab.NumberOfMeetings = 5;
        collab.MeetingDurationMinutes = 60;
        collab.NumberOfParticipants = 3;
        collab.ParticipantPrepTimeMinutes = 15;

        // Collaboration adds to subtotal
        Assert.True(vm.SubtotalHours > subtotalBefore);
    }

    #endregion

    #region CO 24407 — Time for Estimates and Actual Hours

    [Fact]
    public void CO24407_TimeForEstimates_IncludedInSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        decimal subtotalBefore = vm.SubtotalHours;

        vm.TimeForEstimates = 50m;
        Assert.Equal(subtotalBefore + 50m, vm.SubtotalHours);
    }

    [Fact]
    public void CO24407_TotalActualHours_IncludedInSubtotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        decimal subtotalBefore = vm.SubtotalHours;

        vm.TotalActualHours = 100m;
        Assert.Equal(subtotalBefore + 100m, vm.SubtotalHours);
    }

    [Fact]
    public void CO24407_ActualHoursAsOfDate_StoredCorrectly()
    {
        var vm = CreateVm();
        var date = new DateTime(2026, 6, 15);
        vm.ActualHoursAsOfDate = date;
        Assert.Equal(date, vm.ActualHoursAsOfDate);
    }

    #endregion
}
