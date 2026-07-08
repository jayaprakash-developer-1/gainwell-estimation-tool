using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Tests for the EstimateNavigator static class.
/// Since it manages WPF windows, we test the constants and public contract.
/// </summary>
public class EstimateNavigator_Tests
{
    #region Happy Path — Constants

    [Fact]
    public void HappyPath_WindowWidth_Is1500()
    {
        Assert.Equal(1500, EstimateNavigator.WindowWidth);
    }

    [Fact]
    public void HappyPath_WindowHeight_Is1000()
    {
        Assert.Equal(1000, EstimateNavigator.WindowHeight);
    }

    #endregion

    #region Positive Path — Constants Are Reasonable

    [Fact]
    public void Positive_WindowWidth_GreaterThanMinimum()
    {
        Assert.True(EstimateNavigator.WindowWidth >= 800);
    }

    [Fact]
    public void Positive_WindowHeight_GreaterThanMinimum()
    {
        Assert.True(EstimateNavigator.WindowHeight >= 600);
    }

    [Fact]
    public void Positive_AspectRatio_WiderThanTall()
    {
        Assert.True(EstimateNavigator.WindowWidth > EstimateNavigator.WindowHeight);
    }

    #endregion
}
