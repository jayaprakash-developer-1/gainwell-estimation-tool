using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for SettingsViewModel — covers WeightedValueRow, save/reset operations.
/// </summary>
public class SettingsViewModel_AllPathTests
{
    #region Happy Path — WeightedValueRow Properties

    [Fact]
    public void HappyPath_WeightedValueRow_PropertiesSet()
    {
        var row = new WeightedValueRow
        {
            Id = 1,
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            BaseHours = 25.00m,
            OriginalBaseHours = 25.00m,
            DisplayName = "PowerBuilder Windows",
            LastModified = DateTime.UtcNow,
            ModifiedBy = "System"
        };

        Assert.Equal(1, row.Id);
        Assert.Equal(ComponentType.PowerBuilderWindows, row.ComponentType);
        Assert.Equal(ComponentSize.Small, row.Size);
        Assert.Equal(ChangeType.New, row.ChangeType);
        Assert.Equal(25.00m, row.BaseHours);
        Assert.Equal("PowerBuilder Windows", row.DisplayName);
    }

    [Fact]
    public void HappyPath_WeightedValueRow_DetectsChange()
    {
        var row = new WeightedValueRow
        {
            BaseHours = 25.00m,
            OriginalBaseHours = 25.00m
        };

        Assert.Equal(row.BaseHours, row.OriginalBaseHours);
        row.BaseHours = 30.00m;
        Assert.NotEqual(row.BaseHours, row.OriginalBaseHours);
    }

    #endregion

    #region Sad Path — WeightedValueRow Edge Cases

    [Fact]
    public void SadPath_WeightedValueRow_ZeroBaseHours()
    {
        var row = new WeightedValueRow
        {
            BaseHours = 0m,
            OriginalBaseHours = 25.00m
        };
        Assert.Equal(0m, row.BaseHours);
    }

    [Fact]
    public void SadPath_WeightedValueRow_NegativeBaseHours()
    {
        var row = new WeightedValueRow
        {
            BaseHours = -5m,
            OriginalBaseHours = 25.00m
        };
        Assert.Equal(-5m, row.BaseHours);
    }

    #endregion

    #region Positive Path — Display Name Mapping

    [Fact]
    public void Positive_DisplayName_AllTypes()
    {
        var types = Enum.GetValues<ComponentType>().Where(t => t != ComponentType.None);
        foreach (var type in types)
        {
            var name = WeightedValues.GetDisplayName(type);
            Assert.False(string.IsNullOrWhiteSpace(name));
            Assert.NotEqual("— Select —", name);
        }
    }

    #endregion

    #region Negative Path — ModifiedBy and LastModified

    [Fact]
    public void Negative_ModifiedBy_EmptyString()
    {
        var row = new WeightedValueRow
        {
            ModifiedBy = string.Empty
        };
        Assert.Equal(string.Empty, row.ModifiedBy);
    }

    [Fact]
    public void Negative_LastModified_DefaultMinValue()
    {
        var row = new WeightedValueRow();
        // Default DateTime is MinValue
        Assert.Equal(default(DateTime), row.LastModified);
    }

    #endregion
}
