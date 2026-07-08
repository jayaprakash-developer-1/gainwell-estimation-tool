using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Negative path tests for ComponentRowViewModel — verifying behavior with
/// invalid, boundary, and error-prone inputs.
/// </summary>
public class ComponentRowViewModel_NegativeTests
{
    #region None Enum Values

    [Fact]
    public void Negative_AllNone_ZeroHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.None,
            Size = ComponentSize.None,
            ChangeType = ChangeType.None,
            Count = 0
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void Negative_TypeNone_WithValidOthers_ZeroHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.None,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 10
        };
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void Negative_SizeNone_WithValidOthers_ZeroHours()
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
    public void Negative_ChangeTypeNone_WithValidOthers_ZeroHours()
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

    #endregion

    #region Zero and Negative Count

    [Fact]
    public void Negative_ZeroCount_ZeroBaseHours()
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

    [Fact]
    public void Negative_NegativeCount_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = -1
        };
        // Count <= 0 triggers UpdateBaseHours to set BaseHoursPerUnit = 0
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    #endregion

    #region Transition from Valid to Invalid

    [Fact]
    public void Negative_ValidToNone_BaseHoursBecomesZero()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.Reports,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1
        };
        Assert.Equal(85.00m, row.BaseHoursPerUnit);

        row.ComponentType = ComponentType.None;
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void Negative_ValidCountToZero_BaseHoursBecomesZero()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.Reports,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 5
        };
        Assert.Equal(85.00m, row.BaseHoursPerUnit);

        row.Count = 0;
        Assert.Equal(0m, row.BaseHoursPerUnit);
        Assert.Equal(0m, row.TotalHours);
    }

    #endregion

    #region Description and Notes — Edge Cases

    [Fact]
    public void Negative_EmptyDescription_NoImpactOnCalculation()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1,
            Description = ""
        };
        Assert.Equal(100.00m, row.TotalHours);
    }

    [Fact]
    public void Negative_LongNotes_NoImpactOnCalculation()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1,
            Notes = new string('A', 1000)
        };
        Assert.Equal(100.00m, row.TotalHours);
    }

    #endregion

    #region RequirementId — No Impact on Calculation

    [Fact]
    public void Negative_EmptyRequirementId_NoImpactOnCalculation()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1,
            RequirementId = ""
        };
        Assert.Equal(100.00m, row.TotalHours);
    }

    [Fact]
    public void Negative_SpecialCharsRequirementId_NoImpactOnCalculation()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1,
            RequirementId = "<script>alert('xss')</script>"
        };
        Assert.Equal(100.00m, row.TotalHours);
    }

    #endregion

    #region LineNumber — No Impact on Calculation

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(100)]
    [InlineData(-1)]
    public void Negative_LineNumber_NoImpactOnCalculation(int lineNumber)
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 1,
            LineNumber = lineNumber
        };
        Assert.Equal(100.00m, row.TotalHours);
    }

    #endregion
}
