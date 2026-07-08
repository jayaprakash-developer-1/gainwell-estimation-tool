using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Verification tests for "CO 23869 002 Final Estimate V1.0.xlsm".
/// Covers scenarios with adjusted hours and collaboration combined.
/// </summary>
public class ExcelVerification_CO23869_002_Tests
{
    private MainViewModel CreateVm() => new();

    private ComponentRowViewModel AddComponent(MainViewModel vm, ComponentType type, ComponentSize size, ChangeType change, int count)
    {
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = type;
        row.Size = size;
        row.ChangeType = change;
        row.Count = count;
        return row;
    }

    private void ClearCollaboration(MainViewModel vm)
    {
        foreach (var item in vm.CollaborationItems.ToList())
            vm.RemoveCollaborationItemCommand.Execute(item);
    }

    #region CO 23869 002 — Component Verification

    [Fact]
    public void CO23869_002_SupportModules_Large_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.SupportModules, ComponentSize.Large, ChangeType.New);
        Assert.Equal(26.875m, result);
    }

    [Fact]
    public void CO23869_002_SupportModules_Large_Change_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.SupportModules, ComponentSize.Large, ChangeType.Change);
        Assert.Equal(21.5625m, result);
    }

    [Fact]
    public void CO23869_002_DBManipulation_Medium_New_BaseHours()
    {
        var result = WeightedValues.GetBaseHours(ComponentType.DBManipulation, ComponentSize.Medium, ChangeType.New);
        Assert.Equal(15.00m, result);
    }

    #endregion

    #region CO 23869 002 — With Adjustments

    [Fact]
    public void CO23869_002_DevAdjustment_CascadesCorrectly()
    {
        // Scenario: 5× SupportModules Large New (26.875 each) = 134.375
        // + DevAdjusted = 10
        // Effective Dev = 144.375
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 5);
        Assert.Equal(134.375m, vm.TotalDevelopmentHours);

        vm.DevelopmentAdjustedHours = 10m;

        // System Testing = ROUNDUP(144.375 * 0.30, 2) = ROUNDUP(43.3125, 2) = 43.32
        Assert.Equal(43.32m, vm.SystemTestingHours);

        // Analysis = ROUNDUP((144.375 + 43.32) * 0.05, 2) = ROUNDUP(187.695 * 0.05, 2) = ROUNDUP(9.38475, 2) = 9.39
        Assert.Equal(9.39m, vm.AnalysisHours);
    }

    [Fact]
    public void CO23869_002_BaDocAdjustment_OnlyAddsToBaDoc()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 5);
        decimal baDocBefore = vm.BaSystemDocHours;

        vm.BaSystemDocAdjustedHours = 5.5m;

        // BA System Doc total = calculated + adjusted
        Assert.Equal(baDocBefore + 5.5m, vm.BaSystemDocTotalHours);
        // The calculated value itself doesn't change
        Assert.Equal(baDocBefore, vm.BaSystemDocHours);
    }

    [Fact]
    public void CO23869_002_SystemTestingAdjustment_AffectsProdVal()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 5);
        decimal sysTestCalc = vm.SystemTestingHours;

        vm.SystemTestingAdjustedHours = 20m;

        // Effective SysTest = calc + 20
        // ProdVal = ROUNDUP((calc + 20) * 0.20, 2)
        decimal expectedProdVal = MainViewModel.RoundUp((sysTestCalc + 20m) * 0.20m);
        Assert.Equal(expectedProdVal, vm.ProductionValidationHours);
    }

    [Fact]
    public void CO23869_002_WithCollaborationAndAdjustments()
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 5);
        vm.DevelopmentAdjustedHours = 10m;
        vm.BaSystemDocAdjustedHours = 1.17m;

        // Add collaboration
        vm.AddCollaborationItemCommand.Execute(null);
        var collab = vm.CollaborationItems[^1];
        collab.CollabType = CollaborationType.WPRs;
        collab.NumberOfMeetings = 10;
        collab.MeetingDurationMinutes = 30;
        collab.NumberOfParticipants = 3;
        collab.ParticipantPrepTimeMinutes = 15;

        // WPRs hours = 10 × (30/60 + 15/60) × 3 = 10 × 0.75 × 3 = 22.5
        Assert.Equal(22.50m, collab.TotalHours);
        Assert.True(vm.GrandTotalHours > 0);
        Assert.True(vm.SubtotalHours > 0);
    }

    #endregion

    #region CO 23869 002 — PM Percentage Variation

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    [InlineData(15)]
    [InlineData(18)]
    [InlineData(20)]
    public void CO23869_002_PMEffort_VariousPercentages(int pmPercent)
    {
        var vm = CreateVm();
        ClearCollaboration(vm);

        AddComponent(vm, ComponentType.SupportModules, ComponentSize.Large, ChangeType.New, 5);
        vm.PmEffortPercentage = pmPercent;

        decimal effectiveDev = vm.TotalDevelopmentHours;
        decimal effectiveSysTest = vm.SystemTestingHours;
        decimal effectiveAnalysis = vm.AnalysisHours;
        decimal effectiveBizDesign = vm.BusinessDesignHours;
        decimal effectivePromotion = vm.PromotionHours;
        decimal effectiveBaDoc = vm.BaSystemDocHours;
        decimal effectiveProdVal = vm.ProductionValidationHours;

        decimal allTasks = effectiveDev + effectiveSysTest + effectiveAnalysis + effectiveBizDesign
                         + effectivePromotion + effectiveBaDoc + effectiveProdVal;
        decimal expectedPM = MainViewModel.RoundUp(allTasks * (pmPercent / 100m));

        Assert.Equal(expectedPM, vm.ProjectManagementHours);
    }

    #endregion
}
