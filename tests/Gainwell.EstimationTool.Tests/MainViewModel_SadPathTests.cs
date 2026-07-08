using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Sad path tests for MainViewModel — verifying behavior under error conditions,
/// missing data, invalid inputs, and edge cases that should be handled gracefully.
/// </summary>
public class MainViewModel_SadPathTests
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

    #region Sad Path — Save Validation

    [Fact]
    public void SadPath_SaveProject_EmptyName_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("Project Name", result);
    }

    [Fact]
    public void SadPath_SaveProject_EmptyChangeOrderId_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("CO / Defect #", result);
    }

    [Fact]
    public void SadPath_SaveProject_EmptyDescription_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("Description", result);
    }

    [Fact]
    public void SadPath_SaveProject_EmptyEstimatedBy_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "";
        vm.ReviewedBy = "Reviewer";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("Estimated By", result);
    }

    [Fact]
    public void SadPath_SaveProject_EmptyReviewedBy_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("Reviewed By", result);
    }

    [Fact]
    public void SadPath_SaveProject_NoComponents_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("component", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void SadPath_SaveProject_OnlyNoneTypeComponents_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "Test";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";
        vm.AddComponentCommand.Execute(null); // Adds with ComponentType.None

        var result = vm.SaveProject();
        Assert.NotNull(result);
        Assert.Contains("component", result, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Sad Path — Component with None Values

    [Fact]
    public void SadPath_ComponentNoneType_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.None,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 5
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_ComponentNoneSize_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.None,
            ChangeType = ChangeType.New,
            Count = 5
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_ComponentNoneChange_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.None,
            Count = 5
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_ComponentZeroCount_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 0
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    #endregion

    #region Sad Path — Remove Operations on Empty/Null

    [Fact]
    public void SadPath_RemoveComponent_Null_NoException()
    {
        var vm = CreateVm();
        vm.RemoveComponentCommand.Execute(null);
        Assert.Equal(0, vm.ComponentCount);
    }

    [Fact]
    public void SadPath_RemoveCollaboration_Null_NoException()
    {
        var vm = CreateVm();
        vm.RemoveCollaborationItemCommand.Execute(null);
        Assert.Equal(4, vm.CollaborationItems.Count); // unchanged
    }

    #endregion

    #region Sad Path — Negative Adjusted Hours

    [Fact]
    public void SadPath_NegativeDevAdjustment_ReducesTotal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.DevelopmentAdjustedHours = -150m; // More than dev hours
        // Effective Dev = 100 - 150 = -50 — system allows this (mid-project re-estimation)
        Assert.Equal(-50m, vm.DevelopmentTotalHours);
    }

    [Fact]
    public void SadPath_NegativeDevAdjustment_NegativeSystemTesting()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.DevelopmentAdjustedHours = -150m;
        // SysTest = ROUNDUP(-50 * 0.30, 2) — should handle negatives
        // RoundUp for negative: -15 * 100 = -1500, truncate = -1500, shifted = -1500
        // shifted < truncated is false, shifted > truncated is false → -1500/100 = -15.00
        // Actually: -50 * 0.30 = -15.00 exactly, so RoundUp(-15.00) = -15.00
        Assert.Equal(-15.00m, vm.SystemTestingHours);
    }

    #endregion

    #region Sad Path — No Components

    [Fact]
    public void SadPath_NoComponents_GrandTotalZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.GrandTotalHours);
    }

    [Fact]
    public void SadPath_NoComponents_TShirtDash()
    {
        var vm = CreateVm();
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void SadPath_NoComponents_AllDerivedZero()
    {
        var vm = CreateVm();
        Assert.Equal(0m, vm.SystemTestingHours);
        Assert.Equal(0m, vm.AnalysisHours);
        Assert.Equal(0m, vm.BusinessDesignHours);
        Assert.Equal(0m, vm.PromotionHours);
        Assert.Equal(0m, vm.BaSystemDocHours);
        Assert.Equal(0m, vm.ProductionValidationHours);
        Assert.Equal(0m, vm.ProjectManagementHours);
    }

    #endregion

    #region Sad Path — WhiteSpace-Only Fields

    [Fact]
    public void SadPath_SaveProject_WhitespaceProjectName_ReturnsError()
    {
        var vm = CreateVm();
        vm.ProjectName = "   ";
        vm.ChangeOrderId = "CO-001";
        vm.ProjectDescription = "Desc";
        vm.EstimatedBy = "User";
        vm.ReviewedBy = "Reviewer";
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        var result = vm.SaveProject();
        Assert.NotNull(result);
    }

    #endregion

    #region Sad Path — Test Cases with Zero Iterations

    [Fact]
    public void SadPath_TestCases_ZeroIterations_UsesMinimum1()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCaseIterations = 0m;

        // Math.Max(1m, 0) = 1, so iterations = 1
        const decimal r31Simple = 2.1925m;
        const decimal r32Simple = 1.5675m;
        decimal mainHours = 10m * r31Simple;
        decimal defectHours = 10m * r32Simple * 0.1m;
        decimal expected = MainViewModel.RoundUp((mainHours + defectHours) * 1m);
        Assert.Equal(expected, vm.SystemTestingHours);
    }

    [Fact]
    public void SadPath_TestCases_AllZero_SystemTestingZero()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 0;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2m;

        // All zeros → SysTest = 0
        Assert.Equal(0m, vm.SystemTestingHours);
    }

    #endregion
}
