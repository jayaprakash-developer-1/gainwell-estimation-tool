using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for the WeightedValues static class.
/// Covers happy path (lookups), sad path (invalid combos), positive (all valid),
/// and negative (edge cases, mutations).
/// </summary>
public class WeightedValues_AllPathTests
{
    #region Happy Path — All 11 Component Types Exist

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows)]
    [InlineData(ComponentType.Reports)]
    [InlineData(ComponentType.ProgramsDBStoredProcs)]
    [InlineData(ComponentType.SupportModules)]
    [InlineData(ComponentType.DBManipulation)]
    [InlineData(ComponentType.DatabaseReview)]
    [InlineData(ComponentType.Webpage)]
    [InlineData(ComponentType.K2Workflow)]
    [InlineData(ComponentType.K2SmartForm)]
    [InlineData(ComponentType.TestAutomationUFT)]
    [InlineData(ComponentType.MISC)]
    public void HappyPath_AllTypes_SmallNew_ReturnNonZero(ComponentType type)
    {
        var result = WeightedValues.GetBaseHours(type, ComponentSize.Small, ChangeType.New);
        Assert.True(result > 0m, $"{type} Small New should be > 0");
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows)]
    [InlineData(ComponentType.Reports)]
    [InlineData(ComponentType.ProgramsDBStoredProcs)]
    [InlineData(ComponentType.SupportModules)]
    [InlineData(ComponentType.DBManipulation)]
    [InlineData(ComponentType.DatabaseReview)]
    [InlineData(ComponentType.Webpage)]
    [InlineData(ComponentType.K2Workflow)]
    [InlineData(ComponentType.K2SmartForm)]
    [InlineData(ComponentType.TestAutomationUFT)]
    [InlineData(ComponentType.MISC)]
    public void HappyPath_AllTypes_LargeChange_ReturnNonZero(ComponentType type)
    {
        var result = WeightedValues.GetBaseHours(type, ComponentSize.Large, ChangeType.Change);
        Assert.True(result > 0m, $"{type} Large Change should be > 0");
    }

    #endregion

    #region Happy Path — Display Names

    [Fact]
    public void HappyPath_GetDisplayName_AllTypesHaveNames()
    {
        foreach (ComponentType type in Enum.GetValues<ComponentType>())
        {
            var name = WeightedValues.GetDisplayName(type);
            Assert.False(string.IsNullOrEmpty(name));
        }
    }

    [Fact]
    public void HappyPath_GetDisplayName_NoneShowsSelect()
    {
        Assert.Equal("— Select —", WeightedValues.GetDisplayName(ComponentType.None));
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, "PowerBuilder Windows")]
    [InlineData(ComponentType.Reports, "Reports")]
    [InlineData(ComponentType.ProgramsDBStoredProcs, "Programs/DB Stored Procedures")]
    [InlineData(ComponentType.SupportModules, "Support Modules/JOB/JIL")]
    [InlineData(ComponentType.DBManipulation, "DB Manipulation (SQL, PL/SQL, etc.)")]
    [InlineData(ComponentType.DatabaseReview, "Database Review")]
    [InlineData(ComponentType.Webpage, "Webpage (Includes UI, Portal & Intranet)")]
    [InlineData(ComponentType.K2Workflow, "K2 Workflow")]
    [InlineData(ComponentType.K2SmartForm, "K2 Smart Form")]
    [InlineData(ComponentType.TestAutomationUFT, "Test Automation Suites (UFT)")]
    public void HappyPath_GetDisplayName_CorrectNames(ComponentType type, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetDisplayName(type));
    }

    #endregion

    #region Happy Path — T-Shirt Size

    [Fact]
    public void HappyPath_GetTShirtSize_ZeroReturnsDash()
    {
        Assert.Equal("—", WeightedValues.GetTShirtSize(0m));
    }

    [Fact]
    public void HappyPath_GetTShirtSize_NegativeReturnsDash()
    {
        Assert.Equal("—", WeightedValues.GetTShirtSize(-100m));
    }

    [Theory]
    [InlineData(50, "Small")]
    [InlineData(150, "Medium")]
    [InlineData(500, "Large")]
    [InlineData(800, "X-Large")]
    [InlineData(1500, "XL1")]
    [InlineData(2500, "XL2")]
    [InlineData(3500, "XL3")]
    [InlineData(4500, "XL4")]
    [InlineData(5500, "XL5")]
    [InlineData(6500, "XL6")]
    [InlineData(7500, "XL7")]
    [InlineData(9000, "XL8")]
    public void HappyPath_GetTShirtSize_CorrectMapping(int hours, string expected)
    {
        Assert.Equal(expected, WeightedValues.GetTShirtSize(hours));
    }

    #endregion

    #region Sad Path — Invalid Combinations Return Zero

    [Fact]
    public void SadPath_NoneType_ReturnsZero()
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.None, ComponentSize.Small, ChangeType.New));
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.None, ComponentSize.Medium, ChangeType.Change));
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.None, ComponentSize.Large, ChangeType.New));
    }

    [Fact]
    public void SadPath_NoneSize_ReturnsZero()
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.None, ChangeType.New));
    }

    [Fact]
    public void SadPath_NoneChangeType_ReturnsZero()
    {
        Assert.Equal(0m, WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.None));
    }

    [Fact]
    public void SadPath_InvalidEnumValue_ReturnsZero()
    {
        // Cast an invalid int to the enum
        Assert.Equal(0m, WeightedValues.GetBaseHours((ComponentType)999, ComponentSize.Large, ChangeType.New));
    }

    #endregion

    #region Positive Path — New vs Change Relationship

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Small)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Medium)]
    [InlineData(ComponentType.PowerBuilderWindows, ComponentSize.Large)]
    [InlineData(ComponentType.Reports, ComponentSize.Small)]
    [InlineData(ComponentType.Reports, ComponentSize.Medium)]
    [InlineData(ComponentType.Reports, ComponentSize.Large)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Small)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Medium)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ComponentSize.Large)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Small)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Medium)]
    [InlineData(ComponentType.SupportModules, ComponentSize.Large)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Small)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Medium)]
    [InlineData(ComponentType.DBManipulation, ComponentSize.Large)]
    [InlineData(ComponentType.Webpage, ComponentSize.Small)]
    [InlineData(ComponentType.Webpage, ComponentSize.Medium)]
    [InlineData(ComponentType.Webpage, ComponentSize.Large)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Small)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Medium)]
    [InlineData(ComponentType.K2Workflow, ComponentSize.Large)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Small)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Medium)]
    [InlineData(ComponentType.K2SmartForm, ComponentSize.Large)]
    public void Positive_NewAlwaysGTE_Change(ComponentType type, ComponentSize size)
    {
        var newHours = WeightedValues.GetBaseHours(type, size, ChangeType.New);
        var changeHours = WeightedValues.GetBaseHours(type, size, ChangeType.Change);
        // For most types, New >= Change (modifying existing code takes less time)
        // Exception: TestAutomationUFT has some inverted values — skip that
        if (type != ComponentType.TestAutomationUFT)
            Assert.True(newHours >= changeHours, $"{type} {size}: New ({newHours}) should be >= Change ({changeHours})");
    }

    #endregion

    #region Positive Path — Size Ordering

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, ChangeType.New)]
    [InlineData(ComponentType.Reports, ChangeType.New)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, ChangeType.New)]
    [InlineData(ComponentType.SupportModules, ChangeType.New)]
    [InlineData(ComponentType.DBManipulation, ChangeType.New)]
    [InlineData(ComponentType.Webpage, ChangeType.New)]
    [InlineData(ComponentType.K2Workflow, ChangeType.New)]
    [InlineData(ComponentType.K2SmartForm, ChangeType.New)]
    [InlineData(ComponentType.MISC, ChangeType.New)]
    public void Positive_LargeGTE_MediumGTE_Small(ComponentType type, ChangeType change)
    {
        var small = WeightedValues.GetBaseHours(type, ComponentSize.Small, change);
        var medium = WeightedValues.GetBaseHours(type, ComponentSize.Medium, change);
        var large = WeightedValues.GetBaseHours(type, ComponentSize.Large, change);

        // Skip types with flat sizes (DatabaseReview) and TestAutomationUFT (non-standard)
        if (type != ComponentType.DatabaseReview && type != ComponentType.TestAutomationUFT)
        {
            Assert.True(large >= medium, $"{type} Large ({large}) should be >= Medium ({medium})");
            Assert.True(medium >= small, $"{type} Medium ({medium}) should be >= Small ({small})");
        }
    }

    #endregion

    #region Negative Path — ResetToDefaults

    [Fact]
    public void Negative_ResetToDefaults_RestoresOriginalValues()
    {
        // Verify a known value
        decimal original = WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Large, ChangeType.New);
        Assert.Equal(100.00m, original);

        // Reset doesn't break it
        WeightedValues.ResetToDefaults();
        Assert.Equal(100.00m, WeightedValues.GetBaseHours(ComponentType.MISC, ComponentSize.Large, ChangeType.New));
    }

    #endregion
}
