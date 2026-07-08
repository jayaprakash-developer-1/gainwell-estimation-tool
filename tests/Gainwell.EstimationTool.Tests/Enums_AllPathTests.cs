using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Tests for enum values, display names, and detailed estimate enums.
/// Covers all enum types used throughout the application.
/// </summary>
public class Enums_AllPathTests
{
    #region ComponentType

    [Fact]
    public void ComponentType_HasCorrectCount()
    {
        // None + 11 types = 12
        Assert.Equal(12, Enum.GetValues<ComponentType>().Length);
    }

    [Fact]
    public void ComponentType_NoneIsNegative1()
    {
        Assert.Equal(-1, (int)ComponentType.None);
    }

    [Theory]
    [InlineData(ComponentType.PowerBuilderWindows, 0)]
    [InlineData(ComponentType.Reports, 1)]
    [InlineData(ComponentType.ProgramsDBStoredProcs, 2)]
    [InlineData(ComponentType.SupportModules, 3)]
    [InlineData(ComponentType.DBManipulation, 4)]
    [InlineData(ComponentType.DatabaseReview, 5)]
    [InlineData(ComponentType.Webpage, 6)]
    [InlineData(ComponentType.K2Workflow, 7)]
    [InlineData(ComponentType.K2SmartForm, 8)]
    [InlineData(ComponentType.TestAutomationUFT, 9)]
    [InlineData(ComponentType.MISC, 10)]
    public void ComponentType_CorrectIntValues(ComponentType type, int expected)
    {
        Assert.Equal(expected, (int)type);
    }

    #endregion

    #region ChangeType

    [Fact]
    public void ChangeType_HasCorrectCount()
    {
        Assert.Equal(3, Enum.GetValues<ChangeType>().Length); // None, New, Change
    }

    [Fact]
    public void ChangeType_NoneIsNegative1()
    {
        Assert.Equal(-1, (int)ChangeType.None);
    }

    [Fact]
    public void ChangeType_NewIs0()
    {
        Assert.Equal(0, (int)ChangeType.New);
    }

    [Fact]
    public void ChangeType_ChangeIs1()
    {
        Assert.Equal(1, (int)ChangeType.Change);
    }

    #endregion

    #region ComponentSize

    [Fact]
    public void ComponentSize_HasCorrectCount()
    {
        Assert.Equal(4, Enum.GetValues<ComponentSize>().Length); // None, Small, Medium, Large
    }

    [Fact]
    public void ComponentSize_NoneIsNegative1()
    {
        Assert.Equal(-1, (int)ComponentSize.None);
    }

    [Theory]
    [InlineData(ComponentSize.Small, 0)]
    [InlineData(ComponentSize.Medium, 1)]
    [InlineData(ComponentSize.Large, 2)]
    public void ComponentSize_CorrectIntValues(ComponentSize size, int expected)
    {
        Assert.Equal(expected, (int)size);
    }

    #endregion

    #region CollaborationType

    [Fact]
    public void CollaborationType_HasCorrectCount()
    {
        Assert.Equal(4, Enum.GetValues<CollaborationType>().Length);
    }

    [Theory]
    [InlineData(CollaborationType.WPRs, 0)]
    [InlineData(CollaborationType.ClientMeetings, 1)]
    [InlineData(CollaborationType.InternalMeetings, 2)]
    [InlineData(CollaborationType.AutomationTestCollaboration, 3)]
    public void CollaborationType_CorrectIntValues(CollaborationType type, int expected)
    {
        Assert.Equal(expected, (int)type);
    }

    #endregion

    #region ComponentStatus (Detailed)

    [Fact]
    public void ComponentStatus_HasCorrectCount()
    {
        Assert.Equal(3, Enum.GetValues<ComponentStatus>().Length); // Select, New, Existing
    }

    [Fact]
    public void ComponentStatus_SelectIsNegative1()
    {
        Assert.Equal(-1, (int)ComponentStatus.Select);
    }

    [Fact]
    public void ComponentStatus_NewIs0()
    {
        Assert.Equal(0, (int)ComponentStatus.New);
    }

    [Fact]
    public void ComponentStatus_ExistingIs1()
    {
        Assert.Equal(1, (int)ComponentStatus.Existing);
    }

    #endregion

    #region Complexity

    [Fact]
    public void Complexity_HasCorrectCount()
    {
        Assert.Equal(3, Enum.GetValues<Complexity>().Length); // Simple, Moderate, Complex
    }

    [Theory]
    [InlineData(Complexity.Simple, 0)]
    [InlineData(Complexity.Moderate, 1)]
    [InlineData(Complexity.Complex, 2)]
    public void Complexity_CorrectIntValues(Complexity cx, int expected)
    {
        Assert.Equal(expected, (int)cx);
    }

    #endregion

    #region BaComplexity

    [Fact]
    public void BaComplexity_HasCorrectCount()
    {
        Assert.Equal(4, Enum.GetValues<BaComplexity>().Length); // Simple, Moderate, Complex, VeryComplex
    }

    [Theory]
    [InlineData(BaComplexity.Simple, 0)]
    [InlineData(BaComplexity.Moderate, 1)]
    [InlineData(BaComplexity.Complex, 2)]
    [InlineData(BaComplexity.VeryComplex, 3)]
    public void BaComplexity_CorrectIntValues(BaComplexity cx, int expected)
    {
        Assert.Equal(expected, (int)cx);
    }

    #endregion

    #region ExperienceLevel

    [Fact]
    public void ExperienceLevel_HasCorrectCount()
    {
        Assert.Equal(4, Enum.GetValues<ExperienceLevel>().Length);
    }

    [Theory]
    [InlineData(ExperienceLevel.SelectALevel, 0)]
    [InlineData(ExperienceLevel.NewToArea, 1)]
    [InlineData(ExperienceLevel.Proficient, 2)]
    [InlineData(ExperienceLevel.Expert, 3)]
    public void ExperienceLevel_CorrectIntValues(ExperienceLevel level, int expected)
    {
        Assert.Equal(expected, (int)level);
    }

    #endregion

    #region EstimateRole

    [Fact]
    public void EstimateRole_HasCorrectCount()
    {
        Assert.Equal(2, Enum.GetValues<EstimateRole>().Length); // SE, BA
    }

    #endregion

    #region SeTaskPhase

    [Fact]
    public void SeTaskPhase_StandardPhasesExist()
    {
        var phases = Enum.GetValues<SeTaskPhase>();
        Assert.Contains(SeTaskPhase.Analysis, phases);
        Assert.Contains(SeTaskPhase.GenerateTechnicalDesign, phases);
        Assert.Contains(SeTaskPhase.DesignReviewAndAcceptance, phases);
        Assert.Contains(SeTaskPhase.UnitTestCasesScenarios, phases);
        Assert.Contains(SeTaskPhase.CodeConstructionAndUnitTest, phases);
        Assert.Contains(SeTaskPhase.CodeAndUnitTestReviewPreWPR, phases);
        Assert.Contains(SeTaskPhase.UpdateDocumentation, phases);
        Assert.Contains(SeTaskPhase.ProductionImplementation, phases);
    }

    [Fact]
    public void SeTaskPhase_SqlPhasesExist()
    {
        var phases = Enum.GetValues<SeTaskPhase>();
        Assert.Contains(SeTaskPhase.SqlDesign, phases);
        Assert.Contains(SeTaskPhase.SqlConstruction, phases);
        Assert.Contains(SeTaskPhase.SqlTesting, phases);
        Assert.Contains(SeTaskPhase.SqlReview, phases);
    }

    [Fact]
    public void SeTaskPhase_ModulePhasesExist()
    {
        var phases = Enum.GetValues<SeTaskPhase>();
        Assert.Contains(SeTaskPhase.DesignModule, phases);
        Assert.Contains(SeTaskPhase.BuildModule, phases);
        Assert.Contains(SeTaskPhase.TestModule, phases);
        Assert.Contains(SeTaskPhase.ReviewModule, phases);
    }

    #endregion
}
