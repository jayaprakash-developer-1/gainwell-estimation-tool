using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Happy path, sad path, positive, and negative tests for CollaborationRowViewModel.
/// </summary>
public class CollaborationRowViewModel_AllPathTests
{
    #region Happy Path — Standard Calculations

    [Fact]
    public void HappyPath_StandardWPR_CalculatesCorrectly()
    {
        var row = new CollaborationRowViewModel
        {
            CollabType = CollaborationType.WPRs,
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 15
        };
        // 10 × (60/60 + 15/60) × 3 = 10 × 1.25 × 3 = 37.50
        Assert.Equal(37.50m, row.TotalHours);
    }

    [Fact]
    public void HappyPath_ClientMeeting_60Min()
    {
        var row = new CollaborationRowViewModel
        {
            CollabType = CollaborationType.ClientMeetings,
            NumberOfMeetings = 5,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 4,
            ParticipantPrepTimeMinutes = 30
        };
        // 5 × (60/60 + 30/60) × 4 = 5 × 1.5 × 4 = 30
        Assert.Equal(30.00m, row.TotalHours);
    }

    [Fact]
    public void HappyPath_InternalMeeting_15Min()
    {
        var row = new CollaborationRowViewModel
        {
            CollabType = CollaborationType.InternalMeetings,
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 0
        };
        // 20 × (15/60 + 0) × 5 = 20 × 0.25 × 5 = 25
        Assert.Equal(25.00m, row.TotalHours);
    }

    [Fact]
    public void HappyPath_AutomationTest_45Min()
    {
        var row = new CollaborationRowViewModel
        {
            CollabType = CollaborationType.AutomationTestCollaboration,
            NumberOfMeetings = 8,
            MeetingDurationMinutes = 45,
            NumberOfParticipants = 2,
            ParticipantPrepTimeMinutes = 15
        };
        // 8 × (45/60 + 15/60) × 2 = 8 × 1.0 × 2 = 16
        Assert.Equal(16.00m, row.TotalHours);
    }

    [Theory]
    [InlineData(1, 60, 1, 0, 1.00)]    // Minimum meaningful meeting
    [InlineData(20, 60, 20, 180, 1600.00)] // Maximum all params
    [InlineData(10, 30, 5, 30, 50.00)]  // Moderate values
    public void HappyPath_VariousCombinations(int meetings, int duration, int participants, int prep, decimal expected)
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = meetings,
            MeetingDurationMinutes = duration,
            NumberOfParticipants = participants,
            ParticipantPrepTimeMinutes = prep
        };
        Assert.Equal(expected, row.TotalHours);
    }

    #endregion

    #region Sad Path — Zero Values

    [Fact]
    public void SadPath_ZeroMeetings_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 30
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_ZeroParticipants_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 30
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_ZeroDurationAndZeroPrep_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 0
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void SadPath_AllZeros_ZeroHours()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 0
        };
        Assert.Equal(0m, row.TotalHours);
    }

    #endregion

    #region Positive Path — Only Duration (No Prep)

    [Fact]
    public void Positive_OnlyDuration_NoPrepTime()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 0
        };
        // 10 × (60/60 + 0) × 3 = 30
        Assert.Equal(30.00m, row.TotalHours);
    }

    [Fact]
    public void Positive_OnlyPrepTime_NoDuration()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 60
        };
        // 10 × (0 + 60/60) × 3 = 30
        Assert.Equal(30.00m, row.TotalHours);
    }

    #endregion

    #region Positive Path — Maximum Boundary Values

    [Fact]
    public void Positive_MaxMeetings()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 1,
            ParticipantPrepTimeMinutes = 0
        };
        Assert.Equal(20.00m, row.TotalHours);
    }

    [Fact]
    public void Positive_MaxParticipants()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 1,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 20,
            ParticipantPrepTimeMinutes = 0
        };
        Assert.Equal(20.00m, row.TotalHours);
    }

    [Fact]
    public void Positive_MaxPrepTime()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 1,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 1,
            ParticipantPrepTimeMinutes = 180
        };
        // 1 × (0 + 180/60) × 1 = 3.00
        Assert.Equal(3.00m, row.TotalHours);
    }

    [Fact]
    public void Positive_AllMaxValues()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 20,
            ParticipantPrepTimeMinutes = 180
        };
        // 20 × (60/60 + 180/60) × 20 = 20 × 4 × 20 = 1600
        Assert.Equal(1600.00m, row.TotalHours);
    }

    #endregion

    #region Negative Path — Validation

    [Fact]
    public void Negative_MeetingDuration_InvalidValue_ValidationFails()
    {
        // Valid values: 0, 15, 30, 45, 60
        var result = CollaborationRowViewModel.ValidateMeetingDuration(25, null!);
        Assert.NotNull(result);
        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    [InlineData(60)]
    public void Negative_MeetingDuration_ValidValues_Pass(int duration)
    {
        var result = CollaborationRowViewModel.ValidateMeetingDuration(duration, null!);
        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void Negative_PrepTime_NotMultipleOf15_ValidationFails()
    {
        var result = CollaborationRowViewModel.ValidatePrepTime(10, null!);
        Assert.NotNull(result);
        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void Negative_PrepTime_Over180_ValidationFails()
    {
        var result = CollaborationRowViewModel.ValidatePrepTime(195, null!);
        Assert.NotNull(result);
        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Fact]
    public void Negative_PrepTime_NegativeValue_ValidationFails()
    {
        var result = CollaborationRowViewModel.ValidatePrepTime(-15, null!);
        Assert.NotNull(result);
        Assert.NotEqual(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(120)]
    [InlineData(150)]
    [InlineData(180)]
    public void Negative_PrepTime_ValidValues_Pass(int prepTime)
    {
        var result = CollaborationRowViewModel.ValidatePrepTime(prepTime, null!);
        Assert.Equal(System.ComponentModel.DataAnnotations.ValidationResult.Success, result);
    }

    #endregion

    #region Positive Path — Rounding Verification

    [Fact]
    public void Positive_FractionalResult_RoundsUp()
    {
        // 7 × (15/60 + 30/60) × 3 = 7 × 0.75 × 3 = 15.75 (exact)
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 7,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 3,
            ParticipantPrepTimeMinutes = 30
        };
        Assert.Equal(15.75m, row.TotalHours);
    }

    [Fact]
    public void Positive_RepeatingDecimal_RoundsUpCorrectly()
    {
        // 1 × (15/60 + 15/60) × 1 = 0.5 (exact, no rounding needed)
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 1,
            MeetingDurationMinutes = 15,
            NumberOfParticipants = 1,
            ParticipantPrepTimeMinutes = 15
        };
        Assert.Equal(0.50m, row.TotalHours);
    }

    #endregion
}
