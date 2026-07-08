using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for ProjectEntity and ComponentEntryEntity models.
/// Tests all properties, defaults, and data integrity.
/// </summary>
public class ModelEntities_AllPathTests
{
    #region ProjectEntity — Happy Path

    [Fact]
    public void ProjectEntity_HappyPath_DefaultValues()
    {
        var entity = new ProjectEntity();
        Assert.False(string.IsNullOrEmpty(entity.ProjectId));
        Assert.Equal(32, entity.ProjectId.Length); // GUID without hyphens
        Assert.Equal(string.Empty, entity.ProjectName);
        Assert.Equal(15m, entity.PmEffortPercentage);
        Assert.Equal(1, entity.VersionNumber);
        Assert.NotEqual(default(DateTime), entity.CreatedDate);
    }

    [Fact]
    public void ProjectEntity_HappyPath_UniqueIds()
    {
        var e1 = new ProjectEntity();
        var e2 = new ProjectEntity();
        Assert.NotEqual(e1.ProjectId, e2.ProjectId);
    }

    [Fact]
    public void ProjectEntity_HappyPath_AllFieldsSet()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "Test Project",
            ChangeOrderId = "CO-2026-001",
            ProjectDescription = "Description",
            EstimatedBy = "Engineer",
            ReviewedBy = "Manager",
            PmEffortPercentage = 18m,
            TotalDevelopmentHours = 500m,
            GrandTotalHours = 1000m,
            TShirtSize = "XL1",
            CollaborationHours = 50m
        };

        Assert.Equal("Test Project", entity.ProjectName);
        Assert.Equal("CO-2026-001", entity.ChangeOrderId);
        Assert.Equal(18m, entity.PmEffortPercentage);
        Assert.Equal(500m, entity.TotalDevelopmentHours);
        Assert.Equal(1000m, entity.GrandTotalHours);
        Assert.Equal("XL1", entity.TShirtSize);
    }

    #endregion

    #region ProjectEntity — Sad Path

    [Fact]
    public void ProjectEntity_SadPath_EmptyStringsAllowed()
    {
        var entity = new ProjectEntity
        {
            ProjectName = "",
            ChangeOrderId = "",
            ProjectDescription = "",
            EstimatedBy = "",
            ReviewedBy = ""
        };
        Assert.Equal(string.Empty, entity.ProjectName);
    }

    [Fact]
    public void ProjectEntity_SadPath_ZeroHours()
    {
        var entity = new ProjectEntity
        {
            TotalDevelopmentHours = 0m,
            GrandTotalHours = 0m,
            CollaborationHours = 0m
        };
        Assert.Equal(0m, entity.TotalDevelopmentHours);
        Assert.Equal(0m, entity.GrandTotalHours);
    }

    #endregion

    #region ProjectEntity — Adjusted Hours

    [Fact]
    public void ProjectEntity_Positive_AllAdjustmentsStored()
    {
        var entity = new ProjectEntity
        {
            DevelopmentAdjustedHours = 10m,
            AnalysisAdjustedHours = 5m,
            BusinessDesignAdjustedHours = 3m,
            SystemTestingAdjustedHours = 15m,
            PromotionAdjustedHours = 2m,
            BaSystemDocAdjustedHours = 1.17m,
            ProductionValidationAdjustedHours = 4m,
            ProjectManagementAdjustedHours = 8m,
            CollaborationAdjustedHours = 12m
        };

        Assert.Equal(10m, entity.DevelopmentAdjustedHours);
        Assert.Equal(5m, entity.AnalysisAdjustedHours);
        Assert.Equal(3m, entity.BusinessDesignAdjustedHours);
        Assert.Equal(15m, entity.SystemTestingAdjustedHours);
        Assert.Equal(1.17m, entity.BaSystemDocAdjustedHours);
    }

    [Fact]
    public void ProjectEntity_Negative_NegativeAdjustmentsAllowed()
    {
        var entity = new ProjectEntity
        {
            DevelopmentAdjustedHours = -50m,
            SystemTestingAdjustedHours = -20m
        };
        Assert.Equal(-50m, entity.DevelopmentAdjustedHours);
        Assert.Equal(-20m, entity.SystemTestingAdjustedHours);
    }

    #endregion

    #region ProjectEntity — Test Cases

    [Fact]
    public void ProjectEntity_Positive_TestCasesStored()
    {
        var entity = new ProjectEntity
        {
            UseTestCasesForEstimate = true,
            TestCasesSimple = 125m,
            TestCasesMedium = 0m,
            TestCasesComplex = 75m,
            TestCasesVeryComplex = 0m,
            TestCaseIterations = 2.5m
        };

        Assert.True(entity.UseTestCasesForEstimate);
        Assert.Equal(125m, entity.TestCasesSimple);
        Assert.Equal(75m, entity.TestCasesComplex);
        Assert.Equal(2.5m, entity.TestCaseIterations);
    }

    #endregion

    #region ProjectEntity — Assumptions and Notes

    [Fact]
    public void ProjectEntity_HappyPath_AssumptionsStored()
    {
        var entity = new ProjectEntity
        {
            SeAssumptions = "SE assumptions text",
            BaAssumptions = "BA assumptions text",
            CollaborationAssumptions = "Collab assumptions",
            GeneralAssumptions = "General assumptions"
        };

        Assert.Equal("SE assumptions text", entity.SeAssumptions);
        Assert.Equal("BA assumptions text", entity.BaAssumptions);
    }

    [Fact]
    public void ProjectEntity_Positive_AllNotesStored()
    {
        var entity = new ProjectEntity
        {
            DevelopmentNotes = "Dev notes",
            AnalysisNotes = "Analysis notes",
            BusinessDesignNotes = "BD notes",
            SystemTestingNotes = "ST notes",
            PromotionNotes = "Promo notes",
            BaSystemDocNotes = "BA Doc notes",
            ProductionValidationNotes = "PV notes",
            ProjectManagementNotes = "PM notes",
            WprsNotes = "WPR notes",
            ClientMeetingsNotes = "Client notes",
            InternalMeetingsNotes = "Internal notes",
            AutomationTestCollabNotes = "Auto notes",
            ConsultantMentorNotes = "Consultant notes"
        };

        Assert.Equal("Dev notes", entity.DevelopmentNotes);
        Assert.Equal("PM notes", entity.ProjectManagementNotes);
        Assert.Equal("WPR notes", entity.WprsNotes);
    }

    #endregion

    #region ComponentEntryEntity — Happy Path

    [Fact]
    public void ComponentEntry_HappyPath_AllFieldsSet()
    {
        var entity = new ComponentEntryEntity
        {
            ProjectId = "abc123",
            LineNumber = 1,
            RequirementId = "REQ-001",
            ComponentType = "PowerBuilderWindows",
            Description = "Main window",
            ChangeType = "New",
            Size = "Large",
            Count = 3,
            BaseHoursPerUnit = 125.00m,
            TotalHours = 375.00m,
            Notes = "Critical component"
        };

        Assert.Equal("abc123", entity.ProjectId);
        Assert.Equal("REQ-001", entity.RequirementId);
        Assert.Equal("PowerBuilderWindows", entity.ComponentType);
        Assert.Equal(3, entity.Count);
        Assert.Equal(375.00m, entity.TotalHours);
    }

    [Fact]
    public void ComponentEntry_SadPath_DefaultCount1()
    {
        var entity = new ComponentEntryEntity();
        Assert.Equal(1, entity.Count);
    }

    #endregion

    #region CollaborationItemEntity — Happy Path

    [Fact]
    public void CollaborationItem_HappyPath_AllFieldsSet()
    {
        var entity = new CollaborationItemEntity
        {
            ProjectId = "proj-1",
            LineNumber = 1,
            TaskName = "WPRs",
            CollaborationType = "WPRs",
            NumberOfMeetings = 10,
            MeetingDurationMinutes = 60,
            NumberOfParticipants = 5,
            ParticipantPrepTimeMinutes = 15,
            TotalHours = 62.50m,
            Notes = "Weekly WPRs"
        };

        Assert.Equal("proj-1", entity.ProjectId);
        Assert.Equal("WPRs", entity.TaskName);
        Assert.Equal(10, entity.NumberOfMeetings);
        Assert.Equal(62.50m, entity.TotalHours);
    }

    [Fact]
    public void CollaborationItem_SadPath_DefaultValues()
    {
        var entity = new CollaborationItemEntity();
        Assert.Equal(1, entity.NumberOfMeetings);
        Assert.Equal(60, entity.MeetingDurationMinutes);
        Assert.Equal(3, entity.NumberOfParticipants);
        Assert.Equal(15, entity.ParticipantPrepTimeMinutes);
    }

    #endregion

    #region WeightedValueEntity — Happy Path

    [Fact]
    public void WeightedValueEntity_HappyPath()
    {
        var entity = new WeightedValueEntity
        {
            Id = 1,
            ComponentType = ComponentType.PowerBuilderWindows,
            Size = ComponentSize.Small,
            ChangeType = ChangeType.New,
            BaseHours = 25.00m,
            ModifiedBy = "System"
        };

        Assert.Equal(1, entity.Id);
        Assert.Equal(25.00m, entity.BaseHours);
        Assert.Equal("System", entity.ModifiedBy);
    }

    [Fact]
    public void WeightedValueEntity_SadPath_ZeroBaseHours()
    {
        var entity = new WeightedValueEntity { BaseHours = 0m };
        Assert.Equal(0m, entity.BaseHours);
    }

    #endregion

    #region EstimateSummary

    [Fact]
    public void EstimateSummary_HappyPath_AllFieldsSet()
    {
        var summary = new EstimateSummary
        {
            DevelopmentHours = 953.10m,
            SystemTestingHours = 285.93m,
            AnalysisHours = 61.96m,
            BusinessDesignHours = 185.86m,
            PromotionHours = 47.66m,
            BASystemDocumentationHours = 47.66m,
            ProductionValidationHours = 57.19m,
            ProjectManagementHours = 245.91m,
            GrandTotalHours = 1886m,
            TShirtSize = "XL1",
            ComponentCount = 8
        };

        Assert.Equal(953.10m, summary.DevelopmentHours);
        Assert.Equal("XL1", summary.TShirtSize);
        Assert.Equal(8, summary.ComponentCount);
    }

    [Fact]
    public void EstimateSummary_SadPath_DefaultValues()
    {
        var summary = new EstimateSummary();
        Assert.Equal(0m, summary.DevelopmentHours);
        Assert.Equal(string.Empty, summary.TShirtSize);
        Assert.Equal(0, summary.ComponentCount);
    }

    #endregion
}
