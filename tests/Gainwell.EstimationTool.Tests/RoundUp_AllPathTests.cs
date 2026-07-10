using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for the RoundUp function used throughout calculations.
/// Tests exact match, boundary values, precision, and edge cases.
/// </summary>
public class RoundUp_AllPathTests
{
    #region Happy Path — Basic Rounding

    [Fact]
    public void HappyPath_Zero_ReturnsZero()
    {
        Assert.Equal(0m, InitialEstimateViewModel.RoundUp(0m));
    }

    [Fact]
    public void HappyPath_ExactTwoDecimals_NoChange()
    {
        Assert.Equal(10.50m, InitialEstimateViewModel.RoundUp(10.50m));
    }

    [Fact]
    public void HappyPath_ExactWholeNumber_NoChange()
    {
        Assert.Equal(100.00m, InitialEstimateViewModel.RoundUp(100.00m));
    }

    [Fact]
    public void HappyPath_ThirdDecimal_RoundsUp()
    {
        // 10.501 → 10.51
        Assert.Equal(10.51m, InitialEstimateViewModel.RoundUp(10.501m));
    }

    [Fact]
    public void HappyPath_ManyDecimals_RoundsUp()
    {
        // 10.5000001 → 10.51
        Assert.Equal(10.51m, InitialEstimateViewModel.RoundUp(10.5000001m));
    }

    #endregion

    #region Positive Path — Various Precision Levels

    [Theory]
    [InlineData(0.001, 0.01)]
    [InlineData(0.009, 0.01)]
    [InlineData(0.01, 0.01)]
    [InlineData(0.011, 0.02)]
    [InlineData(1.234, 1.24)]
    [InlineData(1.235, 1.24)]
    [InlineData(1.239, 1.24)]
    [InlineData(99.999, 100.00)]
    [InlineData(100.001, 100.01)]
    public void Positive_VariousValues_RoundsUpCorrectly(decimal input, decimal expected)
    {
        Assert.Equal(expected, InitialEstimateViewModel.RoundUp(input));
    }

    [Theory]
    [InlineData(30.00)]
    [InlineData(22.50)]
    [InlineData(100.00)]
    [InlineData(0.50)]
    [InlineData(1000.00)]
    public void Positive_ExactValues_NotRounded(decimal value)
    {
        Assert.Equal(value, InitialEstimateViewModel.RoundUp(value));
    }

    #endregion

    #region Negative Path — Negative Values

    [Fact]
    public void Negative_NegativeExact_NoChange()
    {
        // RoundUp(-10.50) = -10.50 (exact)
        Assert.Equal(-10.50m, InitialEstimateViewModel.RoundUp(-10.50m));
    }

    [Fact]
    public void Negative_NegativeThirdDecimal_RoundsAwayFromZero()
    {
        // -10.501 → shifted = -1050.1, truncated = -1050
        // shifted < truncated → (truncated - 1) / 100 = -1051/100 = -10.51
        Assert.Equal(-10.51m, InitialEstimateViewModel.RoundUp(-10.501m));
    }

    [Fact]
    public void Negative_NegativeSmall_RoundsAwayFromZero()
    {
        // -0.001 → shifted = -0.1, truncated = 0
        // shifted < truncated → (0 - 1) / 100 = -0.01
        Assert.Equal(-0.01m, InitialEstimateViewModel.RoundUp(-0.001m));
    }

    #endregion

    #region Sad Path — Very Large Values

    [Fact]
    public void SadPath_VeryLargeValue_DoesNotOverflow()
    {
        decimal large = 999999.991m;
        Assert.Equal(1000000.00m, InitialEstimateViewModel.RoundUp(large));
    }

    [Fact]
    public void SadPath_VerySmallFraction_RoundsUp()
    {
        decimal tiny = 0.0001m;
        Assert.Equal(0.01m, InitialEstimateViewModel.RoundUp(tiny));
    }

    #endregion

    #region Happy Path — Excel Formula Verification

    [Fact]
    public void HappyPath_ExcelExample_SystemTesting()
    {
        // 953.10 * 0.30 = 285.93 (exact)
        Assert.Equal(285.93m, InitialEstimateViewModel.RoundUp(953.10m * 0.30m));
    }

    [Fact]
    public void HappyPath_ExcelExample_Analysis()
    {
        // (953.10 + 285.93) * 0.05 = 1239.03 * 0.05 = 61.9515
        // RoundUp(61.9515) = 61.96
        Assert.Equal(61.96m, InitialEstimateViewModel.RoundUp(1239.03m * 0.05m));
    }

    [Fact]
    public void HappyPath_ExcelExample_BusinessDesign()
    {
        // (953.10 + 285.93) * 0.15 = 1239.03 * 0.15 = 185.8545
        // RoundUp(185.8545) = 185.86
        Assert.Equal(185.86m, InitialEstimateViewModel.RoundUp(1239.03m * 0.15m));
    }

    [Fact]
    public void HappyPath_ExcelExample_CO23327_SystemTesting()
    {
        // Test case formula: (931.0625 + 75.91875) * 2.5 = 2517.453125
        // RoundUp(2517.453125) = 2517.46
        Assert.Equal(2517.46m, InitialEstimateViewModel.RoundUp(2517.453125m));
    }

    [Fact]
    public void HappyPath_ExcelExample_CO23327_PMEffort()
    {
        // 4301.17 * 0.15 = 645.1755
        // RoundUp(645.1755) = 645.18
        Assert.Equal(645.18m, InitialEstimateViewModel.RoundUp(4301.17m * 0.15m));
    }

    #endregion

    #region Boundary — Decimal Precision

    [Fact]
    public void Boundary_TrailingZeros_TreatedAsExact()
    {
        Assert.Equal(1.10m, InitialEstimateViewModel.RoundUp(1.10m));
        Assert.Equal(1.10m, InitialEstimateViewModel.RoundUp(1.100m));
        Assert.Equal(1.10m, InitialEstimateViewModel.RoundUp(1.1000000m));
    }

    [Fact]
    public void Boundary_HalfCentRoundsUp()
    {
        // 1.005 → 1.01 (rounds up, not banker's rounding)
        Assert.Equal(1.01m, InitialEstimateViewModel.RoundUp(1.005m));
    }

    [Fact]
    public void Boundary_OneThird()
    {
        // 1/3 ≈ 0.3333... → RoundUp = 0.34
        decimal oneThird = 1m / 3m;
        Assert.Equal(0.34m, InitialEstimateViewModel.RoundUp(oneThird));
    }

    [Fact]
    public void Boundary_TwoThirds()
    {
        // 2/3 ≈ 0.6666... → RoundUp = 0.67
        decimal twoThirds = 2m / 3m;
        Assert.Equal(0.67m, InitialEstimateViewModel.RoundUp(twoThirds));
    }

    #endregion
}
