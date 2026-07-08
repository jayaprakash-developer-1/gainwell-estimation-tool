using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verification tests against "PROMISe_Estimating Tool.xlsm" (the base Excel template).
/// Verifies all 66 weighted values, T-shirt size boundaries, and formula structure.
/// </summary>
public class ExcelVerification_PROMISe_Tests
{
    #region All 66 Weighted Values — Complete Matrix

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New, 25.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 20.94)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.New, 75.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium, ChangeType.Change, 60.63)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 125.00)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.Change, 100.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Small, ChangeType.New, 17.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Small, ChangeType.Change, 13.60)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium, ChangeType.New, 51.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium, ChangeType.Change, 40.80)]
    [InlineData(ComponentType.Reports, ComponentSize.Large, ChangeType.New, 85.00)]
    [InlineData(ComponentType.Reports, ComponentSize.Large, ChangeType.Change, 68.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.New, 46.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small, ChangeType.Change, 36.80)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.New, 115.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium, ChangeType.Change, 92.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 294.40)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.Change, 235.525)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Small, ChangeType.New, 5.00)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Small, ChangeType.Change, 4.0625)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.New, 11.875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 9.6875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 26.875)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change, 21.5625)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.New, 5.9375)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small, ChangeType.Change, 4.6875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New, 15.00)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.Change, 11.875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 31.875)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 25.625)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Small, ChangeType.Change, 6.10)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Medium, ChangeType.Change, 6.10)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.New, 8.125)]
    [InlineData(ComponentType.DatabaseReview, ComponentSize.Large, ChangeType.Change, 6.10)]
    [InlineData(ComponentType.Webpage, ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Small, ChangeType.Change, 16.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 60.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium, ChangeType.Change, 48.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large, ChangeType.Change, 75.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.New, 50.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Small, ChangeType.Change, 35.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.New, 100.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium, ChangeType.Change, 80.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.New, 200.00)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large, ChangeType.Change, 150.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.New, 15.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium, ChangeType.Change, 35.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.New, 90.00)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Large, ChangeType.Change, 75.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.New, 3.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Small, ChangeType.Change, 5.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.New, 8.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Medium, ChangeType.Change, 1.00)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.New, 2.50)]
    [InlineData(ComponentType.TestAutomationUFT, ComponentSize.Large, ChangeType.Change, 5.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Small, ChangeType.New, 20.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Small, ChangeType.Change, 10.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Medium, ChangeType.New, 50.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Medium, ChangeType.Change, 25.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Large, ChangeType.New, 100.00)]
    [InlineData(ComponentType.MISC, ComponentSize.Large, ChangeType.Change, 50.00)]
    public void PROMISe_WeightedValues_AllMatch(ComponentType type, ComponentSize size, ChangeType change, decimal expected)
    {
        Assert.Equal(expected, WeightedValues.GetBaseHours(type, size, change));
    }

    #endregion

    #region T-Shirt Size Boundaries (PROMISe Template)

    [Theory]
    [InlineData(0, "—")]
    [InlineData(1, "Small")]
    [InlineData(99, "Small")]
    [InlineData(100, "Medium")]
    [InlineData(299, "Medium")]
    [InlineData(300, "Large")]
    [InlineData(749, "Large")]
    [InlineData(750, "X-Large")]
    [InlineData(999, "X-Large")]
    [InlineData(1000, "XL1")]
    [InlineData(1999, "XL1")]
    [InlineData(2000, "XL2")]
    [InlineData(2999, "XL2")]
    [InlineData(3000, "XL3")]
    [InlineData(3999, "XL3")]
    [InlineData(4000, "XL4")]
    [InlineData(4999, "XL4")]
    [InlineData(5000, "XL5")]
    [InlineData(5999, "XL5")]
    [InlineData(6000, "XL6")]
    [InlineData(6999, "XL6")]
    [InlineData(7000, "XL7")]
    [InlineData(7999, "XL7")]
    [InlineData(8000, "XL8")]
    [InlineData(10000, "XL8")]
    [InlineData(100000, "XL8")]
    public void PROMISe_TShirtSize_BoundaryValues(int grandTotal, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(grandTotal));
    }

    #endregion

    #region PROMISe Formula Structure

    [Fact]
    public void PROMISe_FormulaStructure_SystemTesting_30PercentOfDev()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // Dev = 100, SysTest = ROUNDUP(100*0.30, 2) = 30.00
        Assert.Equal(100m, vm.TotalDevelopmentHours);
        Assert.Equal(30.00m, vm.SystemTestingHours);
    }

    [Fact]
    public void PROMISe_FormulaStructure_Analysis_5PercentOfDevPlusSysTest()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // Analysis = ROUNDUP((100 + 30) * 0.05, 2) = ROUNDUP(6.50, 2) = 6.50
        Assert.Equal(6.50m, vm.AnalysisHours);
    }

    [Fact]
    public void PROMISe_FormulaStructure_BusinessDesign_15PercentOfDevPlusSysTest()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // BusinessDesign = ROUNDUP((100 + 30) * 0.15, 2) = ROUNDUP(19.50, 2) = 19.50
        Assert.Equal(19.50m, vm.BusinessDesignHours);
    }

    [Fact]
    public void PROMISe_FormulaStructure_Promotion_5PercentOfDev()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // Promotion = ROUNDUP(100 * 0.05, 2) = 5.00
        Assert.Equal(5.00m, vm.PromotionHours);
    }

    [Fact]
    public void PROMISe_FormulaStructure_ProductionValidation_20PercentOfSysTest()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // ProdVal = ROUNDUP(30 * 0.20, 2) = 6.00
        Assert.Equal(6.00m, vm.ProductionValidationHours);
    }

    [Fact]
    public void PROMISe_FormulaStructure_GrandTotal_CeilingOfSubtotal()
    {
        var vm = new MainViewModel();
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);

        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.MISC;
        row.Size = ComponentSize.Large;
        row.ChangeType = ChangeType.New;
        row.Count = 1;

        // Verify grand total is ceiling of subtotal
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    #endregion

    #region PROMISe — Component Type None Returns Zero

    [Theory]
    [InlineData(ComponentSize.Small, ChangeType.New)]
    [InlineData(ComponentSize.Medium, ChangeType.Change)]
    [InlineData(ComponentSize.Large, ChangeType.New)]
    public void PROMISe_NoneComponentType_ReturnsZero(ComponentSize size, ChangeType change)
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.None, size, change));
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ChangeType.New)]
    [InlineData(ComponentType.Reports, ChangeType.Change)]
    public void PROMISe_NoneSize_ReturnsZero(ComponentType type, ChangeType change)
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(type, ComponentSize.None, change));
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small)]
    [InlineData(ComponentType.Reports, ComponentSize.Large)]
    public void PROMISe_NoneChangeType_ReturnsZero(ComponentType type, ComponentSize size)
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(type, size, ChangeType.None));
    }

    #endregion
}
