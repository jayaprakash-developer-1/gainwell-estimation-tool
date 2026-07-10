using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verification tests for "CO 23810 002 Final Estimate V1.0.xlsm".
/// Tests all calculation paths match the referenced Excel file.
/// </summary>
public class ExcelVerification_CO23810_002_Tests
{
    private InitialEstimateViewModel CreateVm() => new();

    private ComponentRowViewModel AddComponent(InitialEstimateViewModel vm, ComponentType type, ComponentSize size, ChangeType change, int count)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = count;
        return row;
    }

    private void ClearCollaboration(InitialEstimateViewModel vm)
    {
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
    }

    #region CO 23810 002 — Component Base Hours

    [Fact]
    public void CO23810_002_PowerBuilderWindows_Medium_Change_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change);
        Assert.Equal(60.63m, result);
    }

    [Fact]
    public void CO23810_002_Reports_Large_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.Reports, ComponentSize.Large, ChangeType.New);
        Assert.Equal(85.00m, result);
    }

    [Fact]
    public void CO23810_002_ProgramsDBStoredProcs_Medium_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New);
        Assert.Equal(115.00m, result);
    }

    #endregion

    #region CO 23810 002 — Calculation Pipeline (30% System Testing)

    [Fact]
    public void CO23810_002_DevelopmentTotal_Standard30Percent()
    {
        // Scenario: 2× PB Windows Medium Change (60.63 each) + 1× Reports Large New (85) + 3× Programs/DB Medium New (115)
        // Dev = 60.63*2 + 85 + 115*3 = 121.26 + 85 + 345 = 551.26
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        Assert.Equal(551.26m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void CO23810_002_SystemTesting_30Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // SystemTesting = ROUNDUP(551.26 * 0.30, 2) = ROUNDUP(165.378, 2) = 165.38
        Assert.Equal(165.38m, vm.SystemTestingHours);
    }

    [Fact]
    public void CO23810_002_Analysis_5Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // Analysis = ROUNDUP((551.26 + 165.38) * 0.05, 2) = ROUNDUP(716.64 * 0.05, 2) = ROUNDUP(35.832, 2) = 35.84
        Assert.Equal(35.84m, vm.AnalysisHours);
    }

    [Fact]
    public void CO23810_002_BusinessDesign_15Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // BusinessDesign = ROUNDUP((551.26 + 165.38) * 0.15, 2) = ROUNDUP(716.64 * 0.15, 2) = ROUNDUP(107.496, 2) = 107.50
        Assert.Equal(107.50m, vm.BusinessDesignHours);
    }

    [Fact]
    public void CO23810_002_Promotion_5Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // Promotion = ROUNDUP(551.26 * 0.05, 2) = ROUNDUP(27.563, 2) = 27.57
        Assert.Equal(27.57m, vm.PromotionHours);
    }

    [Fact]
    public void CO23810_002_BaSystemDoc_5Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // BaSystemDoc = ROUNDUP(551.26 * 0.05, 2) = 27.57
        Assert.Equal(27.57m, vm.BaSystemDocHours);
    }

    [Fact]
    public void CO23810_002_ProductionValidation_20Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // ProductionValidation = ROUNDUP(165.38 * 0.20, 2) = ROUNDUP(33.076, 2) = 33.08
        Assert.Equal(33.08m, vm.ProductionValidationHours);
    }

    [Fact]
    public void CO23810_002_PMEffort_15Percent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // PM = ROUNDUP((551.26+165.38+35.84+107.50+27.57+27.57+33.08) * 0.15, 2)
        // = ROUNDUP(948.20 * 0.15, 2) = ROUNDUP(142.23, 2) = 142.23
        decimal allTasks = 551.26m + 165.38m + 35.84m + 107.50m + 27.57m + 27.57m + 33.08m;
        decimal expectedPM = InitialEstimateViewModel.RoundUp(allTasks * 0.15m);
        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    [Fact]
    public void CO23810_002_GrandTotal_Ceiling()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // Grand Total = Math.Ceiling(Subtotal)
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    [Fact]
    public void CO23810_002_TShirtSize_Large()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 2);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 3);

        // Grand total ~1091 → XL1
        Assert.Equal("XL1", vm.TShirtSize);
    }

    #endregion
}
