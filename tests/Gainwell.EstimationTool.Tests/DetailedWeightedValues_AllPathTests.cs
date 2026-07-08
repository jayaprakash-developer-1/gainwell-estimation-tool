using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for DetailedWeightedValues static class.
/// Covers SE values, BA values, and all component type/phase/status/complexity combinations.
/// </summary>
public class DetailedWeightedValues_AllPathTests
{
    #region Happy Path — SE Values Exist

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 4)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 12)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 20)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 6)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 12.5)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 0.5)]
    public void HappyPath_SE_PowerBuilder_New(ComponentType type, SeTaskPhase phase, ComponentStatus status, Complexity complexity, decimal expected)
    {
        var result = DetailedWeightedValues.GetSeHours(type, phase, status, complexity);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 3.25)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 9.75)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 16)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 5)]
    [InlineData(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 2)]
    public void HappyPath_SE_PowerBuilder_Existing(ComponentType type, SeTaskPhase phase, ComponentStatus status, Complexity complexity, decimal expected)
    {
        var result = DetailedWeightedValues.GetSeHours(type, phase, status, complexity);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 4.8)]
    [InlineData(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 14.4)]
    [InlineData(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 24)]
    [InlineData(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 2)]
    public void HappyPath_SE_Reports_New(ComponentType type, SeTaskPhase phase, ComponentStatus status, Complexity complexity, decimal expected)
    {
        var result = DetailedWeightedValues.GetSeHours(type, phase, status, complexity);
        Assert.Equal(expected, result);
    }

    #endregion

    #region Positive Path — Existing Values are Less than or Equal to New

    [Theory]
    [InlineData(SeTaskPhase.Analysis, Complexity.Simple)]
    [InlineData(SeTaskPhase.Analysis, Complexity.Moderate)]
    [InlineData(SeTaskPhase.Analysis, Complexity.Complex)]
    [InlineData(SeTaskPhase.GenerateTechnicalDesign, Complexity.Simple)]
    [InlineData(SeTaskPhase.GenerateTechnicalDesign, Complexity.Moderate)]
    [InlineData(SeTaskPhase.GenerateTechnicalDesign, Complexity.Complex)]
    public void Positive_SE_PowerBuilder_ExistingLTE_New(SeTaskPhase phase, Complexity complexity)
    {
        var newHours = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, phase, ComponentStatus.New, complexity);
        var existingHours = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, phase, ComponentStatus.Existing, complexity);
        Assert.True(existingHours <= newHours, $"Existing ({existingHours}) should be <= New ({newHours}) for {phase} {complexity}");
    }

    #endregion

    #region Positive Path — Complexity Ordering

    [Theory]
    [InlineData(SeTaskPhase.Analysis, ComponentStatus.New)]
    [InlineData(SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New)]
    [InlineData(SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New)]
    [InlineData(SeTaskPhase.Analysis, ComponentStatus.Existing)]
    public void Positive_SE_PowerBuilder_ComplexGTE_Moderate_GTE_Simple(SeTaskPhase phase, ComponentStatus status)
    {
        var simple = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, phase, status, Complexity.Simple);
        var moderate = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, phase, status, Complexity.Moderate);
        var complex = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, phase, status, Complexity.Complex);

        Assert.True(complex >= moderate, $"Complex ({complex}) should be >= Moderate ({moderate})");
        Assert.True(moderate >= simple, $"Moderate ({moderate}) should be >= Simple ({simple})");
    }

    #endregion

    #region Sad Path — Invalid Combinations

    [Fact]
    public void SadPath_SE_InvalidPhaseForType_ReturnsZero()
    {
        // SQL-specific phases shouldn't have values for PowerBuilder
        var result = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void SadPath_SE_SelectStatus_ReturnsZero()
    {
        var result = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Select, Complexity.Simple);
        Assert.Equal(0m, result);
    }

    #endregion

    #region Negative Path — All Hours Non-Negative

    [Fact]
    public void Negative_AllSeHours_NonNegative()
    {
        foreach (ComponentType ct in new[] { ComponentType.PowerBuilderWindows, ComponentType.Reports })
        {
            foreach (SeTaskPhase phase in Enum.GetValues<SeTaskPhase>())
            {
                foreach (ComponentStatus status in Enum.GetValues<ComponentStatus>())
                {
                    foreach (Complexity cx in Enum.GetValues<Complexity>())
                    {
                        var hours = DetailedWeightedValues.GetSeHours(ct, phase, status, cx);
                        Assert.True(hours >= 0m, $"SE hours for {ct}/{phase}/{status}/{cx} = {hours} should be >= 0");
                    }
                }
            }
        }
    }

    #endregion
}
