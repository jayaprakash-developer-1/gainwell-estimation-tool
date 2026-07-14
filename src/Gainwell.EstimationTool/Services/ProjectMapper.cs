using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Services;

/// <summary>
/// Centralized mapping between ProjectEntity (persistence) and ViewModels.
/// Eliminates the ~200 lines of repetitive property copying in Save/Load methods.
/// </summary>
public static class ProjectMapper
{
    /// <summary>
    /// Maps all ViewModel properties to a ProjectEntity for persistence.
    /// Used by both Create and Update operations.
    /// </summary>
    public static void MapToEntity(InitialEstimateViewModel vm, ProjectEntity entity)
    {
        entity.ProjectName = vm.ProjectName;
        entity.ChangeOrderId = vm.ChangeOrderId;
        entity.ProjectDescription = vm.ProjectDescription;
        entity.EstimatedBy = vm.EstimatedBy;
        entity.ReviewedBy = vm.ReviewedBy;
        entity.PmEffortPercentage = vm.PmEffortPercentage;
        entity.TotalDevelopmentHours = vm.TotalDevelopmentHours;
        entity.GrandTotalHours = vm.GrandTotalHours;
        entity.TShirtSize = vm.TShirtSize;
        entity.CollaborationHours = vm.TotalCollaborationHours;

        // Per-task adjusted hours
        entity.DevelopmentAdjustedHours = vm.DevelopmentAdjustedHours;
        entity.AnalysisAdjustedHours = vm.AnalysisAdjustedHours;
        entity.BusinessDesignAdjustedHours = vm.BusinessDesignAdjustedHours;
        entity.SystemTestingAdjustedHours = vm.SystemTestingAdjustedHours;
        entity.PromotionAdjustedHours = vm.PromotionAdjustedHours;
        entity.BaSystemDocAdjustedHours = vm.BaSystemDocAdjustedHours;
        entity.ProductionValidationAdjustedHours = vm.ProductionValidationAdjustedHours;
        entity.ProjectManagementAdjustedHours = vm.ProjectManagementAdjustedHours;
        entity.CollaborationAdjustedHours = vm.CollaborationAdjustedHours;
        entity.WprsAdjustedHours = vm.WprsAdjustedHours;
        entity.ClientMeetingsAdjustedHours = vm.ClientMeetingsAdjustedHours;
        entity.InternalMeetingsAdjustedHours = vm.InternalMeetingsAdjustedHours;
        entity.AutomationTestCollabAdjustedHours = vm.AutomationTestCollabAdjustedHours;
        entity.ConsultantMentorAdjustedHours = vm.ConsultantMentorAdjustedHours;
        entity.SeAdjustedHours = vm.DevelopmentAdjustedHours; // backward compat
        // Only write BaAdjustedHours from Initial Estimate if Detailed Estimate hasn't set it
        if (entity.BaAdjustedHours == 0)
            entity.BaAdjustedHours = vm.AnalysisAdjustedHours;

        // Assumptions & notes
        entity.SeAssumptions = vm.SeAssumptions;
        entity.BaAssumptions = vm.BaAssumptions;
        entity.CollaborationAssumptions = vm.CollaborationAssumptions;
        entity.GeneralAssumptions = vm.GeneralAssumptions;
        entity.AdjustedHoursComments = vm.AdjustedHoursComments;
        entity.DevelopmentNotes = vm.DevelopmentNotes;
        entity.AnalysisNotes = vm.AnalysisNotes;
        entity.BusinessDesignNotes = vm.BusinessDesignNotes;
        entity.SystemTestingNotes = vm.SystemTestingNotes;
        entity.PromotionNotes = vm.PromotionNotes;
        entity.BaSystemDocNotes = vm.BaSystemDocNotes;
        entity.ProductionValidationNotes = vm.ProductionValidationNotes;
        entity.ProjectManagementNotes = vm.ProjectManagementNotes;
        entity.WprsNotes = vm.WprsNotes;
        entity.ClientMeetingsNotes = vm.ClientMeetingsNotes;
        entity.InternalMeetingsNotes = vm.InternalMeetingsNotes;
        entity.AutomationTestCollabNotes = vm.AutomationTestCollabNotes;
        entity.ConsultantMentorNotes = vm.ConsultantMentorNotes;

        // Actual hours — Initial Estimate uses ProjectEntity.TotalActualHours (separate from Detailed Estimate's DetailedMiscFields.ActualHours)
        entity.TotalActualHours = vm.TotalActualHours;
        entity.ActualHoursAsOfDate = vm.ActualHoursAsOfDate;
        entity.TimeForEstimates = vm.TimeForEstimates;
        entity.UseTestCasesForEstimate = vm.UseTestCasesForEstimate;
        entity.TestCasesSimple = vm.TestCasesSimple;
        entity.TestCasesMedium = vm.TestCasesMedium;
        entity.TestCasesComplex = vm.TestCasesComplex;
        entity.TestCasesVeryComplex = vm.TestCasesVeryComplex;
        entity.TestCaseIterations = vm.TestCaseIterations;
    }

    /// <summary>
    /// Maps a ProjectEntity back to ViewModel properties.
    /// </summary>
    public static void MapToViewModel(ProjectEntity entity, InitialEstimateViewModel vm)
    {
        vm.ProjectName = entity.ProjectName;
        vm.ChangeOrderId = entity.ChangeOrderId;
        vm.ProjectDescription = entity.ProjectDescription;
        vm.EstimatedBy = !string.IsNullOrWhiteSpace(entity.EstimatedBy) ? entity.EstimatedBy : Environment.UserName;
        vm.ReviewedBy = entity.ReviewedBy ?? string.Empty;
        vm.PmEffortPercentage = entity.PmEffortPercentage;

        // Per-task adjusted hours (Initial Estimate's own fields — don't read BaAdjustedHours which Detailed Estimate owns)
        vm.DevelopmentAdjustedHours = entity.DevelopmentAdjustedHours != 0 ? entity.DevelopmentAdjustedHours : entity.SeAdjustedHours;
        vm.AnalysisAdjustedHours = entity.AnalysisAdjustedHours;
        vm.BusinessDesignAdjustedHours = entity.BusinessDesignAdjustedHours;
        vm.SystemTestingAdjustedHours = entity.SystemTestingAdjustedHours;
        vm.PromotionAdjustedHours = entity.PromotionAdjustedHours;
        vm.BaSystemDocAdjustedHours = entity.BaSystemDocAdjustedHours;
        vm.ProductionValidationAdjustedHours = entity.ProductionValidationAdjustedHours;
        vm.ProjectManagementAdjustedHours = entity.ProjectManagementAdjustedHours;
        vm.CollaborationAdjustedHours = entity.CollaborationAdjustedHours;
        vm.WprsAdjustedHours = entity.WprsAdjustedHours;
        vm.ClientMeetingsAdjustedHours = entity.ClientMeetingsAdjustedHours;
        vm.InternalMeetingsAdjustedHours = entity.InternalMeetingsAdjustedHours;
        vm.AutomationTestCollabAdjustedHours = entity.AutomationTestCollabAdjustedHours;
        vm.ConsultantMentorAdjustedHours = entity.ConsultantMentorAdjustedHours;

        // Assumptions & notes
        vm.SeAssumptions = entity.SeAssumptions;
        vm.BaAssumptions = entity.BaAssumptions;
        vm.CollaborationAssumptions = entity.CollaborationAssumptions;
        vm.GeneralAssumptions = entity.GeneralAssumptions;
        vm.AdjustedHoursComments = entity.AdjustedHoursComments;
        vm.DevelopmentNotes = entity.DevelopmentNotes;
        vm.AnalysisNotes = entity.AnalysisNotes;
        vm.BusinessDesignNotes = entity.BusinessDesignNotes;
        vm.SystemTestingNotes = entity.SystemTestingNotes;
        vm.PromotionNotes = entity.PromotionNotes;
        vm.BaSystemDocNotes = entity.BaSystemDocNotes;
        vm.ProductionValidationNotes = entity.ProductionValidationNotes;
        vm.ProjectManagementNotes = entity.ProjectManagementNotes;
        vm.WprsNotes = entity.WprsNotes;
        vm.ClientMeetingsNotes = entity.ClientMeetingsNotes;
        vm.InternalMeetingsNotes = entity.InternalMeetingsNotes;
        vm.AutomationTestCollabNotes = entity.AutomationTestCollabNotes;
        vm.ConsultantMentorNotes = entity.ConsultantMentorNotes;

        // Actual hours & test cases
        vm.TotalActualHours = entity.TotalActualHours;
        vm.ActualHoursAsOfDate = entity.ActualHoursAsOfDate;
        vm.TimeForEstimates = entity.TimeForEstimates;
        vm.UseTestCasesForEstimate = entity.UseTestCasesForEstimate;
        vm.TestCasesSimple = entity.TestCasesSimple;
        vm.TestCasesMedium = entity.TestCasesMedium;
        vm.TestCasesComplex = entity.TestCasesComplex;
        vm.TestCasesVeryComplex = entity.TestCasesVeryComplex;
        vm.TestCaseIterations = entity.TestCaseIterations;
    }

    /// <summary>
    /// Maps a ComponentRowViewModel to a ComponentEntryEntity.
    /// Uses TryParse-safe enum serialization.
    /// </summary>
    public static ComponentEntryEntity MapComponentToEntity(ComponentRowViewModel c, string projectId) => new()
    {
        ProjectId = projectId,
        LineNumber = c.LineNumber,
        RequirementId = c.RequirementId,
        ComponentType = c.ComponentType.ToString(),
        Description = c.Description,
        ChangeType = c.ChangeType.ToString(),
        Size = c.Size.ToString(),
        Count = c.Count,
        BaseHoursPerUnit = c.BaseHoursPerUnit,
        TotalHours = c.TotalHours,
        Notes = c.Notes
    };

    /// <summary>
    /// Maps a CollaborationRowViewModel to a CollaborationItemEntity.
    /// </summary>
    public static CollaborationItemEntity MapCollaborationToEntity(CollaborationRowViewModel item, string projectId) => new()
    {
        ProjectId = projectId,
        LineNumber = item.LineNumber,
        TaskName = item.TaskName,
        CollaborationType = item.CollabType.ToString(),
        NumberOfMeetings = item.NumberOfMeetings,
        MeetingDurationMinutes = item.MeetingDurationMinutes,
        NumberOfParticipants = item.NumberOfParticipants,
        ParticipantPrepTimeMinutes = item.ParticipantPrepTimeMinutes,
        TotalHours = item.TotalHours,
        Notes = item.Notes
    };

    /// <summary>
    /// Maps a ComponentEntryEntity back to a ComponentRowViewModel with safe enum parsing.
    /// </summary>
    public static ComponentRowViewModel MapEntityToComponent(ComponentEntryEntity entry)
    {
        var row = new ComponentRowViewModel
        {
            LineNumber = entry.LineNumber,
            RequirementId = entry.RequirementId,
            ComponentType = Enum.TryParse<ComponentType>(entry.ComponentType, out var ct) ? ct : ComponentType.None,
            Description = entry.Description,
            ChangeType = Enum.TryParse<ChangeType>(entry.ChangeType, out var cht) ? cht : ChangeType.None,
            Size = Enum.TryParse<ComponentSize>(entry.Size, out var sz) ? sz : ComponentSize.None,
            Count = entry.Count,
            Notes = entry.Notes
        };
        row.UpdateBaseHours();
        return row;
    }

    /// <summary>
    /// Maps a CollaborationItemEntity back to a CollaborationRowViewModel with safe enum parsing.
    /// </summary>
    public static CollaborationRowViewModel MapEntityToCollaboration(CollaborationItemEntity entry) => new()
    {
        LineNumber = entry.LineNumber,
        TaskName = entry.TaskName,
        CollabType = Enum.TryParse<CollaborationType>(entry.CollaborationType, out var ct) ? ct : CollaborationType.WPRs,
        NumberOfMeetings = entry.NumberOfMeetings,
        MeetingDurationMinutes = entry.MeetingDurationMinutes,
        NumberOfParticipants = entry.NumberOfParticipants,
        ParticipantPrepTimeMinutes = entry.ParticipantPrepTimeMinutes,
        Notes = entry.Notes
    };

    /// <summary>
    /// Maps all data from InitialEstimateViewModel to FinalEstimateViewModel.
    /// </summary>
    public static void MapToFinalEstimate(InitialEstimateViewModel source, FinalEstimateViewModel target)
    {
        target.ProjectName = source.ProjectName;
        target.ChangeOrderId = source.ChangeOrderId;
        target.ProjectDescription = source.ProjectDescription;
        target.EstimatedBy = source.EstimatedBy;
        target.ReviewedBy = source.ReviewedBy;
        target.EstimateDate = DateTime.Now;

        target.ComponentCount = source.ComponentCount;
        target.TotalDevelopmentHours = source.TotalDevelopmentHours;

        target.DevelopmentCalculated = source.TotalDevelopmentHours;
        target.DevelopmentAdjusted = source.DevelopmentAdjustedHours;
        target.DevelopmentTotal = source.DevelopmentTotalHours;

        target.SystemTestingCalculated = source.SystemTestingHours;
        target.SystemTestingAdjusted = source.SystemTestingAdjustedHours;
        target.SystemTestingTotal = source.SystemTestingTotalHours;

        target.AnalysisCalculated = source.AnalysisHours;
        target.AnalysisAdjusted = source.AnalysisAdjustedHours;
        target.AnalysisTotal = source.AnalysisTotalHours;

        target.BusinessDesignCalculated = source.BusinessDesignHours;
        target.BusinessDesignAdjusted = source.BusinessDesignAdjustedHours;
        target.BusinessDesignTotal = source.BusinessDesignTotalHours;

        target.PromotionCalculated = source.PromotionHours;
        target.PromotionAdjusted = source.PromotionAdjustedHours;
        target.PromotionTotal = source.PromotionTotalHours;

        target.BaSystemDocCalculated = source.BaSystemDocHours;
        target.BaSystemDocAdjusted = source.BaSystemDocAdjustedHours;
        target.BaSystemDocTotal = source.BaSystemDocTotalHours;

        target.ProductionValidationCalculated = source.ProductionValidationHours;
        target.ProductionValidationAdjusted = source.ProductionValidationAdjustedHours;
        target.ProductionValidationTotal = source.ProductionValidationTotalHours;

        target.PmEffortCalculated = source.ProjectManagementHours;
        target.PmEffortAdjusted = source.ProjectManagementAdjustedHours;
        target.PmEffortTotal = source.ProjectManagementTotalHours;
        target.PmEffortPercentage = source.PmEffortPercentage;

        target.WprsCalculated = source.WprsHours;
        target.WprsAdjusted = source.WprsAdjustedHours;
        target.WprsTotal = source.WprsTotalHours;

        target.ClientMeetingsCalculated = source.ClientMeetingsHours;
        target.ClientMeetingsAdjusted = source.ClientMeetingsAdjustedHours;
        target.ClientMeetingsTotal = source.ClientMeetingsTotalHours;

        target.InternalMeetingsCalculated = source.InternalMeetingsHours;
        target.InternalMeetingsAdjusted = source.InternalMeetingsAdjustedHours;
        target.InternalMeetingsTotal = source.InternalMeetingsTotalHours;

        target.AutomationTestCalculated = source.AutomationTestCollabHours;
        target.AutomationTestAdjusted = source.AutomationTestCollabAdjustedHours;
        target.AutomationTestTotal = source.AutomationTestCollabTotalHours;

        target.ConsultantMentorCalculated = source.ConsultantMentorHours;
        target.ConsultantMentorAdjusted = source.ConsultantMentorAdjustedHours;
        target.ConsultantMentorTotal = source.ConsultantMentorTotalHours;

        target.CollaborationTotalHours = source.CollaborationTotalHours;
        target.TimeForEstimates = source.TimeForEstimates;
        target.TotalActualHours = source.TotalActualHours;
        target.ActualHoursAsOfDate = source.ActualHoursAsOfDate;
        target.SubtotalHours = source.SubtotalHours;
        target.GrandTotalHours = source.GrandTotalHours;
        target.TShirtSize = source.TShirtSize;

        target.BaRoleHours = source.BaRoleHours;
        target.SeRoleHours = source.SeRoleHours;
        target.TesterRoleHours = source.TesterRoleHours;
        target.PmRoleHours = source.PmRoleHours;
        target.CollaborationRoleHours = source.CollaborationRoleHours;
        target.TotalRoleHours = source.BaRoleHours + source.SeRoleHours + source.TesterRoleHours
                              + source.PmRoleHours + source.CollaborationRoleHours;

        target.SeAssumptions = source.SeAssumptions;
        target.BaAssumptions = source.BaAssumptions;
        target.CollaborationAssumptions = source.CollaborationAssumptions;
        target.GeneralAssumptions = source.GeneralAssumptions;
        target.AdjustedHoursComments = source.AdjustedHoursComments;

        target.UseTestCases = source.UseTestCasesForEstimate;
        target.TestCasesSimple = source.TestCasesSimple;
        target.TestCasesMedium = source.TestCasesMedium;
        target.TestCasesComplex = source.TestCasesComplex;
        target.TestCasesVeryComplex = source.TestCasesVeryComplex;
        target.TestCaseIterations = source.TestCaseIterations;
    }
}
