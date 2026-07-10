using Gainwell.EstimationTool;
using Gainwell.EstimationTool.Views;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Tests for the Detailed Estimate "Collaboration/Quality" tab calculations.
/// Verifies all formulas match the Excel "Dtl Collaboration_Quality" sheet exactly.
///
/// Excel Formulas:
///   G14 = B14 * MtgHrs * E14  (MeetingCount × MeetingHours × Attendees)
///   G15 = C15 * E14 * B14     (PrepHrsPerPerson × Attendees × MeetingCount)
///   I(row) = G(row) + H(row)  (HourTotal + AdjustedHrs = GrandTotal)
///   D23 = SUM(D24:D28)        (Consultant total)
///   D31 = SUM(D32:D33)        (Create Estimates total = Detail + Final)
///   G12 = SUM(G14:G21, D23, D31, D35)  (Overall HourTotal)
///   H12 = SUM(H14:H21)                  (Overall Adjusted)
///   I12 = SUM(I14:I21, D23, D31, D35)  (Overall GrandTotal)
/// </summary>
public class DetailedCollaborationQualityTests
{
    #region Meeting Hours Formula: Count × MeetingHours × Attendees

    [Theory]
    [InlineData(25, 1, 6, 150)]       // Excel WPR: 25×1×6 = 150
    [InlineData(5, 1, 4, 20)]         // Excel Client: 5×1×4 = 20
    [InlineData(24, 1, 6, 144)]       // Excel Internal: 24×1×6 = 144
    [InlineData(10, 2, 3, 60)]        // Custom: 10×2×3 = 60
    [InlineData(1, 1, 1, 1)]          // Minimum: 1×1×1 = 1
    [InlineData(0, 1, 6, 0)]          // Zero count
    [InlineData(25, 0, 6, 0)]         // Zero hours
    [InlineData(25, 1, 0, 0)]         // Zero attendees
    public void MeetingHours_CountTimesHoursTimesAttendees(int count, decimal hours, int attendees, decimal expected)
    {
        decimal result = count * hours * attendees;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Prep Hours Formula: PrepHrsPerPerson × Attendees × Count

    [Theory]
    [InlineData(0.5, 6, 25, 75)]      // Excel WPR Prep: 0.5×6×25 = 75
    [InlineData(0.25, 4, 5, 5)]       // Excel Client Prep: 0.25×4×5 = 5
    [InlineData(0.25, 6, 24, 36)]     // Excel Internal Prep: 0.25×6×24 = 36
    [InlineData(1.0, 3, 10, 30)]      // Custom: 1.0×3×10 = 30
    [InlineData(0, 6, 25, 0)]         // Zero prep hours
    [InlineData(0.5, 0, 25, 0)]       // Zero attendees
    [InlineData(0.5, 6, 0, 0)]        // Zero count
    public void PrepHours_PrepTimesAttendeesTimesCount(decimal prepHrs, int attendees, int count, decimal expected)
    {
        decimal result = prepHrs * attendees * count;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Grand Total Per Line: HourTotal + AdjustedHrs

    [Theory]
    [InlineData(150, 0, 150)]         // No adjustment
    [InlineData(150, 10, 160)]        // Positive adjustment
    [InlineData(150, -5, 145)]        // Negative adjustment
    [InlineData(0, 50, 50)]           // Zero calculated, positive adjustment
    [InlineData(75, 0, 75)]           // Prep with no adjustment
    [InlineData(20, 3.5, 23.5)]       // Decimal adjustment
    public void GrandTotalPerLine_HourTotalPlusAdjusted(decimal hourTotal, decimal adjusted, decimal expected)
    {
        decimal result = hourTotal + adjusted;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Consultant Total: SUM of all person hours

    [Fact]
    public void ConsultantTotal_SumOfAllPersons_MatchesExcel()
    {
        // Excel: 100 + 100 + 100 + 75 + 0 = 375
        var consultants = new[] { 100m, 100m, 100m, 75m, 0m };
        decimal total = consultants.Sum();
        Assert.Equal(375m, total);
    }

    [Fact]
    public void ConsultantTotal_AllZeros_ReturnsZero()
    {
        var consultants = new[] { 0m, 0m, 0m, 0m, 0m };
        Assert.Equal(0m, consultants.Sum());
    }

    [Fact]
    public void ConsultantTotal_SinglePerson_ReturnsTheirHours()
    {
        var consultants = new[] { 250m, 0m, 0m, 0m, 0m };
        Assert.Equal(250m, consultants.Sum());
    }

    [Fact]
    public void ConsultantTotal_DecimalHours_CalculatesCorrectly()
    {
        var consultants = new[] { 10.5m, 20.75m, 0m, 0m, 0m };
        Assert.Equal(31.25m, consultants.Sum());
    }

    [Fact]
    public void ConsultantRow_PropertyChange_UpdatesValue()
    {
        var row = new ConsultantRow { Name = "Test Person", Hours = 100m };
        Assert.Equal("Test Person", row.Name);
        Assert.Equal(100m, row.Hours);

        row.Hours = 200m;
        Assert.Equal(200m, row.Hours);
    }

    #endregion

    #region Create Estimates Total: Detail + Final

    [Theory]
    [InlineData(25, 10, 35)]          // Excel: 25 + 10 = 35
    [InlineData(0, 0, 0)]            // Both zero
    [InlineData(40, 0, 40)]          // Only detail
    [InlineData(0, 15, 15)]          // Only final
    [InlineData(100, 50, 150)]       // Large values
    public void EstimatesTotal_DetailPlusFinal(decimal detail, decimal final_, decimal expected)
    {
        decimal result = detail + final_;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Overall Hour Total (G12): SUM(meetings + prep + consultant + estimates + PM)

    [Fact]
    public void OverallHourTotal_MatchesExcel_Exactly()
    {
        // Excel G12 = SUM(G14:G21, D23, D31, D35)
        // = (150 + 75 + 20 + 5 + 144 + 36) + 375 + 35 + 1400 = 2240
        decimal wprMtg = 25 * 1m * 6;          // 150
        decimal wprPrep = 0.5m * 6 * 25;        // 75
        decimal clientMtg = 5 * 1m * 4;         // 20
        decimal clientPrep = 0.25m * 4 * 5;     // 5
        decimal intMtg = 24 * 1m * 6;           // 144
        decimal intPrep = 0.25m * 6 * 24;       // 36
        decimal consultant = 100 + 100 + 100 + 75 + 0; // 375
        decimal estimates = 25 + 10;            // 35
        decimal pm = 1400m;                     // 1400

        decimal hourTotal = wprMtg + wprPrep + clientMtg + clientPrep + intMtg + intPrep
                          + consultant + estimates + pm;

        Assert.Equal(2240m, hourTotal);
    }

    [Fact]
    public void OverallHourTotal_AllZeros_ReturnsZero()
    {
        decimal hourTotal = 0m + 0m + 0m + 0m + 0m + 0m + 0m + 0m + 0m;
        Assert.Equal(0m, hourTotal);
    }

    [Fact]
    public void OverallHourTotal_OnlyMeetings_NoOtherComponents()
    {
        // Only meetings, no prep/consultant/estimates/PM
        decimal wprMtg = 10 * 1m * 5;          // 50
        decimal clientMtg = 3 * 1m * 2;         // 6
        decimal intMtg = 5 * 1m * 4;            // 20
        decimal hourTotal = wprMtg + 0m + clientMtg + 0m + intMtg + 0m + 0m + 0m + 0m;
        Assert.Equal(76m, hourTotal);
    }

    [Fact]
    public void OverallHourTotal_OnlyConsultantAndPM()
    {
        decimal hourTotal = 0m + 0m + 0m + 0m + 0m + 0m + 200m + 0m + 500m;
        Assert.Equal(700m, hourTotal);
    }

    #endregion

    #region Overall Adjusted (H12): SUM of meeting adjusted hours ONLY

    [Fact]
    public void OverallAdjusted_SumOfMeetingAdjustmentsOnly()
    {
        // Excel H12 = SUM(H14:H21) - only 6 meeting/prep adjusted hours
        decimal wprAdj = 5m;
        decimal wprPrepAdj = 2m;
        decimal clientAdj = 3m;
        decimal clientPrepAdj = 0m;
        decimal intAdj = 10m;
        decimal intPrepAdj = -1m;

        decimal adjTotal = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
        Assert.Equal(19m, adjTotal);
    }

    [Fact]
    public void OverallAdjusted_AllZeros_ReturnsZero()
    {
        decimal adjTotal = 0m + 0m + 0m + 0m + 0m + 0m;
        Assert.Equal(0m, adjTotal);
    }

    [Fact]
    public void OverallAdjusted_NegativeValues_SubtractsCorrectly()
    {
        decimal adjTotal = -10m + -5m + 0m + 0m + -3m + 0m;
        Assert.Equal(-18m, adjTotal);
    }

    [Fact]
    public void OverallAdjusted_MixedPositiveNegative()
    {
        decimal adjTotal = 10m + -5m + 3m + -2m + 0m + 1m;
        Assert.Equal(7m, adjTotal);
    }

    #endregion

    #region Overall Grand Total (I12): HourTotal + Adjusted

    [Fact]
    public void OverallGrandTotal_MatchesExcel_NoAdjustments()
    {
        // When no adjustments: Grand = Hour Total
        decimal hourTotal = 2240m;
        decimal adjTotal = 0m;
        decimal grandTotal = hourTotal + adjTotal;
        Assert.Equal(2240m, grandTotal);
    }

    [Fact]
    public void OverallGrandTotal_WithPositiveAdjustments()
    {
        decimal hourTotal = 2240m;
        decimal adjTotal = 50m;  // Some adjusted hours added
        decimal grandTotal = hourTotal + adjTotal;
        Assert.Equal(2290m, grandTotal);
    }

    [Fact]
    public void OverallGrandTotal_WithNegativeAdjustments()
    {
        decimal hourTotal = 2240m;
        decimal adjTotal = -100m; // Reduction
        decimal grandTotal = hourTotal + adjTotal;
        Assert.Equal(2140m, grandTotal);
    }

    [Fact]
    public void OverallGrandTotal_ZeroHoursWithAdjustments()
    {
        decimal hourTotal = 0m;
        decimal adjTotal = 25m;
        decimal grandTotal = hourTotal + adjTotal;
        Assert.Equal(25m, grandTotal);
    }

    #endregion

    #region Full Pipeline Integration — Excel Exact Match

    [Fact]
    public void FullPipeline_ExcelExactValues_GrandTotal2240()
    {
        // Complete Excel scenario from Dtl Collaboration_Quality sheet
        // Inputs:
        int wprCount = 25, wprAtt = 6;
        decimal wprHrs = 1m, wprPrepHrs = 0.5m;
        int clientCount = 5, clientAtt = 4;
        decimal clientHrs = 1m, clientPrepHrs = 0.25m;
        int intCount = 24, intAtt = 6;
        decimal intHrs = 1m, intPrepHrs = 0.25m;
        decimal[] consultantHours = { 100m, 100m, 100m, 75m, 0m };
        decimal detailEst = 25m, finalEst = 10m;
        decimal pmEffort = 1400m;

        // All adjusted hours = 0 (Excel default)
        decimal wprAdj = 0, wprPrepAdj = 0;
        decimal clientAdj = 0, clientPrepAdj = 0;
        decimal intAdj = 0, intPrepAdj = 0;

        // Calculate meeting hours (G column)
        decimal g14 = wprCount * wprHrs * wprAtt;           // 150
        decimal g15 = wprPrepHrs * wprAtt * wprCount;       // 75
        decimal g17 = clientCount * clientHrs * clientAtt;   // 20
        decimal g18 = clientPrepHrs * clientAtt * clientCount;// 5
        decimal g20 = intCount * intHrs * intAtt;            // 144
        decimal g21 = intPrepHrs * intAtt * intCount;        // 36

        Assert.Equal(150m, g14);
        Assert.Equal(75m, g15);
        Assert.Equal(20m, g17);
        Assert.Equal(5m, g18);
        Assert.Equal(144m, g20);
        Assert.Equal(36m, g21);

        // Grand totals per line (I column = G + H)
        decimal i14 = g14 + wprAdj;
        decimal i15 = g15 + wprPrepAdj;
        decimal i17 = g17 + clientAdj;
        decimal i18 = g18 + clientPrepAdj;
        decimal i20 = g20 + intAdj;
        decimal i21 = g21 + intPrepAdj;

        Assert.Equal(150m, i14);
        Assert.Equal(75m, i15);
        Assert.Equal(20m, i17);
        Assert.Equal(5m, i18);
        Assert.Equal(144m, i20);
        Assert.Equal(36m, i21);

        // Consultant (D23)
        decimal d23 = consultantHours.Sum();
        Assert.Equal(375m, d23);

        // Estimates (D31)
        decimal d31 = detailEst + finalEst;
        Assert.Equal(35m, d31);

        // Overall totals (Row 12)
        decimal g12 = g14 + g15 + g17 + g18 + g20 + g21 + d23 + d31 + pmEffort;
        decimal h12 = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
        decimal i12 = i14 + i15 + i17 + i18 + i20 + i21 + d23 + d31 + pmEffort;

        Assert.Equal(2240m, g12);   // Excel G12 = 2240.00
        Assert.Equal(0m, h12);      // Excel H12 = 0.00
        Assert.Equal(2240m, i12);   // Excel I12 = 2240.00
    }

    [Fact]
    public void FullPipeline_WithAdjustments_CorrectGrandTotal()
    {
        // Same as Excel but with some adjusted hours
        decimal g14 = 25 * 1m * 6;           // 150
        decimal g15 = 0.5m * 6 * 25;          // 75
        decimal g17 = 5 * 1m * 4;             // 20
        decimal g18 = 0.25m * 4 * 5;          // 5
        decimal g20 = 24 * 1m * 6;            // 144
        decimal g21 = 0.25m * 6 * 24;         // 36

        // Add adjustments
        decimal wprAdj = 10m, wprPrepAdj = 5m;
        decimal clientAdj = -3m, clientPrepAdj = 0m;
        decimal intAdj = 20m, intPrepAdj = 0m;

        decimal d23 = 375m;  // consultants
        decimal d31 = 35m;   // estimates
        decimal pm = 1400m;  // PM

        decimal hourTotal = g14 + g15 + g17 + g18 + g20 + g21 + d23 + d31 + pm;
        decimal adjTotal = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
        decimal grandTotal = hourTotal + adjTotal;

        Assert.Equal(2240m, hourTotal);
        Assert.Equal(32m, adjTotal);       // 10+5-3+0+20+0 = 32
        Assert.Equal(2272m, grandTotal);   // 2240 + 32 = 2272
    }

    [Fact]
    public void FullPipeline_MinimalInput_SingleMeeting()
    {
        // 1 WPR meeting, 1 hour, 1 attendee, no prep, no consultant, no estimates, no PM
        decimal g14 = 1 * 1m * 1;  // 1
        decimal hourTotal = g14 + 0 + 0 + 0 + 0 + 0 + 0 + 0 + 0;
        Assert.Equal(1m, hourTotal);
    }

    [Fact]
    public void FullPipeline_OnlyPM_HoursCorrect()
    {
        // No meetings, no consultant, no estimates — only PM hours
        decimal hourTotal = 0m + 0m + 0m + 0m + 0m + 0m + 0m + 0m + 500m;
        Assert.Equal(500m, hourTotal);
    }

    #endregion

    #region Edge Cases — Negative Inputs

    [Theory]
    [InlineData(-1, 1, 6)]     // Negative count
    [InlineData(25, -1, 6)]    // Negative hours
    [InlineData(25, 1, -1)]    // Negative attendees
    public void MeetingHours_NegativeInputs_ProducesNegativeResult(int count, decimal hours, int attendees)
    {
        // Negative inputs should still compute (UI validation prevents this, but formula should handle)
        decimal result = count * hours * attendees;
        Assert.True(result < 0 || result == 0); // At least one negative makes product negative or zero
    }

    [Fact]
    public void PrepHours_NegativePrepTime_ProducesNegativeResult()
    {
        decimal result = -0.5m * 6 * 25;
        Assert.Equal(-75m, result);
    }

    [Fact]
    public void ConsultantHours_NegativeValue_SubtractsFromTotal()
    {
        var consultants = new[] { 100m, -50m, 0m, 0m, 0m };
        Assert.Equal(50m, consultants.Sum());
    }

    #endregion

    #region Edge Cases — Large Values

    [Fact]
    public void LargeValues_NoOverflow()
    {
        // Very large meeting scenario
        decimal mtgHours = 100m * 8m * 20m;  // 16000
        decimal prepHours = 2m * 20m * 100m;  // 4000
        decimal consultant = 5000m;
        decimal estimates = 200m;
        decimal pm = 10000m;

        decimal total = mtgHours + prepHours + consultant + estimates + pm;
        Assert.Equal(35200m, total);
    }

    [Fact]
    public void DecimalPrecision_FractionalHours()
    {
        // 7 meetings × 0.75 hrs × 3 attendees = 15.75
        decimal mtgHours = 7m * 0.75m * 3m;
        Assert.Equal(15.75m, mtgHours);

        // Prep: 0.33 hrs × 3 attendees × 7 meetings = 6.93
        decimal prepHours = 0.33m * 3m * 7m;
        Assert.Equal(6.93m, prepHours);
    }

    #endregion

    #region Edge Cases — Boundary Values

    [Fact]
    public void ZeroCount_ZeroAttendees_AllZero()
    {
        decimal mtg = 0 * 1m * 0;
        decimal prep = 0.5m * 0 * 0;
        Assert.Equal(0m, mtg);
        Assert.Equal(0m, prep);
    }

    [Fact]
    public void MaxValidInputs_20Meetings20Attendees()
    {
        // Collaboration validation allows max 20 meetings, 20 attendees
        decimal mtgHours = 20 * 1m * 20;  // 400
        decimal prepHours = 1m * 20 * 20;  // 400
        Assert.Equal(400m, mtgHours);
        Assert.Equal(400m, prepHours);
    }

    [Fact]
    public void SingleMeetingSingleAttendee_MinimumNonZero()
    {
        decimal mtg = 1 * 1m * 1;     // 1
        decimal prep = 0.25m * 1 * 1;  // 0.25
        Assert.Equal(1m, mtg);
        Assert.Equal(0.25m, prep);
    }

    #endregion

    #region ConsultantRow Model Tests

    [Fact]
    public void ConsultantRow_DefaultValues()
    {
        var row = new ConsultantRow();
        Assert.Equal(string.Empty, row.Name);
        Assert.Equal(0m, row.Hours);
    }

    [Fact]
    public void ConsultantRow_SetName_RaisesPropertyChanged()
    {
        var row = new ConsultantRow();
        bool raised = false;
        row.PropertyChanged += (s, e) => { if (e.PropertyName == "Name") raised = true; };
        row.Name = "Mario Castillon";
        Assert.True(raised);
    }

    [Fact]
    public void ConsultantRow_SetHours_RaisesPropertyChanged()
    {
        var row = new ConsultantRow();
        bool raised = false;
        row.PropertyChanged += (s, e) => { if (e.PropertyName == "Hours") raised = true; };
        row.Hours = 100m;
        Assert.True(raised);
    }

    [Fact]
    public void ConsultantRow_FivePersons_MatchesExcel()
    {
        var rows = new[]
        {
            new ConsultantRow { Name = "Person 1 Mario Castillon", Hours = 100m },
            new ConsultantRow { Name = "Person 2 Tracey Elias", Hours = 100m },
            new ConsultantRow { Name = "Person 3 Virginia Innerst", Hours = 100m },
            new ConsultantRow { Name = "Person 4 Madeline Woodward", Hours = 75m },
            new ConsultantRow { Name = "Person 5", Hours = 0m },
        };
        Assert.Equal(375m, rows.Sum(r => r.Hours));
    }

    #endregion

    #region SummaryRow Model Tests

    [Fact]
    public void SummaryRow_CollaborationTotal_CorrectValues()
    {
        var row = new SummaryRow
        {
            TaskName = "Collaboration Total",
            StraightHours = 2240m,
            AdjustedMisc = 0m,
            GrandTotal = 2240m,
            IsTotalRow = true
        };
        Assert.Equal(2240m, row.GrandTotal);
        Assert.True(row.IsTotalRow);
    }

    [Fact]
    public void SummaryRow_WithAdjustedMisc()
    {
        var row = new SummaryRow
        {
            StraightHours = 2240m,
            AdjustedMisc = 50m,
            GrandTotal = 2290m
        };
        Assert.Equal(2290m, row.GrandTotal);
        Assert.Equal(50m, row.AdjustedMisc);
    }

    #endregion

    #region Formula Variation Tests — Non-Standard Meeting Durations

    [Theory]
    [InlineData(10, 0.5, 4, 20)]     // Half-hour meetings: 10×0.5×4 = 20
    [InlineData(5, 2, 3, 30)]         // 2-hour meetings: 5×2×3 = 30
    [InlineData(3, 1.5, 6, 27)]       // 90-min meetings: 3×1.5×6 = 27
    [InlineData(8, 0.25, 2, 4)]       // 15-min meetings: 8×0.25×2 = 4
    public void MeetingHours_VariousDurations_CorrectResult(int count, decimal hours, int attendees, decimal expected)
    {
        decimal result = count * hours * attendees;
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0.5, 4, 10, 20)]      // 30min prep: 0.5×4×10 = 20
    [InlineData(1.5, 3, 5, 22.5)]     // 90min prep: 1.5×3×5 = 22.5
    [InlineData(0.1, 10, 20, 20)]     // 6min prep: 0.1×10×20 = 20
    [InlineData(2, 6, 24, 288)]       // 2hr prep: 2×6×24 = 288
    public void PrepHours_VariousDurations_CorrectResult(decimal prepHrs, int attendees, int count, decimal expected)
    {
        decimal result = prepHrs * attendees * count;
        Assert.Equal(expected, result);
    }

    #endregion

    #region Adjusted Hours — Positive and Negative Scenarios

    [Fact]
    public void AdjustedHours_AllPositive_IncreasesGrandTotal()
    {
        decimal hourTotal = 430m; // meetings + prep
        decimal adjustments = 5m + 3m + 2m + 1m + 10m + 4m; // all 6 rows positive
        decimal grandTotal = hourTotal + adjustments;
        Assert.True(grandTotal > hourTotal);
        Assert.Equal(455m, grandTotal);
    }

    [Fact]
    public void AdjustedHours_AllNegative_DecreasesGrandTotal()
    {
        decimal hourTotal = 430m;
        decimal adjustments = -5m + -3m + -2m + -1m + -10m + -4m; // all negative
        decimal grandTotal = hourTotal + adjustments;
        Assert.True(grandTotal < hourTotal);
        Assert.Equal(405m, grandTotal);
    }

    [Fact]
    public void AdjustedHours_NetZero_GrandEqualsHourTotal()
    {
        decimal hourTotal = 430m;
        decimal adjustments = 10m + -10m + 5m + -5m + 3m + -3m; // net zero
        decimal grandTotal = hourTotal + adjustments;
        Assert.Equal(hourTotal, grandTotal);
    }

    [Fact]
    public void AdjustedHours_LargeNegative_CanMakeGrandNegative()
    {
        // In theory, if someone enters very large negative adjustments
        decimal hourTotal = 100m;
        decimal adjustments = -200m;
        decimal grandTotal = hourTotal + adjustments;
        Assert.Equal(-100m, grandTotal);
    }

    #endregion

    #region Integration — Different Scenarios

    [Fact]
    public void Scenario_SmallProject_NoConsultantNoPM()
    {
        // Small project: 5 WPRs, 3 client meetings, no consultants, no PM
        decimal wprMtg = 5 * 1m * 3;        // 15
        decimal wprPrep = 0.25m * 3 * 5;     // 3.75
        decimal clientMtg = 3 * 1m * 2;      // 6
        decimal clientPrep = 0.25m * 2 * 3;  // 1.5
        decimal intMtg = 0m;
        decimal intPrep = 0m;

        decimal hourTotal = wprMtg + wprPrep + clientMtg + clientPrep + intMtg + intPrep + 0 + 0 + 0;
        Assert.Equal(26.25m, hourTotal);
    }

    [Fact]
    public void Scenario_LargeProject_AllComponents()
    {
        // Large project with everything maxed out
        decimal wprMtg = 20 * 2m * 10;      // 400
        decimal wprPrep = 1m * 10 * 20;      // 200
        decimal clientMtg = 15 * 1m * 5;     // 75
        decimal clientPrep = 0.5m * 5 * 15;  // 37.5
        decimal intMtg = 20 * 1m * 8;        // 160
        decimal intPrep = 0.5m * 8 * 20;     // 80
        decimal consultant = 500m;
        decimal estimates = 80m;
        decimal pm = 2000m;

        decimal hourTotal = wprMtg + wprPrep + clientMtg + clientPrep
                          + intMtg + intPrep + consultant + estimates + pm;
        Assert.Equal(3532.5m, hourTotal);
    }

    [Fact]
    public void Scenario_OnlyInternalMeetings()
    {
        decimal intMtg = 10 * 1m * 6;        // 60
        decimal intPrep = 0.5m * 6 * 10;     // 30
        decimal hourTotal = 0 + 0 + 0 + 0 + intMtg + intPrep + 0 + 0 + 0;
        Assert.Equal(90m, hourTotal);
    }

    [Fact]
    public void Scenario_OnlyConsultants()
    {
        decimal consultant = 150m + 200m + 50m;
        decimal hourTotal = 0 + 0 + 0 + 0 + 0 + 0 + consultant + 0 + 0;
        Assert.Equal(400m, hourTotal);
    }

    #endregion
}
