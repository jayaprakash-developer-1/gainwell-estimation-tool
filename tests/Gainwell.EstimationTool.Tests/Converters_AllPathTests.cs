using System.Globalization;
using System.Windows;
using Gainwell.EstimationTool.Converters;
using Gainwell.EstimationTool.Models;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive happy/sad/positive/negative path tests for all WPF converters.
/// </summary>
public class Converters_AllPathTests
{
    #region ComponentTypeDisplayConverter

    [Fact]
    public void ComponentTypeDisplay_HappyPath_ConvertsType()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(ComponentType.PowerBuilderWindows, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("PowerBuilder Windows", result);
    }

    [Fact]
    public void ComponentTypeDisplay_SadPath_NullInput()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert(null!, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ComponentTypeDisplay_Negative_NonEnumType()
    {
        var converter = new ComponentTypeDisplayConverter();
        var result = converter.Convert("not an enum", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("not an enum", result);
    }

    #endregion

    #region DecimalFormatConverter

    [Fact]
    public void DecimalFormat_HappyPath_FormatsDecimal()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(123.456m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("123.46", result);
    }

    [Fact]
    public void DecimalFormat_HappyPath_Zero()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert(0m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalFormat_SadPath_NonDecimal()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.Convert("not a decimal", typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0.00", result);
    }

    [Fact]
    public void DecimalFormat_ConvertBack_ValidString()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("123.45", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(123.45m, result);
    }

    [Fact]
    public void DecimalFormat_ConvertBack_InvalidString()
    {
        var converter = new DecimalFormatConverter();
        var result = converter.ConvertBack("abc", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    #endregion

    #region AdjustedHoursConverter

    [Fact]
    public void AdjustedHours_HappyPath_NonZero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(25.5m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("25.5", result);
    }

    [Fact]
    public void AdjustedHours_HappyPath_Zero()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.Convert(0m, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("0", result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_EmptyString()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_Dash()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("-", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_Dot()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack(".", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_ValidNumber()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("15.75", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(15.75m, result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_NegativeNumber()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("-10.5", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(-10.5m, result);
    }

    [Fact]
    public void AdjustedHours_ConvertBack_Whitespace()
    {
        var converter = new AdjustedHoursConverter();
        var result = converter.ConvertBack("   ", typeof(decimal), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0m, result);
    }

    #endregion

    #region ZeroToVisibilityConverter

    [Fact]
    public void ZeroToVisibility_HappyPath_ZeroReturnsVisible()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert(0, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void ZeroToVisibility_HappyPath_NonZeroReturnsCollapsed()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert(5, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void ZeroToVisibility_SadPath_NotInt()
    {
        var converter = new ZeroToVisibilityConverter();
        var result = converter.Convert("not an int", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    #endregion

    #region XMarkConverter

    [Fact]
    public void XMark_HappyPath_ZeroReturnsEmpty()
    {
        var converter = new XMarkConverter();
        var result = converter.Convert(0, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void XMark_HappyPath_PositiveReturnsX()
    {
        var converter = new XMarkConverter();
        var result = converter.Convert(1, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("X", result);
    }

    [Fact]
    public void XMark_ConvertBack_XReturns1()
    {
        var converter = new XMarkConverter();
        var result = converter.ConvertBack("X", typeof(int), null!, CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    [Fact]
    public void XMark_ConvertBack_LowercaseXReturns1()
    {
        var converter = new XMarkConverter();
        var result = converter.ConvertBack("x", typeof(int), null!, CultureInfo.InvariantCulture);
        Assert.Equal(1, result);
    }

    [Fact]
    public void XMark_ConvertBack_EmptyReturns0()
    {
        var converter = new XMarkConverter();
        var result = converter.ConvertBack("", typeof(int), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0, result);
    }

    [Fact]
    public void XMark_ConvertBack_OtherStringReturns0()
    {
        var converter = new XMarkConverter();
        var result = converter.ConvertBack("Y", typeof(int), null!, CultureInfo.InvariantCulture);
        Assert.Equal(0, result);
    }

    #endregion

    #region NullToCollapsedConverter

    [Fact]
    public void NullToCollapsed_HappyPath_NonEmpty_Visible()
    {
        var converter = new NullToCollapsedConverter();
        var result = converter.Convert("text", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void NullToCollapsed_SadPath_EmptyString_Collapsed()
    {
        var converter = new NullToCollapsedConverter();
        var result = converter.Convert("", typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void NullToCollapsed_SadPath_Null_Collapsed()
    {
        var converter = new NullToCollapsedConverter();
        var result = converter.Convert(null!, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    #endregion

    #region BoolToVisibilityConverter

    [Fact]
    public void BoolToVisibility_True_Visible()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    [Fact]
    public void BoolToVisibility_False_Collapsed()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void BoolToVisibility_ConvertBack_Visible_True()
    {
        var converter = new BoolToVisibilityConverter();
        var result = converter.ConvertBack(Visibility.Visible, typeof(bool), null!, CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    #endregion

    #region InverseBoolToVisibilityConverter

    [Fact]
    public void InverseBoolToVisibility_True_Collapsed()
    {
        var converter = new InverseBoolToVisibilityConverter();
        var result = converter.Convert(true, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Collapsed, result);
    }

    [Fact]
    public void InverseBoolToVisibility_False_Visible()
    {
        var converter = new InverseBoolToVisibilityConverter();
        var result = converter.Convert(false, typeof(Visibility), null!, CultureInfo.InvariantCulture);
        Assert.Equal(Visibility.Visible, result);
    }

    #endregion

    #region NotesCharCountConverter

    [Fact]
    public void NotesCharCount_HappyPath_ShortString()
    {
        var converter = new NotesCharCountConverter();
        var result = converter.Convert("Hello", typeof(string), "1000", CultureInfo.InvariantCulture);
        Assert.Equal("5/1000", result);
    }

    [Fact]
    public void NotesCharCount_SadPath_AtLimit()
    {
        var converter = new NotesCharCountConverter();
        var text = new string('A', 1000);
        var result = converter.Convert(text, typeof(string), "1000", CultureInfo.InvariantCulture);
        Assert.Contains("limit reached", (string)result);
    }

    [Fact]
    public void NotesCharCount_SadPath_EmptyString()
    {
        var converter = new NotesCharCountConverter();
        var result = converter.Convert("", typeof(string), "1000", CultureInfo.InvariantCulture);
        Assert.Equal("0/1000", result);
    }

    [Fact]
    public void NotesCharCount_Negative_NullValue()
    {
        var converter = new NotesCharCountConverter();
        var result = converter.Convert(null!, typeof(string), "1000", CultureInfo.InvariantCulture);
        Assert.Equal("0/1000", result);
    }

    #endregion

    #region IntEqualityConverter

    [Fact]
    public void IntEquality_HappyPath_Equal_True()
    {
        var converter = new IntEqualityConverter();
        var result = converter.Convert(3, typeof(bool), "3", CultureInfo.InvariantCulture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void IntEquality_HappyPath_NotEqual_False()
    {
        var converter = new IntEqualityConverter();
        var result = converter.Convert(2, typeof(bool), "3", CultureInfo.InvariantCulture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void IntEquality_ConvertBack_True_ReturnsParameter()
    {
        var converter = new IntEqualityConverter();
        var result = converter.ConvertBack(true, typeof(int), "3", CultureInfo.InvariantCulture);
        Assert.Equal(3, result);
    }

    #endregion

    #region ExperienceLevelConverter

    [Fact]
    public void ExperienceLevel_HappyPath_NewToArea()
    {
        var converter = new ExperienceLevelConverter();
        var result = converter.Convert(ExperienceLevel.NewToArea, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("New to Area", result);
    }

    [Fact]
    public void ExperienceLevel_HappyPath_Expert()
    {
        var converter = new ExperienceLevelConverter();
        var result = converter.Convert(ExperienceLevel.Expert, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("Expert", result);
    }

    [Fact]
    public void ExperienceLevel_ConvertBack_NewToArea()
    {
        var converter = new ExperienceLevelConverter();
        var result = converter.ConvertBack("New to Area", typeof(ExperienceLevel), null!, CultureInfo.InvariantCulture);
        Assert.Equal(ExperienceLevel.NewToArea, result);
    }

    [Fact]
    public void ExperienceLevel_ConvertBack_UnknownString()
    {
        var converter = new ExperienceLevelConverter();
        var result = converter.ConvertBack("Unknown", typeof(ExperienceLevel), null!, CultureInfo.InvariantCulture);
        Assert.Equal(ExperienceLevel.SelectALevel, result);
    }

    #endregion

    #region EnumDisplayConverter

    [Fact]
    public void EnumDisplay_ComponentType_None_Select()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(ComponentType.None, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("— Select —", result);
    }

    [Fact]
    public void EnumDisplay_ChangeType_None_Select()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(ChangeType.None, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("— Select —", result);
    }

    [Fact]
    public void EnumDisplay_ComponentSize_None_Select()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(ComponentSize.None, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("— Select —", result);
    }

    [Fact]
    public void EnumDisplay_CollaborationType_WPRs()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(CollaborationType.WPRs, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("WPRs", result);
    }

    [Fact]
    public void EnumDisplay_ChangeType_New()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(ChangeType.New, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("New", result);
    }

    [Fact]
    public void EnumDisplay_ComponentSize_Large()
    {
        var converter = new EnumDisplayConverter();
        var result = converter.Convert(ComponentSize.Large, typeof(string), null!, CultureInfo.InvariantCulture);
        Assert.Equal("Large", result);
    }

    #endregion
}
