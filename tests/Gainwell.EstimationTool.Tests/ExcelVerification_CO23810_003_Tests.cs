using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verification tests for "CO 23810 003 Final Estimate V1.0.xlsm".
/// Covers additional component scenarios and test-case-based estimation.
/// </summary>
public class ExcelVerification_CO23810_003_Tests
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

    #region CO 23810 003 — Component Verification

    [Fact]
    public void CO23810_003_Webpage_Medium_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.Webpage, ComponentSize.Medium, ChangeType.New);
        Assert.Equal(60.00m, result);
    }

    [Fact]
    public void CO23810_003_K2Workflow_Large_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New);
        Assert.Equal(200.00m, result);
    }

    [Fact]
    public void CO23810_003_K2SmartForm_Large_Change_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change);
        Assert.Equal(75.00m, result);
    }

    #endregion

    #region CO 23810 003 — Full Calculation With Mixed Components

    [Fact]
    public void CO23810_003_DevelopmentTotal_MixedComponents()
    {
        // Scenario: 4× Webpage Med New (60ea) + 2× K2Workflow Large New (200ea) + 1× K2SmartForm Large Change (75)
        // Dev = 240 + 400 + 75 = 715
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        Assert.Equal(715.00m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void CO23810_003_SystemTesting_30Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        // SystemTesting = ROUNDUP(715 * 0.30, 2) = ROUNDUP(214.50, 2) = 214.50
        Assert.Equal(214.50m, vm.SystemTestingHours);
    }

    [Fact]
    public void CO23810_003_Analysis_5Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        // Analysis = ROUNDUP((715 + 214.50) * 0.05, 2) = ROUNDUP(929.50 * 0.05, 2) = ROUNDUP(46.475, 2) = 46.48
        Assert.Equal(46.48m, vm.AnalysisHours);
    }

    [Fact]
    public void CO23810_003_BusinessDesign_15Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        // BusinessDesign = ROUNDUP((715 + 214.50) * 0.15, 2) = ROUNDUP(929.50 * 0.15, 2) = ROUNDUP(139.425, 2) = 139.43
        Assert.Equal(139.43m, vm.BusinessDesignHours);
    }

    [Fact]
    public void CO23810_003_Promotion_5Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        // Promotion = ROUNDUP(715 * 0.05, 2) = ROUNDUP(35.75, 2) = 35.75 (exact)
        Assert.Equal(35.75m, vm.PromotionHours);
    }

    [Fact]
    public void CO23810_003_ProductionValidation_20Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        // ProdVal = ROUNDUP(214.50 * 0.20, 2) = ROUNDUP(42.90, 2) = 42.90
        Assert.Equal(42.90m, vm.ProductionValidationHours);
    }

    [Fact]
    public void CO23810_003_GrandTotal_XL1()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);
        AddComponent(vm, ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 2);
        AddComponent(vm, ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 1);

        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
        Assert.True(vm.GrandTotalHours >= 1000m && vm.GrandTotalHours < 2000m);
        Assert.Equal("XL1", vm.TShirtSize);
    }

    #endregion

    #region CO 23810 003 — With Test Cases

    [Fact]
    public void CO23810_003_TestCases_OverridesPercentage()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 4);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 50;
        vm.TestCasesMedium = 30;
        vm.TestCasesComplex = 0;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 1.5m;

        // mainHours = 50*2.1925 + 30*4.065 = 109.625 + 121.95 = 231.575
        // defectHours = (50*1.5675 + 30*3.44) * 0.1 = (78.375 + 103.2) * 0.1 = 18.1575
        // total = (231.575 + 18.1575) * 1.5 = 249.7325 * 1.5 = 374.59875 → ROUNDUP = 374.60
        const decimal r31Simple = 2.1925m, r31Medium = 4.065m;
        const decimal r32Simple = 1.5675m, r32Medium = 3.44m;
        decimal mainHours = 50m * r31Simple + 30m * r31Medium;
        decimal defectHours = (50m * r32Simple + 30m * r32Medium) * 0.1m;
        decimal expectedSysTest = MainViewModel.RoundUp((mainHours + defectHours) * 1.5m);

        Assert.Equal(expectedSysTest, vm.SystemTestingHours);
    }

    [Fact]
    public void CO23810_003_TestCases_VeryComplex()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 10;
        vm.TestCasesMedium = 20;
        vm.TestCasesComplex = 15;
        vm.TestCasesVeryComplex = 5;
        vm.TestCaseIterations = 2m;

        const decimal r31S = 2.1925m, r31M = 4.065m, r31C = 8.76m, r31VC = 14.38m;
        const decimal r32S = 1.5675m, r32M = 3.44m, r32C = 7.51m, r32VC = 13.13m;
        decimal main = 10m * r31S + 20m * r31M + 15m * r31C + 5m * r31VC;
        decimal defect = (10m * r32S + 20m * r32M + 15m * r32C + 5m * r32VC) * 0.1m;
        decimal expected = MainViewModel.RoundUp((main + defect) * 2m);

        Assert.Equal(expected, vm.SystemTestingHours);
    }

    #endregion
}
