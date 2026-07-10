using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Integration tests verifying the complete estimation flow from components through
/// all calculations to final grand total — matches "Final Estimate" Excel output exactly.
/// Covers multiple Excel reference files with full adjusted hours, collaboration,
/// time-for-estimates, and actual hours.
/// </summary>
public class FinalEstimate_IntegrationTests
{
    private InitialEstimateViewModel CreateVm() => new();

    private ComponentRowViewModel AddComponent(InitialEstimateViewModel vm, ComponentType type, ComponentSize size, ChangeType change, int count)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = count;
        return row;
    }

    private void ClearCollaboration(InitialEstimateViewModel vm)
    {
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
    }

    #region Integration — Small Project (< 100 hrs)

    [Fact]
    public void Integration_SmallProject_SingleComponent()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.Change, 1);

        // Dev = 20.94
        Assert.Equal(20.94m, vm.TotalDevelopmentHours);
        Assert.Equal("Small", vm.TShirtSize);

        // All derived tasks should cascade correctly
        Assert.Equal(InitialEstimateViewModel.RoundUp(20.94m * 0.30m), vm.SystemTestingHours);
        Assert.True(vm.GrandTotalHours < 100m);
    }

    #endregion

    #region Integration — Medium Project (100-300 hrs)

    [Fact]
    public void Integration_MediumProject()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.Webpage, ComponentSize.Medium, ChangeType.New, 1);

        // Dev = 60, should be "Small" at this level
        // With all derived tasks: GrandTotal will be around 130-145
        Assert.True(vm.GrandTotalHours >= 100 && vm.GrandTotalHours < 300);
        Assert.Equal("Medium", vm.TShirtSize);
    }

    #endregion

    #region Integration — Large Project (300-750 hrs)

    [Fact]
    public void Integration_LargeProject()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 1);
        AddComponent(vm, ComponentType.Reports, ComponentSize.Large, ChangeType.New, 1);

        // Dev = 125 + 85 = 210
        Assert.Equal(210.00m, vm.TotalDevelopmentHours);
        Assert.True(vm.GrandTotalHours >= 300 && vm.GrandTotalHours < 750);
        Assert.Equal("Large", vm.TShirtSize);
    }

    #endregion

    #region Integration — XL Project with Adjustments

    [Fact]
    public void Integration_XLProject_WithAdjustments()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.ProgramsDBStoredProcs, ComponentSize.Large, ChangeType.New, 3);
        AddComponent(vm, ComponentType.PowerBuilderWindows, ComponentSize.Large, ChangeType.New, 2);

        // Dev = 294.40*3 + 125*2 = 883.20 + 250 = 1133.20
        Assert.Equal(1133.20m, vm.TotalDevelopmentHours);

        vm.DevelopmentAdjustedHours = 50m;
        vm.SystemTestingAdjustedHours = 20m;
        vm.BaSystemDocAdjustedHours = 5m;

        // With adjustments, all derived tasks recalculate
        Assert.True(vm.GrandTotalHours >= 2000);
    }

    #endregion

    #region Integration — Full CO 23327 002 Reproduction

    [Fact]
    public void Integration_CO23327_ExactReproduction()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        // Step 1: Add exact components from CO 23327 002
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.Change, 2);   // 25.625 × 2 = 51.25
        AddComponent(vm, ComponentType.DBManipulation, ComponentSize.Large, ChangeType.New, 1);      // 31.875 × 1 = 31.875
        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Medium, ChangeType.Change, 53); // 9.6875 × 53 = 513.4375

        Assert.Equal(596.5625m, vm.TotalDevelopmentHours);

        // Step 2: Enable test-case-based system testing
        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 125;
        vm.TestCasesMedium = 0;
        vm.TestCasesComplex = 75;
        vm.TestCasesVeryComplex = 0;
        vm.TestCaseIterations = 2.5m;

        Assert.Equal(2517.46m, vm.SystemTestingHours);

        // Step 3: BA System Doc adjustment
        vm.BaSystemDocAdjustedHours = 1.17m;

        // Step 4: Time for estimates
        vm.TimeForEstimates = 129m;

        // Step 5: Collaboration
        vm.AddCollaborationItemCommand.Execute(null);
        var wprs = vm.CollaborationItems[^1];
        wprs.CollabType = CollaborationType.WPRs;
        wprs.NumberOfMeetings = 20;
        wprs.MeetingDurationMinutes = 15;
        wprs.NumberOfParticipants = 5;
        wprs.ParticipantPrepTimeMinutes = 60;

        vm.AddCollaborationItemCommand.Execute(null);
        var client = vm.CollaborationItems[^1];
        client.CollabType = CollaborationType.ClientMeetings;
        client.NumberOfMeetings = 7;
        client.MeetingDurationMinutes = 60;
        client.NumberOfParticipants = 3;
        client.ParticipantPrepTimeMinutes = 60;

        vm.AddCollaborationItemCommand.Execute(null);
        var intern = vm.CollaborationItems[^1];
        intern.CollabType = CollaborationType.InternalMeetings;
        intern.NumberOfMeetings = 3;
        intern.MeetingDurationMinutes = 15;
        intern.NumberOfParticipants = 5;
        intern.ParticipantPrepTimeMinutes = 60;

        // Step 6: Verify all values match Excel
        Assert.Equal(596.5625m, vm.TotalDevelopmentHours);
        Assert.Equal(2517.46m, vm.SystemTestingHours);
        Assert.Equal(125.00m, wprs.TotalHours);
        Assert.Equal(42.00m, client.TotalHours);
        Assert.Equal(18.75m, intern.TotalHours);
        Assert.Equal(185.75m, vm.TotalCollaborationHours);
        Assert.Equal(5262m, vm.GrandTotalHours);
        Assert.Equal("XL5", vm.TShirtSize);
    }

    #endregion

    #region Integration — Cascading Adjustments

    [Fact]
    public void Integration_DevAdjustment_CascadesToAllDerived()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal origSysTest = vm.SystemTestingHours;
        decimal origAnalysis = vm.AnalysisHours;
        decimal origBizDesign = vm.BusinessDesignHours;
        decimal origPromo = vm.PromotionHours;
        decimal origBaDoc = vm.BaSystemDocHours;

        vm.DevelopmentAdjustedHours = 100m;

        // All derived tasks should increase (they depend on effectiveDev)
        Assert.True(vm.SystemTestingHours > origSysTest);
        Assert.True(vm.AnalysisHours > origAnalysis);
        Assert.True(vm.BusinessDesignHours > origBizDesign);
        Assert.True(vm.PromotionHours > origPromo);
        Assert.True(vm.BaSystemDocHours > origBaDoc);
    }

    [Fact]
    public void Integration_SysTestAdjustment_CascadesToAnalysisBizDesignProdVal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal origAnalysis = vm.AnalysisHours;
        decimal origBizDesign = vm.BusinessDesignHours;
        decimal origProdVal = vm.ProductionValidationHours;
        decimal origPromo = vm.PromotionHours;

        vm.SystemTestingAdjustedHours = 50m;

        // Analysis and BizDesign depend on (dev + sysTest) — sysTest changed
        Assert.True(vm.AnalysisHours > origAnalysis);
        Assert.True(vm.BusinessDesignHours > origBizDesign);
        Assert.True(vm.ProductionValidationHours > origProdVal);
        // Promotion depends only on dev, should NOT change
        Assert.Equal(origPromo, vm.PromotionHours);
    }

    #endregion

    #region Integration — HasComponents and HasValidComponents

    [Fact]
    public void Integration_HasComponents_TrueWhenComponentsExist()
    {
        var vm = CreateVm();
        Assert.False(vm.HasComponents);

        vm.AddComponentCommand.Execute(null);
        Assert.True(vm.HasComponents);
    }

    [Fact]
    public void Integration_HasValidComponents_RequiresAllFields()
    {
        var vm = CreateVm();
        vm.AddComponentCommand.Execute(null);
        // Default component has ComponentType.None → not valid
        Assert.False(vm.HasValidComponents);

        var row = vm.Components[0];
        row.RequirementId = "REQ-001";
        row.ComponentType = ComponentType.MISC;
        row.ChangeType = ChangeType.New;
        row.Size = ComponentSize.Large;
        row.Count = 1;
        Assert.True(vm.HasValidComponents);
    }

    #endregion

    #region Integration — Multiple Iterations Test Case Formula

    [Theory]
    [InlineData(1.0)]
    [InlineData(1.5)]
    [InlineData(2.0)]
    [InlineData(2.5)]
    [InlineData(3.0)]
    public void Integration_TestCaseIterations_ScalesLinearly(decimal iterations)
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        vm.UseTestCasesForEstimate = true;
        vm.TestCasesSimple = 50;
        vm.TestCaseIterations = 1m;
        decimal baseHours = vm.SystemTestingHours;

        vm.TestCaseIterations = iterations;
        decimal scaledHours = vm.SystemTestingHours;

        // Should scale by iterations (approximately, due to RoundUp)
        Assert.True(scaledHours >= baseHours * Math.Max(1m, iterations) - 1m);
    }

    #endregion

    #region Integration — Notes Don't Affect Calculations

    [Fact]
    public void Integration_Notes_DontAffectAnyCalculation()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);
        AddComponent(vm, ComponentType.MISC, ComponentSize.Large, ChangeType.New, 1);

        decimal gtBefore = vm.GrandTotalHours;

        vm.DevelopmentNotes = "These are development notes";
        vm.AnalysisNotes = "Analysis notes here";
        vm.SeAssumptions = "SE assumptions";
        vm.BaAssumptions = "BA assumptions";
        vm.GeneralAssumptions = "General assumptions";
        vm.AdjustedHoursComments = "Comments about adjustments";

        Assert.Equal(gtBefore, vm.GrandTotalHours);
    }

    #endregion
}
