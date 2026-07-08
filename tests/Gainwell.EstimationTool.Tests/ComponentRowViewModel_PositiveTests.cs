using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Positive path tests for ComponentRowViewModel — verifying all component types,
/// sizes, and change types produce correct base hours and totals.
/// </summary>
public class ComponentRowViewModel_PositiveTests
{
    #region All Component Types — Small New

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, 25.00)]
    [InlineData(ComponentType.Reports, 17.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, 46.00)]
    [InlineData(ComponentType.SupportModules, 5.00)]
    [InlineData(ComponentType.DBManipulation, 5.9375)]
    [InlineData(ComponentType.DatabaseReview, 8.125)]
    [InlineData(ComponentType.Webpage, 20.00)]
    [InlineData(ComponentType.K2Workflow, 50.00)]
    [InlineData(ComponentType.K2SmartForm, 15.00)]
    [InlineData(ComponentType.TestAutomationUFT, 3.00)]
    [InlineData(ComponentType.MISC, 20.00)]
    public void Positive_SmallNew_AllTypes(ComponentType type, decimal expected)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = type,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(expected, row.BaseHoursPerUnit);
        Assert.Equal(expected, row.TotalHours);
    }

    #endregion

    #region All Component Types — Medium Change

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, 60.63)]
    [InlineData(ComponentType.Reports, 40.80)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, 92.00)]
    [InlineData(ComponentType.SupportModules, 9.6875)]
    [InlineData(ComponentType.DBManipulation, 11.875)]
    [InlineData(ComponentType.DatabaseReview, 6.10)]
    [InlineData(ComponentType.Webpage, 48.00)]
    [InlineData(ComponentType.K2Workflow, 80.00)]
    [InlineData(ComponentType.K2SmartForm, 35.00)]
    [InlineData(ComponentType.TestAutomationUFT, 1.00)]
    [InlineData(ComponentType.MISC, 25.00)]
    public void Positive_MediumChange_AllTypes(ComponentType type, decimal expected)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = type,
            Size = ComponentSize.Medium,
            ChangeType = ChangeType.Change,
            Count = 1
        };
        Assert.Equal(expected, row.BaseHoursPerUnit);
    }

    #endregion

    #region All Component Types — Large New

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, 125.00)]
    [InlineData(ComponentType.Reports, 85.00)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, 294.40)]
    [InlineData(ComponentType.SupportModules, 26.875)]
    [InlineData(ComponentType.DBManipulation, 31.875)]
    [InlineData(ComponentType.DatabaseReview, 8.125)]
    [InlineData(ComponentType.Webpage, 90.00)]
    [InlineData(ComponentType.K2Workflow, 200.00)]
    [InlineData(ComponentType.K2SmartForm, 90.00)]
    [InlineData(ComponentType.TestAutomationUFT, 2.50)]
    [InlineData(ComponentType.MISC, 100.00)]
    public void Positive_LargeNew_AllTypes(ComponentType type, decimal expected)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = type,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(expected, row.BaseHoursPerUnit);
    }

    #endregion

    #region Count Multiplication

    [Theory]
    [InlineData(1, 25.00)]
    [InlineData(2, 50.00)]
    [InlineData(3, 75.00)]
    [InlineData(5, 125.00)]
    [InlineData(10, 250.00)]
    [InlineData(50, 1250.00)]
    [InlineData(100, 2500.00)]
    public void Positive_Count_MultipliesCorrectly(int count, decimal expectedTotal)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            Count = count
        };
        Assert.Equal(expectedTotal, row.TotalHours);
    }

    #endregion

    #region TotalHours Property

    [Fact]
    public void Positive_TotalHours_IsBaseTimesCount()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.ProgramsDBStoredProcs,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.Change,
            Count = 3
        };
        Assert.Equal(235.525m * 3, row.TotalHours);
    }

    [Fact]
    public void Positive_TotalHours_FractionalPrecision()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.SupportModules,
            Size = ComponentSize.Medium,
            ChangeType = ChangeType.Change,
            Count = 53
        };
        // 9.6875 × 53 = 513.4375
        Assert.Equal(513.4375m, row.TotalHours);
    }

    #endregion

    #region UpdateBaseHours Auto-triggered

    [Fact]
    public void Positive_ChangeType_TriggersUpdate()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(125.00m, row.BaseHoursPerUnit);

        row.ChangeType = ChangeType.Change;
        Assert.Equal(100.00m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void Positive_SizeChange_TriggersUpdate()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.Reports,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(17.00m, row.BaseHoursPerUnit);

        row.Size = ComponentSize.Large;
        Assert.Equal(85.00m, row.BaseHoursPerUnit);
    }

    [Fact]
    public void Positive_TypeChange_TriggersUpdate()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(20.00m, row.BaseHoursPerUnit);

        row.ComponentType = ComponentType.K2Workflow;
        Assert.Equal(50.00m, row.BaseHoursPerUnit);
    }

    #endregion

    #region DatabaseReview — Same Hours For All Sizes

    [Theory]
    [InlineData(ComponentSize.Small)]
    [InlineData(ComponentSize.Medium)]
    [InlineData(ComponentSize.Large)]
    public void Positive_DatabaseReview_SameHoursAllSizes_New(ComponentSize size)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.DatabaseReview,
            Size = size,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(8.125m, row.BaseHoursPerUnit);
    }

    [Theory]
    [InlineData(ComponentSize.Small)]
    [InlineData(ComponentSize.Medium)]
    [InlineData(ComponentSize.Large)]
    public void Positive_DatabaseReview_SameHoursAllSizes_Change(ComponentSize size)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.DatabaseReview,
            Size = size,
            ChangeType = ChangeType.Change,
            Count = 1
        };
        Assert.Equal(6.10m, row.BaseHoursPerUnit);
    }

    #endregion
}
