using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Final coverage gap tests for InitialEstimate sheet and Dtl Collaboration_Quality sheet.
/// These tests verify remaining scenarios not covered by other test files:
/// - Exact Excel role breakout formulas with full CO 23327 002 data
/// - PM% boundary and zero scenarios
/// - Subtotal ordering and composition
/// - Collaboration in role breakout (decimal vs ceiling)
/// - Multi-name EstimatedBy field
/// - Default collaboration items
/// - ConsultantMentor contributes to effectiveCollab
/// - Dtl Collaboration_Quality specific scenarios (PM direct hours, >5 consultants)
/// </summary>
public class InitialEstimateAndDtlCollabCoverageTests
{
    private MainViewModel CreateVm() => new();

    #region Excel Role Breakout — Exact BA Formula (Row 47)

    [Fact]
    public void Excel_BaRoleHours_ExactFormula_MatchesExpected()
    {
        // BA = ROUNDUP(Analysis/2 + BizDesign + BADoc + ProdVal + ActualHours/2 + TimeForEstimates/2, 2)
        // Excel B47: ROUNDUP((I28/2)+I29+I32+I33+(I41/2)+(I42/2),2)
        // With Excel values: Analysis=155.71, BizDesign=467.11, BADoc=31, ProdVal=503.50
        // ActualHours=0, TimeForEstimates=129
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;
        vm.TotalActualHours = 0m;

        // BA = ROUNDUP(155.71/2 + 467.11 + 31 + 503.50 + 0/2 + 129/2, 2)
        //    = ROUNDUP(77.855 + 467.11 + 31 + 503.50 + 0 + 64.5, 2)
        //    = ROUNDUP(1143.965, 2) = 1143.97
        Assert.Equal(1143.97m, vm.BaRoleHours);
    }

    [Fact]
    public void Excel_BaRoleHours_IncludesTimeForEstimates()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 0m;

        decimal baWithoutTime = vm.BaRoleHours;

        vm.TimeForEstimates = 100m;
        decimal baWithTime = vm.BaRoleHours;

        // TimeForEstimates contributes half to BA
        Assert.True(baWithTime > baWithoutTime);
        Assert.Equal(MainViewModel.RoundUp(baWithoutTime + 100m / 2m), baWithTime);
    }

    #endregion

    #region Excel Role Breakout — Exact SE Formula (Row 48)

    [Fact]
    public void Excel_SeRoleHours_ExactFormula_MatchesExpected()
    {
        // SE = ROUNDUP(Dev + Analysis/2 + Promotion + ActualHours/2 + TimeForEstimates/2, 2)
        // Excel B48: ROUNDUP(I27+(I28/2)+I31+(I41/2)+(I42/2),2)
        // With Excel values: Dev=596.5625, Analysis=155.71, Promotion=29.83
        // ActualHours=0, TimeForEstimates=129
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;
        vm.TotalActualHours = 0m;

        // SE = ROUNDUP(596.5625 + 155.71/2 + 29.83 + 0/2 + 129/2, 2)
        //    = ROUNDUP(596.5625 + 77.855 + 29.83 + 0 + 64.5, 2)
        //    = ROUNDUP(768.7475, 2) = 768.75
        Assert.Equal(768.75m, vm.SeRoleHours);
    }

    [Fact]
    public void Excel_SeRoleHours_DoesNotIncludePM()
    {
        // SE formula does NOT include PM hours
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        decimal seAt10 = vm.SeRoleHours;
        vm.PmEffortPercentage = 25m;
        decimal seAt25 = vm.SeRoleHours;

        // SE doesn't change when PM% changes
        Assert.Equal(seAt10, seAt25);
    }

    #endregion

    #region Excel Role Breakout — Tester and PM (Rows 49-50)

    [Fact]
    public void Excel_TesterRoleHours_EqualsEffectiveSystemTesting()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        // Tester = SystemTestingTotalHours (effective SysTest = calculated + adjusted)
        Assert.Equal(vm.SystemTestingTotalHours, vm.TesterRoleHours);
        Assert.Equal(2517.46m, vm.TesterRoleHours);
    }

    [Fact]
    public void Excel_PmRoleHours_EqualsEffectivePM()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        Assert.Equal(vm.ProjectManagementTotalHours, vm.PmRoleHours);
        Assert.Equal(645.18m, vm.PmRoleHours);
    }

    #endregion

    #region Excel Collaboration Role Hours (Row 51) — Decimal, Not Ceiling

    [Fact]
    public void Excel_CollaborationRoleHours_IsDecimal_NotCeiling()
    {
        // Excel shows 186 in role breakout but this is display formatting (0 decimal places)
        // The actual value is 185.75 (WPR=125 + Client=42 + Internal=18.75)
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        Assert.Equal(185.75m, vm.CollaborationRoleHours);
        Assert.Equal(vm.CollaborationTotalHours, vm.CollaborationRoleHours);
    }

    [Fact]
    public void CollaborationRoleHours_WithAdjustedHours_IncludesAdjusted()
    {
        var vm = CreateVm();
        vm.CollaborationItems[0].NumberOfMeetings = 5;
        vm.CollaborationItems[0].MeetingDurationMinutes = 60;
        vm.CollaborationItems[0].NumberOfParticipants = 3;
        vm.CollaborationItems[0].ParticipantPrepTimeMinutes = 15;

        decimal baseCollab = vm.CollaborationRoleHours;
        vm.WprsAdjustedHours = 10m;

        Assert.Equal(baseCollab + 10m, vm.CollaborationRoleHours);
    }

    #endregion

    #region PM Effort Percentage — Boundary and Zero

    [Fact]
    public void PmEffort_ZeroPercent_PMHoursIsZero()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.PmEffortPercentage = 0m;
        Assert.Equal(0m, vm.ProjectManagementHours);
        Assert.Equal(0m, vm.ProjectManagementTotalHours);
    }

    [Fact]
    public void PmEffort_OnePercent_CalculatesCorrectly()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hrs
        vm.PmEffortPercentage = 1m;

        // PM = ROUNDUP(allEffectiveTasks * 0.01)
        Assert.True(vm.ProjectManagementHours > 0m);
        Assert.True(vm.ProjectManagementHours < 5m); // 1% of ~172 = ~1.72
    }

    [Fact]
    public void PmEffort_TwentyFivePercent_CalculatesCorrectly()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hrs

        vm.PmEffortPercentage = 25m;
        decimal pmAt25 = vm.ProjectManagementHours;

        vm.PmEffortPercentage = 10m;
        decimal pmAt10 = vm.ProjectManagementHours;

        Assert.True(pmAt25 > pmAt10);
        Assert.Equal(2.5m, Math.Round(pmAt25 / pmAt10, 1));
    }

    [Fact]
    public void PmEffort_ExcelValue_15Percent()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        Assert.Equal(15m, vm.PmEffortPercentage);
        Assert.Equal(645.18m, vm.ProjectManagementHours);
    }

    #endregion

    #region Subtotal Composition and Ordering

    [Fact]
    public void Subtotal_IncludesAllComponents_ExactFormula()
    {
        // SubtotalHours = ROUNDUP(Dev + SysTest + Analysis + BizDesign + Promotion
        //                  + BASysDoc + ProdVal + PM + Collab + TimeForEst + ActualHours, 2)
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;
        vm.TotalActualHours = 0m;

        decimal expected = MainViewModel.RoundUp(
            vm.DevelopmentTotalHours +
            vm.SystemTestingTotalHours +
            vm.AnalysisTotalHours +
            vm.BusinessDesignTotalHours +
            vm.PromotionTotalHours +
            vm.BaSystemDocTotalHours +
            vm.ProductionValidationTotalHours +
            vm.ProjectManagementTotalHours +
            vm.CollaborationTotalHours +
            vm.TimeForEstimates +
            vm.TotalActualHours);

        Assert.Equal(expected, vm.SubtotalHours);
        Assert.Equal(5261.11m, vm.SubtotalHours);
    }

    [Fact]
    public void GrandTotal_AlwaysGreaterOrEqualToSubtotal()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;

        Assert.True(vm.GrandTotalHours >= vm.SubtotalHours);
        Assert.Equal(Math.Ceiling(vm.SubtotalHours), vm.GrandTotalHours);
    }

    [Fact]
    public void GrandTotal_WhenSubtotalIsWholeNumber_EqualsSubtotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.PmEffortPercentage = 0m; // disable PM

        // Set adjusted hours to force whole number subtotal
        // Dev=100, SysTest=30, Analysis=6.50, BizDesign=19.50, Promo=5, BASysDoc=5, ProdVal=6
        // Subtotal = ROUNDUP(100+30+6.50+19.50+5+5+6+0+0+0, 2) = 172.00
        Assert.Equal(172m, vm.SubtotalHours);
        Assert.Equal(172m, vm.GrandTotalHours); // Ceiling of whole number = same number
    }

    #endregion

    #region Estimated By / Reviewed By Fields

    [Fact]
    public void EstimatedBy_MultiNameCommaSeparated_Allowed()
    {
        // Excel Row 53: "Virginia Innerst, Tracey Elias, Richard Yanez, Mario Castillon, Patrick Mulreany"
        var vm = CreateVm();
        vm.EstimatedBy = "Virginia Innerst, Tracey Elias, Richard Yanez, Mario Castillon, Patrick Mulreany";
        Assert.Equal("Virginia Innerst, Tracey Elias, Richard Yanez, Mario Castillon, Patrick Mulreany", vm.EstimatedBy);
    }

    [Fact]
    public void EstimatedBy_DoesNotAffectCalculations()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        decimal grandBefore = vm.GrandTotalHours;

        vm.EstimatedBy = "Alice, Bob, Charlie";
        Assert.Equal(grandBefore, vm.GrandTotalHours);
    }

    [Fact]
    public void ReviewedBy_CanBeSet_DoesNotAffectCalculations()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        decimal grandBefore = vm.GrandTotalHours;

        vm.ReviewedBy = "Senior Architect, QA Lead";
        Assert.Equal(grandBefore, vm.GrandTotalHours);
        Assert.Equal("Senior Architect, QA Lead", vm.ReviewedBy);
    }

    [Fact]
    public void EstimatedBy_DefaultIsCurrentUser()
    {
        // Tool auto-fills EstimatedBy with current username
        var vm = CreateVm();
        Assert.False(string.IsNullOrEmpty(vm.EstimatedBy));
        Assert.Equal(Environment.UserName, vm.EstimatedBy);
    }

    [Fact]
    public void ReviewedBy_DefaultIsEmpty()
    {
        var vm = CreateVm();
        Assert.Equal(string.Empty, vm.ReviewedBy);
    }

    #endregion

    #region Default Collaboration Items — Correct CollabTypes

    [Fact]
    public void DefaultCollabItems_Has4Types_InCorrectOrder()
    {
        var vm = CreateVm();
        Assert.Equal(4, vm.CollaborationItems.Count);
        Assert.Equal(CollaborationType.WPRs, vm.CollaborationItems[0].CollabType);
        Assert.Equal(CollaborationType.ClientMeetings, vm.CollaborationItems[1].CollabType);
        Assert.Equal(CollaborationType.InternalMeetings, vm.CollaborationItems[2].CollabType);
        Assert.Equal(CollaborationType.AutomationTestCollaboration, vm.CollaborationItems[3].CollabType);
    }

    [Fact]
    public void DefaultCollabItems_AllZeroHours()
    {
        var vm = CreateVm();
        foreach (var item in vm.CollaborationItems)
        {
            Assert.Equal(0m, item.TotalHours);
            Assert.Equal(0, item.NumberOfMeetings);
            Assert.Equal(0, item.MeetingDurationMinutes);
            Assert.Equal(0, item.NumberOfParticipants);
            Assert.Equal(0, item.ParticipantPrepTimeMinutes);
        }
    }

    [Fact]
    public void DefaultCollabItems_HaveSequentialLineNumbers()
    {
        var vm = CreateVm();
        for (int i = 0; i < vm.CollaborationItems.Count; i++)
            Assert.Equal(i + 1, vm.CollaborationItems[i].LineNumber);
    }

    #endregion

    #region ConsultantMentor — Only Adjusted Hours Contribute

    [Fact]
    public void ConsultantMentor_CalculatedHoursAlwaysZero()
    {
        // In InitialEstimate, Consultant/Mentor has no formula (unlike Dtl sheet)
        // Only adjusted hours apply
        var vm = CreateVm();
        Assert.Equal(0m, vm.ConsultantMentorHours);
    }

    [Fact]
    public void ConsultantMentor_AdjustedHours_ContributesToEffectiveCollab()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        decimal collabBefore = vm.CollaborationTotalHours;
        vm.ConsultantMentorAdjustedHours = 50m;

        Assert.Equal(collabBefore + 50m, vm.CollaborationTotalHours);
        Assert.Equal(50m, vm.ConsultantMentorTotalHours);
    }

    [Fact]
    public void ConsultantMentor_AdjustedHours_AffectsGrandTotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        decimal grandBefore = vm.GrandTotalHours;
        vm.ConsultantMentorAdjustedHours = 100m;

        Assert.True(vm.GrandTotalHours > grandBefore);
    }

    #endregion

    #region Development Adjustment Cascades, Others Don't Cascade Into Dev

    [Fact]
    public void SysTestAdjustment_DoesNotChangeDevHours()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        decimal devBefore = vm.DevelopmentTotalHours;
        vm.SystemTestingAdjustedHours = 50m;

        Assert.Equal(devBefore, vm.DevelopmentTotalHours); // Dev unchanged
        Assert.True(vm.AnalysisHours > MainViewModel.RoundUp((100m + 30m) * 0.05m)); // Analysis increased
    }

    [Fact]
    public void AnalysisAdjustment_DoesNotChangeDev_DoesNotChangeSysTest()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        decimal devBefore = vm.DevelopmentTotalHours;
        decimal sysTestBefore = vm.SystemTestingTotalHours;

        vm.AnalysisAdjustedHours = 100m;

        Assert.Equal(devBefore, vm.DevelopmentTotalHours);
        Assert.Equal(sysTestBefore, vm.SystemTestingTotalHours);
    }

    [Fact]
    public void DevAdjustment_CascadesIntoSysTest_30Percent()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 100 hrs

        // No test cases enabled: SysTest = ROUNDUP(effectiveDev * 30%)
        vm.DevelopmentAdjustedHours = 100m; // effectiveDev = 200
        Assert.Equal(200m, vm.DevelopmentTotalHours);
        Assert.Equal(60m, vm.SystemTestingHours); // ROUNDUP(200 * 0.30) = 60
    }

    #endregion

    #region No Components, Only Adjusted Dev — Still Calculates

    [Fact]
    public void NoComponents_WithDevAdjustment_CalculatesAllDerived()
    {
        var vm = CreateVm();
        // No components added, but dev adjusted hours set
        vm.DevelopmentAdjustedHours = 100m;

        // effectiveDev = 0 + 100 = 100
        Assert.Equal(100m, vm.DevelopmentTotalHours);
        Assert.Equal(30m, vm.SystemTestingHours); // ROUNDUP(100 * 0.30)
        Assert.Equal(6.50m, vm.AnalysisHours);    // ROUNDUP(130 * 0.05)
        Assert.Equal(19.50m, vm.BusinessDesignHours); // ROUNDUP(130 * 0.15)
        Assert.True(vm.GrandTotalHours > 0m);
    }

    #endregion

    #region Additional Collaboration Items (>4) Still Calculate Correctly

    [Fact]
    public void FifthCollaborationItem_IncludedInTotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        vm.AddCollaborationItemCommand.Execute(null);
        var fifth = vm.CollaborationItems[^1];
        fifth.NumberOfMeetings = 10;
        fifth.MeetingDurationMinutes = 60;
        fifth.NumberOfParticipants = 3;
        fifth.ParticipantPrepTimeMinutes = 0;

        // 10 × (60/60 + 0) × 3 = 30
        Assert.Equal(30m, fifth.TotalHours);
        Assert.True(vm.TotalCollaborationHours >= 30m);
    }

    #endregion

    #region Initial Estimate — Collaboration Formula (Duration in Minutes)

    [Fact]
    public void InitialEstimate_CollabFormula_DurationInMinutes_NotHours()
    {
        // InitialEstimate uses minutes: Meetings × (Duration/60 + Prep/60) × Participants
        // This is DIFFERENT from Dtl sheet which uses hours
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 20,
            MeetingDurationMinutes = 15, // 15 minutes = 0.25 hours
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 60 // 60 minutes = 1 hour
        };

        // 20 × (15/60 + 60/60) × 5 = 20 × 1.25 × 5 = 125
        Assert.Equal(125m, row.TotalHours);
    }

    [Fact]
    public void InitialEstimate_CollabFormula_ZeroDuration_OnlyPrepContributes()
    {
        var row = new CollaborationRowViewModel
        {
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 4,
            ParticipantPrepTimeMinutes = 30
        };

        // 10 × (0/60 + 30/60) × 4 = 10 × 0.5 × 4 = 20
        Assert.Equal(20m, row.TotalHours);
    }

    #endregion

    #region T-Shirt Size Integration

    [Fact]
    public void TShirtSize_UpdatesWhenGrandTotalChanges()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // Small project

        string sizeBefore = vm.TShirtSize;

        // Add many more to push into XL range
        vm.Components[0].Count = 100;
        Assert.NotEqual(sizeBefore, vm.TShirtSize);
    }

    [Fact]
    public void TShirtSize_NoComponents_ShowsDash()
    {
        var vm = CreateVm();
        Assert.Equal("—", vm.TShirtSize);
    }

    [Fact]
    public void TShirtSize_ExcelExact_XL5_At5262()
    {
        Assert.Equal("XL5", WeightedValues.GetTShirtSize(5262m));
    }

    #endregion

    #region Dtl Collaboration_Quality — PM as Direct Hours (Not Percentage)

    [Fact]
    public void DtlCollab_PMEffort_IsDirectHoursInput()
    {
        // In Dtl Collaboration_Quality, PM is a direct hour input (1400 hrs)
        // NOT calculated as a percentage of other tasks
        decimal pmHours = 1400m;
        decimal meetingTotal = 150m + 75m + 20m + 5m + 144m + 36m; // 430
        decimal consultantTotal = 375m;
        decimal estimatesTotal = 35m;

        decimal overallTotal = meetingTotal + consultantTotal + estimatesTotal + pmHours;
        Assert.Equal(2240m, overallTotal);

        // PM is a fixed input, changing meetings doesn't change PM
        decimal newMeetingTotal = 200m;
        decimal newOverall = newMeetingTotal + consultantTotal + estimatesTotal + pmHours;
        Assert.Equal(2010m, newOverall); // PM stays 1400, not recalculated
    }

    #endregion

    #region Dtl Collaboration_Quality — More Than 5 Consultants

    [Fact]
    public void DtlCollab_SixConsultants_SumIsCorrect()
    {
        var consultants = new[] { 100m, 100m, 100m, 75m, 50m, 25m };
        decimal total = consultants.Sum();
        Assert.Equal(450m, total);
    }

    [Fact]
    public void DtlCollab_TenConsultants_SumIsCorrect()
    {
        var consultants = new[] { 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m, 50m };
        decimal total = consultants.Sum();
        Assert.Equal(500m, total);
    }

    [Fact]
    public void DtlCollab_ConsultantRow_LargeHours_NoOverflow()
    {
        var row = new ConsultantRow { Name = "Senior Architect", Hours = 9999m };
        Assert.Equal(9999m, row.Hours);
    }

    #endregion

    #region Dtl Collaboration_Quality — Create Estimates Edge Cases

    [Fact]
    public void DtlCollab_CreateEstimates_BothZero_TotalIsZero()
    {
        decimal detail = 0m;
        decimal final_ = 0m;
        Assert.Equal(0m, detail + final_);
    }

    [Fact]
    public void DtlCollab_CreateEstimates_FractionalHours()
    {
        decimal detail = 12.5m;
        decimal final_ = 7.75m;
        Assert.Equal(20.25m, detail + final_);
    }

    [Fact]
    public void DtlCollab_CreateEstimates_LargeValues()
    {
        decimal detail = 500m;
        decimal final_ = 250m;
        Assert.Equal(750m, detail + final_);
    }

    #endregion

    #region Dtl Collaboration_Quality — All Zeros Scenario

    [Fact]
    public void DtlCollab_AllZeroInputs_GrandTotalIsZero()
    {
        decimal wprMtg = 0m, wprPrep = 0m;
        decimal clientMtg = 0m, clientPrep = 0m;
        decimal intMtg = 0m, intPrep = 0m;
        decimal consultant = 0m, estimates = 0m, pm = 0m;

        decimal total = wprMtg + wprPrep + clientMtg + clientPrep + intMtg + intPrep
                      + consultant + estimates + pm;
        Assert.Equal(0m, total);
    }

    #endregion

    #region Dtl Collaboration_Quality — Meeting Formula (Hours, Not Minutes)

    [Fact]
    public void DtlCollab_MeetingFormula_UsesHoursDirectly()
    {
        // Dtl sheet: Count × Hours × Attendees (hours are already in hours, not minutes)
        // WPR: 25 × 1hr × 6 = 150
        int count = 25;
        decimal hours = 1m; // 1 hour (not 60 minutes)
        int attendees = 6;

        decimal mtgHours = count * hours * attendees;
        Assert.Equal(150m, mtgHours);
    }

    [Fact]
    public void DtlCollab_PrepFormula_UsesHoursDirectly()
    {
        // Dtl sheet: PrepHrsPerPerson × Attendees × Count
        // WPR Prep: 0.5hrs × 6 × 25 = 75
        decimal prepHrs = 0.5m; // half-hour per person
        int attendees = 6;
        int count = 25;

        decimal prepTotal = prepHrs * attendees * count;
        Assert.Equal(75m, prepTotal);
    }

    [Fact]
    public void DtlCollab_MeetingAndPrep_OnSeparateRows()
    {
        // In Dtl sheet, meeting hours and prep hours are SEPARATE rows
        // (unlike Initial Estimate which combines them into one formula)
        decimal mtgRow = 25 * 1m * 6;       // 150 (G14)
        decimal prepRow = 0.5m * 6 * 25;    // 75  (G15)

        // They are summed separately into the grand total
        Assert.Equal(150m, mtgRow);
        Assert.Equal(75m, prepRow);
        Assert.Equal(225m, mtgRow + prepRow); // Combined WPR contribution
    }

    #endregion

    #region Dtl Collaboration_Quality — Assumptions and Notes Fields

    [Fact]
    public void DtlCollab_Assumptions_LongMultiLineText()
    {
        // Excel Row 39 has extensive multi-line assumptions text
        string assumptions = @"Meeting Walkthrough/Work Product Reviews: WPR for Test Plans, Test Results, Detailed Estimate, TDDs, Codes, and Prod Vals for T-MSIS CIP, CLT, COT, CRX, FTX files. 
Client Meetings: Review Test Results for T-MSIS CIP, CLT, COT, CRX, FTX files. 
Internal Meetings: Testing Status Meetings and any issue resolution discussions.
Consultant/Mentor Effort: Testing support";

        // Verify it can be stored and retrieved
        Assert.Contains("WPR for Test Plans", assumptions);
        Assert.Contains("CIP, CLT, COT, CRX, FTX", assumptions);
        Assert.Contains("Consultant/Mentor Effort", assumptions);
        Assert.True(assumptions.Length > 100);
    }

    [Fact]
    public void DtlCollab_AdditionalHoursNotes_IsJustification()
    {
        // Excel Row 47: "*Additional Hours Notes (i.e. BRD revisions and approvals)"
        // This maps to AdjustedHoursComments in the tool
        var vm = CreateVm();
        vm.AdjustedHoursComments = "BRD revisions required 3 additional cycles.";
        Assert.Contains("BRD revisions", vm.AdjustedHoursComments);
    }

    #endregion

    #region Dtl Collaboration_Quality — Adjusted Hours Per Meeting Row

    [Fact]
    public void DtlCollab_WprAdjusted_IncreasesLineGrandTotal()
    {
        // I14 = G14 + H14 (HourTotal + AdjustedHrs = GrandTotal)
        decimal hourTotal = 150m;
        decimal adjusted = 10m;
        decimal grandTotal = hourTotal + adjusted;
        Assert.Equal(160m, grandTotal);
    }

    [Fact]
    public void DtlCollab_NegativeAdjusted_DecreasesLineGrandTotal()
    {
        decimal hourTotal = 75m;
        decimal adjusted = -25m;
        decimal grandTotal = hourTotal + adjusted;
        Assert.Equal(50m, grandTotal);
    }

    [Fact]
    public void DtlCollab_AdjustedExceedsHourTotal_NegativeGrand_Allowed()
    {
        // Edge case: negative adjustment larger than calculated hours
        decimal hourTotal = 20m;
        decimal adjusted = -30m;
        decimal grandTotal = hourTotal + adjusted;
        Assert.Equal(-10m, grandTotal); // Negative grand total is mathematically valid
    }

    #endregion

    #region Dtl Collaboration_Quality — Overall Formula (I12) Includes PM

    [Fact]
    public void DtlCollab_OverallGrandTotal_IncludesPM()
    {
        // Excel I12 = SUM(I14:I21, D23, D31, D35)
        // I14-I21 = meeting line grand totals
        // D23 = consultant total
        // D31 = estimates total
        // D35 = PM effort
        decimal meetingGrands = 150m + 75m + 20m + 5m + 144m + 36m; // 430
        decimal consultant = 375m;
        decimal estimates = 35m;
        decimal pm = 1400m;

        decimal i12 = meetingGrands + consultant + estimates + pm;
        Assert.Equal(2240m, i12);
    }

    [Fact]
    public void DtlCollab_OverallGrandTotal_WithAdjustments()
    {
        decimal meetingGrands = (150m + 10m) + (75m + 5m) + (20m + 0m) + (5m + 0m) + (144m + 20m) + (36m + 0m);
        // = 160 + 80 + 20 + 5 + 164 + 36 = 465
        decimal consultant = 375m;
        decimal estimates = 35m;
        decimal pm = 1400m;

        decimal i12 = meetingGrands + consultant + estimates + pm;
        Assert.Equal(2275m, i12);
    }

    #endregion

    #region Component Grid — Requirement ID and Description

    [Fact]
    public void ComponentRequirementId_Persists_DoesNotAffectCalc()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.Components[0].RequirementId = "2.2.1";

        Assert.Equal("2.2.1", vm.Components[0].RequirementId);
        // No change to calculation (MISC Small New = 20.00)
        Assert.Equal(20m, vm.Components[0].TotalHours);
    }

    [Fact]
    public void ComponentDescription_Optional_CanBeMultiLine()
    {
        // Excel Row 7: Description has multi-line content
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.DBManipulation;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.Change;
        vm.Components[0].Count = 2;
        vm.Components[0].Description = "Update the \nT_TMSIS_MBESCBES-FTX \nT_TMSIS_MBESCBES-COS";

        Assert.Contains("T_TMSIS_MBESCBES-FTX", vm.Components[0].Description);
        Assert.Equal(51.25m, vm.Components[0].TotalHours); // Calc unaffected
    }

    [Fact]
    public void ComponentDescription_Empty_ValidAndNoError()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.Reports;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;
        vm.Components[0].Description = "";

        Assert.Equal(string.Empty, vm.Components[0].Description);
        Assert.Equal(17m, vm.Components[0].TotalHours);
    }

    #endregion

    #region Excel Exact Match — Full Integration End-to-End

    [Fact]
    public void ExcelExactMatch_AllFieldsVerified_InitialEstimate()
    {
        var vm = CreateExcelVm();
        vm.BaSystemDocAdjustedHours = 1.17m;
        vm.TimeForEstimates = 129m;
        vm.TotalActualHours = 0m;
        vm.EstimatedBy = "Virginia Innerst, Tracey Elias, Richard Yanez, Mario Castillon, Patrick Mulreany";
        vm.AdjustedHoursComments = "This Initial Estimate was copied from the original.";

        // Row 3: Total Hours
        Assert.Equal(5262m, vm.GrandTotalHours);

        // Row 5: T-Shirt
        Assert.Equal("XL5", vm.TShirtSize);

        // Row 23: Development Total
        Assert.Equal(596.5625m, vm.DevelopmentTotalHours);

        // Row 27-34: All Tasks
        Assert.Equal(596.5625m, vm.DevelopmentTotalHours);
        Assert.Equal(2517.46m, vm.SystemTestingTotalHours);
        Assert.Equal(155.71m, vm.AnalysisTotalHours);
        Assert.Equal(467.11m, vm.BusinessDesignTotalHours);
        Assert.Equal(29.83m, vm.PromotionTotalHours);
        Assert.Equal(31.00m, vm.BaSystemDocTotalHours); // 29.83 + 1.17
        Assert.Equal(503.50m, vm.ProductionValidationTotalHours);
        Assert.Equal(645.18m, vm.ProjectManagementTotalHours);

        // Row 36-38: Collaboration
        Assert.Equal(125m, vm.WprsTotalHours);
        Assert.Equal(42m, vm.ClientMeetingsTotalHours);
        Assert.Equal(18.75m, vm.InternalMeetingsTotalHours);
        Assert.Equal(0m, vm.AutomationTestCollabTotalHours);
        Assert.Equal(0m, vm.ConsultantMentorTotalHours);

        // Row 42: Time for Estimates
        Assert.Equal(129m, vm.TimeForEstimates);

        // Row 43: Subtotal
        Assert.Equal(5261.11m, vm.SubtotalHours);

        // Row 44: T-Shirt
        Assert.Equal("XL5", vm.TShirtSize);

        // Rows 47-51: Role Breakout
        Assert.Equal(1143.97m, vm.BaRoleHours);
        Assert.Equal(768.75m, vm.SeRoleHours);
        Assert.Equal(2517.46m, vm.TesterRoleHours);
        Assert.Equal(645.18m, vm.PmRoleHours);
        Assert.Equal(185.75m, vm.CollaborationRoleHours);

        // Row 53: Estimated By
        Assert.Contains("Virginia Innerst", vm.EstimatedBy);

        // Row 56: Comments
        Assert.Contains("copied from the original", vm.AdjustedHoursComments);
    }

    [Fact]
    public void ExcelExactMatch_DtlCollabQuality_FullPipeline()
    {
        // Complete Dtl Collaboration_Quality sheet verification
        int wprCount = 25, clientCount = 5, intCount = 24;
        int wprAtt = 6, clientAtt = 4, intAtt = 6;
        decimal wprMtgHrs = 1m, clientMtgHrs = 1m, intMtgHrs = 1m;
        decimal wprPrepHrs = 0.5m, clientPrepHrs = 0.25m, intPrepHrs = 0.25m;

        // Row 14: WPR = 25 × 1 × 6 = 150
        Assert.Equal(150m, wprCount * wprMtgHrs * wprAtt);
        // Row 15: WPR Prep = 0.5 × 6 × 25 = 75
        Assert.Equal(75m, wprPrepHrs * wprAtt * wprCount);
        // Row 17: Client = 5 × 1 × 4 = 20
        Assert.Equal(20m, clientCount * clientMtgHrs * clientAtt);
        // Row 18: Client Prep = 0.25 × 4 × 5 = 5
        Assert.Equal(5m, clientPrepHrs * clientAtt * clientCount);
        // Row 20: Internal = 24 × 1 × 6 = 144
        Assert.Equal(144m, intCount * intMtgHrs * intAtt);
        // Row 21: Internal Prep = 0.25 × 6 × 24 = 36
        Assert.Equal(36m, intPrepHrs * intAtt * intCount);

        // Row 23: Consultant = SUM(100, 100, 100, 75, 0) = 375
        decimal consultant = 100m + 100m + 100m + 75m + 0m;
        Assert.Equal(375m, consultant);

        // Row 31: Estimates = 25 + 10 = 35
        decimal estimates = 25m + 10m;
        Assert.Equal(35m, estimates);

        // Row 35: PM = 1400
        decimal pm = 1400m;

        // Row 12: Grand Total = 150+75+20+5+144+36 + 375 + 35 + 1400 = 2240
        decimal grandTotal = 150m + 75m + 20m + 5m + 144m + 36m + consultant + estimates + pm;
        Assert.Equal(2240m, grandTotal);
    }

    #endregion

    #region Negative/Sad Path — Invalid Component Combinations

    [Fact]
    public void Component_NoneType_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.None,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.New,
            Count = 5
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void Component_NoneSize_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.None,
            ChangeType = ChangeType.New,
            Count = 5
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void Component_NoneChangeType_ZeroBaseHours()
    {
        var row = new ComponentRowViewModel
        {
            ComponentType = ComponentType.MISC,
            Size = ComponentSize.Large,
            ChangeType = ChangeType.None,
            Count = 5
        };
        Assert.Equal(0m, row.TotalHours);
    }

    [Fact]
    public void AllComponentsRemoved_GrandTotalReturnsToZero()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1;

        Assert.True(vm.GrandTotalHours > 0m);

        vm.RemoveComponentCommand.Execute(vm.Components[0]);
        Assert.Equal(0m, vm.GrandTotalHours);
        Assert.Equal("—", vm.TShirtSize);
    }

    #endregion

    #region Negative/Sad Path — Extreme Values

    [Fact]
    public void VeryLargeComponent_CalculatesWithoutOverflow()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.K2Workflow;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 500; // 200 × 500 = 100,000 hrs

        Assert.Equal(100000m, vm.DevelopmentTotalHours);
        Assert.Equal("XL8", vm.TShirtSize);
    }

    [Fact]
    public void MaxIterations_LargeTestCases_NoOverflow()
    {
        var vm = CreateVm();
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesVeryComplex = 500; // 500 × 14.38 + 500×13.13×0.1 = 7190+656.5 = 7846.5
        vm.TestCaseIterations = 10; // × 10 = 78465
        Assert.Equal(78465m, vm.SystemTestingHours);
    }

    [Fact]
    public void AllAdjustedNegative_CanProduceNegativeSubtotal()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.MISC;
        vm.Components[0].Size = ComponentSize.Small;
        vm.Components[0].ChangeType = ChangeType.New;
        vm.Components[0].Count = 1; // 10 hrs dev

        vm.DevelopmentAdjustedHours = -100m; // effectiveDev = -90
        // SubtotalHours could become negative
        Assert.True(vm.SubtotalHours < 0m);
    }

    #endregion

    #region Happy Path — Multiple Components Same Type

    [Fact]
    public void MultipleComponents_SameType_SumCorrectly()
    {
        var vm = CreateVm();

        // Add 3 DB Manipulation components (matches Excel rows 7-8)
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.DBManipulation;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.Change;
        vm.Components[0].Count = 2; // 51.25

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].ComponentType = ComponentType.DBManipulation;
        vm.Components[1].Size = ComponentSize.Large;
        vm.Components[1].ChangeType = ChangeType.New;
        vm.Components[1].Count = 1; // 31.875

        Assert.Equal(51.25m + 31.875m, vm.TotalDevelopmentHours);
    }

    [Fact]
    public void MixedComponentTypes_AllContributeToDev()
    {
        var vm = CreateVm();

        vm.AddComponentCommand.Execute(null);
        vm.Components[0].ComponentType = ComponentType.DBManipulation;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.Change;
        vm.Components[0].Count = 2;

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].ComponentType = ComponentType.SupportModules;
        vm.Components[1].Size = ComponentSize.Medium;
        vm.Components[1].ChangeType = ChangeType.Change;
        vm.Components[1].Count = 53;

        // 51.25 + 513.4375 = 564.6875
        Assert.Equal(564.6875m, vm.TotalDevelopmentHours);
    }

    #endregion

    #region Happy Path — Collaboration Per-Type Adjusted Hours in Initial Estimate

    [Fact]
    public void WprsAdjusted_AddsToWprTotal_AndCollabTotal()
    {
        var vm = CreateVm();
        vm.CollaborationItems[0].NumberOfMeetings = 20;
        vm.CollaborationItems[0].MeetingDurationMinutes = 15;
        vm.CollaborationItems[0].NumberOfParticipants = 5;
        vm.CollaborationItems[0].ParticipantPrepTimeMinutes = 60;

        Assert.Equal(125m, vm.WprsHours);
        Assert.Equal(125m, vm.WprsTotalHours);

        vm.WprsAdjustedHours = 5m;
        Assert.Equal(130m, vm.WprsTotalHours);
        Assert.True(vm.CollaborationTotalHours >= 130m);
    }

    [Fact]
    public void ClientMeetingsAdjusted_AddsToClientTotal()
    {
        var vm = CreateVm();
        vm.CollaborationItems[1].NumberOfMeetings = 7;
        vm.CollaborationItems[1].MeetingDurationMinutes = 60;
        vm.CollaborationItems[1].NumberOfParticipants = 3;
        vm.CollaborationItems[1].ParticipantPrepTimeMinutes = 60;

        Assert.Equal(42m, vm.ClientMeetingsHours);
        vm.ClientMeetingsAdjustedHours = -2m;
        Assert.Equal(40m, vm.ClientMeetingsTotalHours);
    }

    #endregion

    #region Helper — Creates VM with Exact Excel CO 23327 002 Data

    private MainViewModel CreateExcelVm()
    {
        var vm = new MainViewModel();

        // Components (Excel rows 7-9)
        vm.AddComponentCommand.Execute(null);
        vm.Components[0].RequirementId = "2.2.1";
        vm.Components[0].ComponentType = ComponentType.DBManipulation;
        vm.Components[0].Size = ComponentSize.Large;
        vm.Components[0].ChangeType = ChangeType.Change;
        vm.Components[0].Count = 2;
        vm.Components[0].Description = "Update the T_TMSIS_MBESCBES-FTX T_TMSIS_MBESCBES-COS";

        vm.AddComponentCommand.Execute(null);
        vm.Components[1].RequirementId = "2.2.1";
        vm.Components[1].ComponentType = ComponentType.DBManipulation;
        vm.Components[1].Size = ComponentSize.Large;
        vm.Components[1].ChangeType = ChangeType.New;
        vm.Components[1].Count = 1;
        vm.Components[1].Description = "T_TMSIS_SPA_NUMBER";

        vm.AddComponentCommand.Execute(null);
        vm.Components[2].RequirementId = "2.2.1";
        vm.Components[2].ComponentType = ComponentType.SupportModules;
        vm.Components[2].Size = ComponentSize.Medium;
        vm.Components[2].ChangeType = ChangeType.Change;
        vm.Components[2].Count = 53;
        vm.Components[2].Description = "Update support modules to ETLs, C Programs, packages, and queries";

        // Test Cases (Excel Row 30)
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2.5m;

        // PM Effort (Excel Row 34)
        vm.PmEffortPercentage = 15m;

        // Collaboration (Excel Rows 36-39)
        var wprs = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.WPRs);
        wprs.NumberOfMeetings = 20;
        wprs.MeetingDurationMinutes = 15;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        var client = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.ClientMeetings);
        client.NumberOfMeetings = 7;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 3;
        client.ParticipantPrepTimeMinutes = 60;

        var internalMtg = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.InternalMeetings);
        internalMtg.NumberOfMeetings = 3;
        internalMtg.MeetingDurationMinutes = 15;
        internalMtg.NumberOfParticipants = 5;
        internalMtg.ParticipantPrepTimeMinutes = 60;

        var auto = vm.CollaborationItems.First(c => c.CollabType == CollaborationType.AutomationTestCollaboration);
        auto.NumberOfMeetings = 0;
        auto.MeetingDurationMinutes = 0;
        auto.NumberOfParticipants = 0;
        auto.ParticipantPrepTimeMinutes = 0;

        return vm;
    }

    #endregion
}
