using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Happy path tests for MainViewModel — verifying correct behavior under normal conditions.
/// </summary>
public class MainViewModel_HappyPathTests
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

    #region Happy Path — Normal Initialization

    [Fact]
    public void HappyPath_NewVm_HasDefaultValues()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.ProjectName);
        Assert.Equal(string.Empty, vm.ChangeOrderId);
        Assert.Equal(15m, vm.PmEffortPercentage);
        Assert.Equal(0, vm.ComponentCount);
        Assert.Equal("—", vm.TShirtSize);
        Assert.Equal(0m, vm.GrandTotalHours);
    }

    [Fact]
    public void HappyPath_NewVm_HasDefaultCollaboration()
    {
        var vm = CreateVm();
        Assert.Equal(4, vm.CollaborationItems.Count);
        Assert.Equal("WPRs", vm.CollaborationItems[0].TaskName);
        Assert.Equal("Client Meetings", vm.CollaborationItems[1].TaskName);
        Assert.Equal("Internal Meetings", vm.CollaborationItems[2].TaskName);
        Assert.Equal("Automation Test Collaboration", vm.CollaborationItems[3].TaskName);
    }

    [Fact]
    public void HappyPath_NewVm_AllAdjustmentsZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.DevelopmentAdjustedHours);
        Assert.Equal(0m, vm.AnalysisAdjustedHours);
        Assert.Equal(0m, vm.BusinessDesignAdjustedHours);
        Assert.Equal(0m, vm.SystemTestingAdjustedHours);
        Assert.Equal(0m, vm.PromotionAdjustedHours);
        Assert.Equal(0m, vm.BaSystemDocAdjustedHours);
        Assert.Equal(0m, vm.ProductionValidationAdjustedHours);
        Assert.Equal(0m, vm.ProjectManagementAdjustedHours);
        Assert.Equal(0m, vm.CollaborationAdjustedHours);
    }

    #endregion

    #region Happy Path — Adding Components

    [Fact]
    public void HappyPath_AddSingleComponent_CalculatesCorrectly()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 1);

        Assert.Equal(75.00m, vm.TotalDevelopmentHours);
        Assert.Equal(1, vm.ComponentCount);
        Assert.True(vm.GrandTotalHours > 0);
    }

    [Fact]
    public void HappyPath_AddMultipleComponents_SumsCorrectly()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 2); // 50
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 1); // 51

        Assert.Equal(101.00m, vm.TotalDevelopmentHours);
        Assert.Equal(2, vm.ComponentCount);
    }

    [Fact]
    public void HappyPath_RemoveComponent_RecalculatesTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 2); // 50
        AddComponent(vm, ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 1); // 51

        vm.RemoveComponentCommand.Execute(vm.Components[1]);
        Assert.Equal(50.00m, vm.TotalDevelopmentHours);
        Assert.Equal(1, vm.ComponentCount);
    }

    [Fact]
    public void HappyPath_ComponentCount_MultipliesBaseHours()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Small, ChangeType.New, 10);

        // 17 × 10 = 170
        Assert.Equal(170.00m, vm.TotalDevelopmentHours);
    }

    #endregion

    #region Happy Path — Collaboration

    [Fact]
    public void HappyPath_SetCollaboration_IncludedInTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var collab = vm.CollaborationItems[^1];
        collab.CollabType = CollaborationType.WPRs;
        collab.NumberOfMeetings = 10;
        collab.MeetingDurationMinutes = 60;
        collab.NumberOfParticipants = 3;
        collab.ParticipantPrepTimeMinutes = 15;

        // 10 × (1 + 0.25) × 3 = 37.5
        Assert.Equal(37.50m, collab.TotalHours);
        Assert.Equal(37.50m, vm.TotalCollaborationHours);
    }

    [Fact]
    public void HappyPath_MultipleCollabTypes_SummedCorrectly()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 5;
        wprs.MeetingDurationMinutes = 60;
        wprs.NumberOfParticipants = 2;
        wprs.ParticipantPrepTimeMinutes = 15;

        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 3;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 2;
        client.ParticipantPrepTimeMinutes = 0;

        Assert.Equal(wprs.TotalHours + client.TotalHours, vm.TotalCollaborationHours);
    }

    #endregion

    #region Happy Path — Adjusted Hours

    [Fact]
    public void HappyPath_PositiveAdjustment_IncreasesTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.DevelopmentAdjustedHours = 25m;
        Assert.Equal(125m, vm.DevelopmentTotalHours);
    }

    [Fact]
    public void HappyPath_AllAdjustments_AffectGrandTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal gtBefore = vm.GrandTotalHours;
        vm.DevelopmentAdjustedHours = 50m;
        Assert.True(vm.GrandTotalHours > gtBefore);
    }

    #endregion

    #region Happy Path — PM Percentage

    [Fact]
    public void HappyPath_PMPercentage_DefaultIs15()
    {
        var vm = CreateVm();
        Assert.Equal(15m, vm.PmEffortPercentage);
    }

    [Fact]
    public void HappyPath_PMPercentage_Change_RecalculatesPM()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal pm15 = vm.ProjectManagementHours;
        vm.PmEffortPercentage = 20m;
        Assert.True(vm.ProjectManagementHours > pm15);
    }

    #endregion

    #region Happy Path — ClearAll

    [Fact]
    public void HappyPath_ClearAll_ResetsEverything()
    {
        var vm = CreateVm();
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.DevelopmentAdjustedHours = 50m;

        vm.ClearAllCommand.Execute(null);

        Assert.Equal(string.Empty, vm.ProjectName);
        Assert.Equal(string.Empty, vm.ChangeOrderId);
        Assert.Equal(0, vm.ComponentCount);
        Assert.Equal(0m, vm.TotalDevelopmentHours);
        Assert.Equal(0m, vm.DevelopmentAdjustedHours);
        Assert.Equal(15m, vm.PmEffortPercentage);
        Assert.Equal("—", vm.TShirtSize);
    }

    #endregion

    #region Happy Path — Role Breakout

    [Fact]
    public void HappyPath_RoleBreakout_AllRolesPositive()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        Assert.True(vm.BaRoleHours > 0);
        Assert.True(vm.SeRoleHours > 0);
        Assert.True(vm.TesterRoleHours > 0);
        Assert.True(vm.PmRoleHours > 0);
    }

    [Fact]
    public void HappyPath_TesterRole_EqualsSysTest()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        Assert.Equal(vm.SystemTestingHours, vm.TesterRoleHours);
    }

    [Fact]
    public void HappyPath_PMRole_EqualsPMEffort()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        Assert.Equal(vm.ProjectManagementHours, vm.PmRoleHours);
    }

    #endregion

    #region Happy Path — Test Case Mode

    [Fact]
    public void HappyPath_TestCaseMode_EnabledAndCalculates()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 100;
        vm.TestCaseIterations = 2m;

        // 100 * 2.1925 * 2 + (100 * 1.5675 * 0.1) * 2
        Assert.True(vm.SystemTestingHours > 0);
        Assert.True(vm.SystemTestingHours != MainViewModel.RoundUp(100m * 0.30m)); // Not 30% anymore
    }

    [Fact]
    public void HappyPath_TestCaseMode_DisabledRevertsTo30Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 100;
        vm.TestCaseIterations = 2m;
        decimal testCaseHours = vm.SystemTestingHours;

        vm.UseTestCasesForEstimate = false;
        Assert.Equal(30.00m, vm.SystemTestingHours); // Back to 30%
        Assert.NotEqual(testCaseHours, vm.SystemTestingHours);
    }

    #endregion
}
