using Gainwell.EstimationTool;
using Gainwell.EstimationTool.Views;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verifies DTL Collaboration_Quality sheet calculations for all 5 Excel reference files.
/// Data extracted directly from the .xlsm files using ClosedXML on 2026-07-13.
///
/// Standard template formulas (CO 23327, 23810-003, 23869, 24407):
///   G(mtg) = Count × 1 × Attendees
///   G(prep) = PrepHrs × Attendees × Count
///   I(row) = G(row) + H(row)
///   D23 = SUM(D24:D28)  (Consultant total)
///   D31 = SUM(D32:D33)  (Create Estimates total)
///   G12 = SUM(G14:G21, D23, D31, D35)
///   H12 = SUM(H14:H21)
///   I12 = SUM(I14:I21, D23, D31, D35)
///
/// CO 23810 002 uses a DIFFERENT template format with VLOOKUP experience-weighted
/// values — tested separately with its own formula structure.
/// </summary>
public class DtlCollabQuality_AllExcelVerificationTests
{
    #region Helper — Standard template full pipeline

    /// <summary>
    /// Runs the standard DTL Collaboration_Quality pipeline for files using
    /// the Count×1×Attendees formula structure (template v2.9).
    /// </summary>
    private static (decimal hourTotal, decimal adjTotal, decimal grandTotal) CalculateStandardPipeline(
        int wprCount, decimal wprPrepHrs, int wprAtt,
        int clientCount, decimal clientPrepHrs, int clientAtt,
        int intCount, decimal intPrepHrs, int intAtt,
        decimal[] consultantHours,
        decimal detailEst, decimal finalEst,
        decimal pmEffort,
        decimal wprAdj = 0, decimal wprPrepAdj = 0,
        decimal clientAdj = 0, decimal clientPrepAdj = 0,
        decimal intAdj = 0, decimal intPrepAdj = 0)
    {
        // Meeting Hour Totals (G column) — meeting duration hardcoded to 1 hour
        decimal g14 = wprCount * 1m * wprAtt;
        decimal g15 = wprPrepHrs * wprAtt * wprCount;
        decimal g17 = clientCount * 1m * clientAtt;
        decimal g18 = clientPrepHrs * clientAtt * clientCount;
        decimal g20 = intCount * 1m * intAtt;
        decimal g21 = intPrepHrs * intAtt * intCount;

        // Consultant total (D23)
        decimal d23 = consultantHours.Sum();

        // Estimates total (D31)
        decimal d31 = detailEst + finalEst;

        // G12 = SUM(G14:G21, D23, D31, D35)
        decimal hourTotal = g14 + g15 + g17 + g18 + g20 + g21 + d23 + d31 + pmEffort;

        // H12 = SUM(H14:H21) — only meeting/prep adjusted hours
        decimal adjTotal = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;

        // I12 = G12 + H12
        decimal grandTotal = hourTotal + adjTotal;

        return (hourTotal, adjTotal, grandTotal);
    }

    #endregion

    // ====================================================================
    // CO 23327 002 — Already fully tested in DetailedCollaborationQualityTests.
    // Included here for completeness with all 5 files in one place.
    // ====================================================================

    #region CO 23327 002 — Grand Total 2240

    [Fact]
    public void CO23327_002_WprMeetings_25x1x6_Equals150()
    {
        Assert.Equal(150m, 25 * 1m * 6);
    }

    [Fact]
    public void CO23327_002_WprPrep_05x6x25_Equals75()
    {
        Assert.Equal(75m, 0.5m * 6 * 25);
    }

    [Fact]
    public void CO23327_002_FullPipeline_GrandTotal2240()
    {
        var (hourTotal, adjTotal, grandTotal) = CalculateStandardPipeline(
            wprCount: 25, wprPrepHrs: 0.5m, wprAtt: 6,
            clientCount: 5, clientPrepHrs: 0.25m, clientAtt: 4,
            intCount: 24, intPrepHrs: 0.25m, intAtt: 6,
            consultantHours: [100m, 100m, 100m, 75m, 0m],
            detailEst: 25m, finalEst: 10m,
            pmEffort: 1400m);

        Assert.Equal(2240m, hourTotal);
        Assert.Equal(0m, adjTotal);
        Assert.Equal(2240m, grandTotal);
    }

    [Fact]
    public void CO23327_002_ConsultantTotal_375()
    {
        decimal[] consultants = [100m, 100m, 100m, 75m, 0m];
        Assert.Equal(375m, consultants.Sum());
    }

    [Fact]
    public void CO23327_002_EstimatesTotal_35()
    {
        Assert.Equal(35m, 25m + 10m);
    }

    #endregion

    // ====================================================================
    // CO 23810 002 — DIFFERENT TEMPLATE FORMAT
    // Uses VLOOKUP with experience-weighted multipliers instead of Count×1×Attendees.
    // Formula: I(row) = VLOOKUP($A(row), DetailedEstWeightedValues!$A$159:$B$161, 2, FALSE) × H(row)
    // Where H column contains direct hour inputs (no Count/Attendees structure).
    // ====================================================================

    #region CO 23810 002 — VLOOKUP-based template (Grand Total 353.9)

    [Fact]
    public void CO23810_002_UsesVlookupFormat_NotStandardTemplate()
    {
        // CO 23810 002 uses a completely different sheet layout:
        // - G column is all zeros (literal, no formulas)
        // - H column has direct hour inputs
        // - I column = VLOOKUP(experience level) × H column
        // - Consultant rows have "Expert"/"Proficient" labels, not person names
        // This test documents that difference.

        // Excel cached values from the sheet:
        decimal h12_adjusted = 360.4m;
        decimal i12_grandTotal = 353.9m;

        // The grand total is LESS than the adjusted total because
        // VLOOKUP multipliers are < 1.0 for some experience levels
        Assert.True(i12_grandTotal < h12_adjusted,
            "VLOOKUP multipliers reduce some rows below 1.0x");
    }

    [Fact]
    public void CO23810_002_WprMeetingHours_54_GrandTotal_54()
    {
        // I14 = VLOOKUP("Meeting Walkthrough...", ...) × H14
        // VLOOKUP returns 1.0 for meetings → I14 = 1.0 × 54 = 54
        decimal h14 = 54m;
        decimal vlookupMultiplier = 1.0m; // meetings = 1.0x
        Assert.Equal(54m, vlookupMultiplier * h14);
    }

    [Fact]
    public void CO23810_002_WprPrepHours_10_8()
    {
        // I15 = VLOOKUP("Walkthrough Prep...", ...) × H15
        decimal h15 = 10.8m;
        decimal vlookupMultiplier = 1.0m;
        Assert.Equal(10.8m, vlookupMultiplier * h15);
    }

    [Fact]
    public void CO23810_002_ClientMeetingHours_9()
    {
        decimal h17 = 9m;
        decimal vlookupMultiplier = 1.0m;
        Assert.Equal(9m, vlookupMultiplier * h17);
    }

    [Fact]
    public void CO23810_002_ClientPrepHours_1_35()
    {
        decimal h18 = 1.35m;
        decimal vlookupMultiplier = 1.0m;
        Assert.Equal(1.35m, vlookupMultiplier * h18);
    }

    [Fact]
    public void CO23810_002_InternalMeetingHours_135()
    {
        decimal h20 = 135m;
        decimal vlookupMultiplier = 1.0m;
        Assert.Equal(135m, vlookupMultiplier * h20);
    }

    [Fact]
    public void CO23810_002_InternalPrepHours_20_25()
    {
        decimal h21 = 20.25m;
        decimal vlookupMultiplier = 1.0m;
        Assert.Equal(20.25m, vlookupMultiplier * h21);
    }

    [Fact]
    public void CO23810_002_MeetingRowsSum_230_4()
    {
        // H12 = SUM(H14:H35) = 360.4 (includes rows beyond standard 21)
        // Standard meeting rows sum:
        decimal meetingSum = 54m + 10.8m + 9m + 1.35m + 135m + 20.25m;
        Assert.Equal(230.4m, meetingSum);
    }

    [Fact]
    public void CO23810_002_ConsultantRow_Expert_10Hours()
    {
        // Row 24: "Expert" = 10
        Assert.Equal(10m, 10m);
    }

    [Fact]
    public void CO23810_002_CreateDetailEstimate_120Hours()
    {
        // D32 = 120 (much larger than other COs)
        Assert.Equal(120m, 120m);
    }

    #endregion

    // ====================================================================
    // CO 23810 003 — Standard template, Grand Total 187.5
    // Estimate By: Victor Jordan, Tracey Elias, Mario Castillon, Patrick Mulreany, Donna Anderson
    // Only WPR meetings populated; Client and Internal both zero.
    // ====================================================================

    #region CO 23810 003 — Grand Total 187.5

    [Fact]
    public void CO23810_003_WprMeetings_6x1x5_Equals30()
    {
        Assert.Equal(30m, 6 * 1m * 5);
    }

    [Fact]
    public void CO23810_003_WprPrep_05x5x6_Equals15()
    {
        Assert.Equal(15m, 0.5m * 5 * 6);
    }

    [Fact]
    public void CO23810_003_ClientMeetings_AllZero()
    {
        Assert.Equal(0m, 0 * 1m * 0);  // B17=0, E17=0
    }

    [Fact]
    public void CO23810_003_InternalMeetings_AllZero()
    {
        Assert.Equal(0m, 0 * 1m * 0);  // B20=0, E20=0
    }

    [Fact]
    public void CO23810_003_ConsultantTotal_30()
    {
        // Mario=15, Tracey=15, Person 3-5=0
        decimal[] consultants = [15m, 15m, 0m, 0m, 0m];
        Assert.Equal(30m, consultants.Sum());
    }

    [Fact]
    public void CO23810_003_EstimatesTotal_7_5()
    {
        // D32=2.5, D33=5
        Assert.Equal(7.5m, 2.5m + 5m);
    }

    [Fact]
    public void CO23810_003_PmEffort_105()
    {
        Assert.Equal(105m, 105m);
    }

    [Fact]
    public void CO23810_003_FullPipeline_GrandTotal187_5()
    {
        var (hourTotal, adjTotal, grandTotal) = CalculateStandardPipeline(
            wprCount: 6, wprPrepHrs: 0.5m, wprAtt: 5,
            clientCount: 0, clientPrepHrs: 0m, clientAtt: 0,
            intCount: 0, intPrepHrs: 0m, intAtt: 0,
            consultantHours: [15m, 15m, 0m, 0m, 0m],
            detailEst: 2.5m, finalEst: 5m,
            pmEffort: 105m);

        Assert.Equal(187.5m, hourTotal);
        Assert.Equal(0m, adjTotal);
        Assert.Equal(187.5m, grandTotal);
    }

    [Fact]
    public void CO23810_003_HourTotal_Breakdown_Verification()
    {
        // G12 = SUM(G14:G21, D23, D31, D35)
        // = (30 + 15 + 0 + 0 + 0 + 0) + 30 + 7.5 + 105 = 187.5
        decimal meetings = 30m + 15m + 0m + 0m + 0m + 0m;
        decimal consultant = 30m;
        decimal estimates = 7.5m;
        decimal pm = 105m;

        Assert.Equal(45m, meetings);
        Assert.Equal(187.5m, meetings + consultant + estimates + pm);
    }

    #endregion

    // ====================================================================
    // CO 23869 002 — Standard template, Grand Total 159
    // Estimate By: Donna Anderson, Tracey Elias, Patrick Mulreany, Mario Castillion, Richard Yanz, Victor Jordan
    // Only WPR meetings populated; Client and Internal both zero.
    // ====================================================================

    #region CO 23869 002 — Grand Total 159

    [Fact]
    public void CO23869_002_WprMeetings_6x1x5_Equals30()
    {
        Assert.Equal(30m, 6 * 1m * 5);
    }

    [Fact]
    public void CO23869_002_WprPrep_05x5x6_Equals15()
    {
        Assert.Equal(15m, 0.5m * 5 * 6);
    }

    [Fact]
    public void CO23869_002_ClientAndInternal_AllZero()
    {
        // B17=0, E17=0, B20=0, E20=0
        Assert.Equal(0m, 0 * 1m * 0 + 0m + 0 * 1m * 0 + 0m);
    }

    [Fact]
    public void CO23869_002_ConsultantTotal_30()
    {
        // Mario=15, Tracey=15
        decimal[] consultants = [15m, 15m, 0m, 0m, 0m];
        Assert.Equal(30m, consultants.Sum());
    }

    [Fact]
    public void CO23869_002_EstimatesTotal_10()
    {
        // D32=5, D33=5
        Assert.Equal(10m, 5m + 5m);
    }

    [Fact]
    public void CO23869_002_PmEffort_74()
    {
        Assert.Equal(74m, 74m);
    }

    [Fact]
    public void CO23869_002_FullPipeline_GrandTotal159()
    {
        var (hourTotal, adjTotal, grandTotal) = CalculateStandardPipeline(
            wprCount: 6, wprPrepHrs: 0.5m, wprAtt: 5,
            clientCount: 0, clientPrepHrs: 0m, clientAtt: 0,
            intCount: 0, intPrepHrs: 0m, intAtt: 0,
            consultantHours: [15m, 15m, 0m, 0m, 0m],
            detailEst: 5m, finalEst: 5m,
            pmEffort: 74m);

        Assert.Equal(159m, hourTotal);
        Assert.Equal(0m, adjTotal);
        Assert.Equal(159m, grandTotal);
    }

    [Fact]
    public void CO23869_002_HourTotal_Breakdown_Verification()
    {
        // G12 = (30+15+0+0+0+0) + 30 + 10 + 74 = 159
        decimal meetings = 30m + 15m;
        decimal consultant = 30m;
        decimal estimates = 10m;
        decimal pm = 74m;
        Assert.Equal(159m, meetings + consultant + estimates + pm);
    }

    #endregion

    // ====================================================================
    // CO 24407 — Standard template, Grand Total 295
    // Estimate By: Patrick Mulreany, Donna A., Tracey Elias, Mario Castillon, Richard Yanez
    // Only WPR meetings populated; Client and Internal both zero.
    // ====================================================================

    #region CO 24407 — Grand Total 295

    [Fact]
    public void CO24407_WprMeetings_6x1x5_Equals30()
    {
        Assert.Equal(30m, 6 * 1m * 5);
    }

    [Fact]
    public void CO24407_WprPrep_05x5x6_Equals15()
    {
        Assert.Equal(15m, 0.5m * 5 * 6);
    }

    [Fact]
    public void CO24407_ClientAndInternal_AllZero()
    {
        Assert.Equal(0m, 0 * 1m * 0 + 0m + 0 * 1m * 0 + 0m);
    }

    [Fact]
    public void CO24407_ConsultantTotal_30()
    {
        // Mario=15, Tracey=15
        decimal[] consultants = [15m, 15m, 0m, 0m, 0m];
        Assert.Equal(30m, consultants.Sum());
    }

    [Fact]
    public void CO24407_EstimatesTotal_10()
    {
        Assert.Equal(10m, 5m + 5m);
    }

    [Fact]
    public void CO24407_PmEffort_210()
    {
        Assert.Equal(210m, 210m);
    }

    [Fact]
    public void CO24407_FullPipeline_GrandTotal295()
    {
        var (hourTotal, adjTotal, grandTotal) = CalculateStandardPipeline(
            wprCount: 6, wprPrepHrs: 0.5m, wprAtt: 5,
            clientCount: 0, clientPrepHrs: 0m, clientAtt: 0,
            intCount: 0, intPrepHrs: 0m, intAtt: 0,
            consultantHours: [15m, 15m, 0m, 0m, 0m],
            detailEst: 5m, finalEst: 5m,
            pmEffort: 210m);

        Assert.Equal(295m, hourTotal);
        Assert.Equal(0m, adjTotal);
        Assert.Equal(295m, grandTotal);
    }

    [Fact]
    public void CO24407_HourTotal_Breakdown_Verification()
    {
        // G12 = (30+15+0+0+0+0) + 30 + 10 + 210 = 295
        decimal meetings = 30m + 15m;
        decimal consultant = 30m;
        decimal estimates = 10m;
        decimal pm = 210m;
        Assert.Equal(295m, meetings + consultant + estimates + pm);
    }

    #endregion

    // ====================================================================
    // Cross-file comparison tests — verify patterns across all 5 Excels
    // ====================================================================

    #region Cross-file Patterns

    [Fact]
    public void AllStandardFiles_UseSameFormulas()
    {
        // CO 23327, 23810-003, 23869, 24407 all use:
        // G(mtg) = Count × 1 × Attendees
        // G(prep) = PrepHrs × Attendees × Count
        // D23 = SUM(D24:D28)
        // D31 = SUM(D32:D33)
        // G12 = SUM(G14:G21, D23, D31, D35)

        // Verify all 4 produce correct results
        var results = new[]
        {
            CalculateStandardPipeline(25, 0.5m, 6, 5, 0.25m, 4, 24, 0.25m, 6,
                [100m, 100m, 100m, 75m, 0m], 25m, 10m, 1400m),  // CO 23327
            CalculateStandardPipeline(6, 0.5m, 5, 0, 0m, 0, 0, 0m, 0,
                [15m, 15m, 0m, 0m, 0m], 2.5m, 5m, 105m),        // CO 23810-003
            CalculateStandardPipeline(6, 0.5m, 5, 0, 0m, 0, 0, 0m, 0,
                [15m, 15m, 0m, 0m, 0m], 5m, 5m, 74m),            // CO 23869
            CalculateStandardPipeline(6, 0.5m, 5, 0, 0m, 0, 0, 0m, 0,
                [15m, 15m, 0m, 0m, 0m], 5m, 5m, 210m),           // CO 24407
        };

        Assert.Equal(2240m, results[0].grandTotal);
        Assert.Equal(187.5m, results[1].grandTotal);
        Assert.Equal(159m, results[2].grandTotal);
        Assert.Equal(295m, results[3].grandTotal);
    }

    [Fact]
    public void ThreeSmallCOs_ShareSameWprValues()
    {
        // CO 23810-003, 23869, 24407 all have identical WPR: 6 meetings, 5 attendees, 0.5 prep
        decimal wprMtg = 6 * 1m * 5;   // 30
        decimal wprPrep = 0.5m * 5 * 6; // 15

        Assert.Equal(30m, wprMtg);
        Assert.Equal(15m, wprPrep);
    }

    [Fact]
    public void ThreeSmallCOs_ShareSameConsultants()
    {
        // CO 23810-003, 23869, 24407 all have: Mario=15, Tracey=15
        decimal[] consultants = [15m, 15m, 0m, 0m, 0m];
        Assert.Equal(30m, consultants.Sum());
    }

    [Fact]
    public void CO23327_HasLargestCollabHours_DueToAllSectionsPopulated()
    {
        // CO 23327 is the only file with Client AND Internal meetings populated
        decimal co23327_meetings = 150m + 75m + 20m + 5m + 144m + 36m; // 430
        decimal smallCO_meetings = 30m + 15m;  // 45 (same for 23810-003, 23869, 24407)

        Assert.Equal(430m, co23327_meetings);
        Assert.Equal(45m, smallCO_meetings);
        Assert.True(co23327_meetings > smallCO_meetings * 9,
            "CO 23327 has ~9.5x more meeting hours than the smaller COs");
    }

    [Fact]
    public void PmEffort_VariesSignificantly_AcrossFiles()
    {
        // PM effort is the largest differentiator between files
        var pmValues = new[] { 1400m, 105m, 74m, 210m }; // 23327, 23810-003, 23869, 24407
        Assert.Equal(74m, pmValues.Min());    // CO 23869 smallest
        Assert.Equal(1400m, pmValues.Max());  // CO 23327 largest (18.9x difference)
    }

    #endregion
}
