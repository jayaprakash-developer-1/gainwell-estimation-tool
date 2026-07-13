using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.Services;

namespace Gainwell.EstimationTool.ViewModels;

public partial class InitialEstimateViewModel : ObservableObject
{
    // === Project Header ===
    [ObservableProperty]
    private string _projectName = string.Empty;

    [ObservableProperty]
    private string _changeOrderId = string.Empty;

    [ObservableProperty]
    private string _projectDescription = string.Empty;

    [ObservableProperty]
    private string _estimatedBy = Environment.UserName;

    [ObservableProperty]
    private string _reviewedBy = string.Empty;

    // === Configuration ===
    [ObservableProperty]
    private decimal _pmEffortPercentage = 15m;

    // Tracks the loaded project's ID (null = new unsaved project)
    private string? _currentProjectId;

    // Suppresses intermediate Recalculate calls during ClearAll
    private bool _suppressRecalculate;

    // === Calculated: Development & Derived Tasks ===
    [ObservableProperty]
    private decimal _totalDevelopmentHours;

    [ObservableProperty]
    private decimal _systemTestingHours;

    [ObservableProperty]
    private decimal _analysisHours;

    [ObservableProperty]
    private decimal _businessDesignHours;

    [ObservableProperty]
    private decimal _promotionHours;

    [ObservableProperty]
    private decimal _baSystemDocHours;

    [ObservableProperty]
    private decimal _productionValidationHours;

    [ObservableProperty]
    private decimal _projectManagementHours;

    [ObservableProperty]
    private decimal _subtotalHours;

    [ObservableProperty]
    private decimal _grandTotalHours;

    [ObservableProperty]
    private string _tShirtSize = "\u2014";

    [ObservableProperty]
    private int _componentCount;

    // === Collaboration ===
    [ObservableProperty]
    private int _collaborationCount;

    [ObservableProperty]
    private decimal _totalCollaborationHours;

    // === Adjusted Hours (per task type, Mid-Project Re-estimation) ===
    [ObservableProperty]
    private decimal _developmentAdjustedHours;

    [ObservableProperty]
    private decimal _analysisAdjustedHours;

    [ObservableProperty]
    private decimal _businessDesignAdjustedHours;

    [ObservableProperty]
    private decimal _systemTestingAdjustedHours;

    [ObservableProperty]
    private decimal _promotionAdjustedHours;

    [ObservableProperty]
    private decimal _baSystemDocAdjustedHours;

    [ObservableProperty]
    private decimal _productionValidationAdjustedHours;

    [ObservableProperty]
    private decimal _projectManagementAdjustedHours;

    [ObservableProperty]
    private decimal _collaborationAdjustedHours;

    // === Adjustment Row Notes ===
    [ObservableProperty]
    private string _developmentNotes = string.Empty;

    [ObservableProperty]
    private string _analysisNotes = string.Empty;

    [ObservableProperty]
    private string _businessDesignNotes = string.Empty;

    [ObservableProperty]
    private string _systemTestingNotes = string.Empty;

    [ObservableProperty]
    private string _promotionNotes = string.Empty;

    [ObservableProperty]
    private string _baSystemDocNotes = string.Empty;

    [ObservableProperty]
    private string _productionValidationNotes = string.Empty;

    [ObservableProperty]
    private string _projectManagementNotes = string.Empty;

    [ObservableProperty]
    private string _wprsNotes = string.Empty;

    [ObservableProperty]
    private string _clientMeetingsNotes = string.Empty;

    [ObservableProperty]
    private string _internalMeetingsNotes = string.Empty;

    [ObservableProperty]
    private string _automationTestCollabNotes = string.Empty;

    [ObservableProperty]
    private string _consultantMentorNotes = string.Empty;

    // Per-task totals (Calculated + Adjusted)
    [ObservableProperty]
    private decimal _developmentTotalHours;

    [ObservableProperty]
    private decimal _analysisTotalHours;

    [ObservableProperty]
    private decimal _businessDesignTotalHours;

    [ObservableProperty]
    private decimal _systemTestingTotalHours;

    [ObservableProperty]
    private decimal _promotionTotalHours;

    [ObservableProperty]
    private decimal _baSystemDocTotalHours;

    [ObservableProperty]
    private decimal _productionValidationTotalHours;

    [ObservableProperty]
    private decimal _projectManagementTotalHours;

    [ObservableProperty]
    private decimal _collaborationTotalHours;

    // Per-type collaboration hours (calculated from Collaboration tab)
    [ObservableProperty]
    private decimal _wprsHours;

    [ObservableProperty]
    private decimal _clientMeetingsHours;

    [ObservableProperty]
    private decimal _internalMeetingsHours;

    [ObservableProperty]
    private decimal _automationTestCollabHours;

    [ObservableProperty]
    private decimal _consultantMentorHours;

    // Per-type collaboration adjusted hours
    [ObservableProperty]
    private decimal _wprsAdjustedHours;

    [ObservableProperty]
    private decimal _clientMeetingsAdjustedHours;

    [ObservableProperty]
    private decimal _internalMeetingsAdjustedHours;

    [ObservableProperty]
    private decimal _automationTestCollabAdjustedHours;

    [ObservableProperty]
    private decimal _consultantMentorAdjustedHours;

    // Per-type collaboration totals (calculated + adjusted)
    [ObservableProperty]
    private decimal _wprsTotalHours;

    [ObservableProperty]
    private decimal _clientMeetingsTotalHours;

    [ObservableProperty]
    private decimal _internalMeetingsTotalHours;

    [ObservableProperty]
    private decimal _automationTestCollabTotalHours;

    [ObservableProperty]
    private decimal _consultantMentorTotalHours;

    // === Assumptions ===
    [ObservableProperty]
    private string _seAssumptions = string.Empty;

    [ObservableProperty]
    private string _baAssumptions = string.Empty;

    [ObservableProperty]
    private string _collaborationAssumptions = string.Empty;

    [ObservableProperty]
    private string _generalAssumptions = string.Empty;

    // === Adjusted Hours Comments ===
    [ObservableProperty]
    private string _adjustedHoursComments = string.Empty;

    // === Total Actual Hours (tracking hours already spent) ===
    [ObservableProperty]
    private decimal _totalActualHours;

    [ObservableProperty]
    private DateTime? _actualHoursAsOfDate;

    // === Time for Estimates (hours spent creating Detailed and Final estimates) ===
    [ObservableProperty]
    private decimal _timeForEstimates;

    // === Test Cases for System Testing (alternative to 30% formula) ===
    [ObservableProperty]
    private bool _useTestCasesForEstimate;

    [ObservableProperty]
    private decimal _testCasesSimple;

    [ObservableProperty]
    private decimal _testCasesMedium;

    [ObservableProperty]
    private decimal _testCasesComplex;

    [ObservableProperty]
    private decimal _testCasesVeryComplex;

    [ObservableProperty]
    private decimal _testCaseIterations = 0;

    // === Role Breakout ===
    [ObservableProperty]
    private decimal _baRoleHours;

    [ObservableProperty]
    private decimal _seRoleHours;

    [ObservableProperty]
    private decimal _testerRoleHours;

    [ObservableProperty]
    private decimal _pmRoleHours;

    [ObservableProperty]
    private decimal _collaborationRoleHours;

    // === Collections ===
    public ObservableCollection<ComponentRowViewModel> Components { get; } = new();
    public ObservableCollection<CollaborationRowViewModel> CollaborationItems { get; } = new();

    public Array ComponentTypes => Enum.GetValues<ComponentType>().OrderBy(v => v == ComponentType.None ? 0 : 1).ToArray();
    public Array ChangeTypes => Enum.GetValues<ChangeType>().OrderBy(v => v == ChangeType.None ? 0 : 1).ToArray();
    public Array Sizes => Enum.GetValues<ComponentSize>().OrderBy(v => v == ComponentSize.None ? 0 : 1).ToArray();
    public Array CollaborationTypes => Enum.GetValues<CollaborationType>();

    /// <summary>PM Effort % dropdown options matching Excel J34: 1–20</summary>
    public decimal[] PmEffortOptions => Enumerable.Range(1, 20).Select(i => (decimal)i).ToArray();

    /// <summary>Number of Meetings / WPRs dropdown matching Excel J36:J42 validation: 0–20</summary>
    public int[] MeetingCountOptions => Enumerable.Range(0, 21).ToArray();

    /// <summary>Meeting Duration (In Mins) dropdown matching Excel K36:K39 validation: 0,15,30,45,60</summary>
    public int[] MeetingDurationOptions => new[] { 0, 15, 30, 45, 60 };

    /// <summary>Number of Participants dropdown matching Excel L36:L42 validation: 0–20</summary>
    public int[] ParticipantCountOptions => Enumerable.Range(0, 21).ToArray();

    /// <summary>Participant Prep Time (In Mins) dropdown matching Excel M36:M39 validation: 0,15,30,...,180</summary>
    public int[] PrepTimeOptions => Enumerable.Range(0, 13).Select(i => i * 15).ToArray();

    /// <summary>
    /// True when at least one component row exists in the grid.
    /// Used to enable/disable Collaboration and Adjustments tabs.
    /// </summary>
    public bool HasComponents => Components.Count > 0;

    /// <summary>
    /// True when at least one component row has all required columns filled:
    /// Req#, Component Type, New/Change, Size, and Count > 0.
    /// Used to enable the Collaboration tab.
    /// </summary>
    public bool HasValidComponents => Components.Any(c =>
        !string.IsNullOrWhiteSpace(c.RequirementId) &&
        c.ComponentType != ComponentType.None &&
        c.ChangeType != ChangeType.None &&
        c.Size != ComponentSize.None &&
        c.Count > 0);

    public InitialEstimateViewModel()
    {
        Components.CollectionChanged += (_, e) =>
        {
            if (e.NewItems != null)
                foreach (ComponentRowViewModel row in e.NewItems)
                    row.PropertyChanged += OnComponentRowPropertyChanged;
            if (e.OldItems != null)
                foreach (ComponentRowViewModel row in e.OldItems)
                    row.PropertyChanged -= OnComponentRowPropertyChanged;
            Recalculate();
            OnPropertyChanged(nameof(HasComponents));
            OnPropertyChanged(nameof(HasValidComponents));
        };
        CollaborationItems.CollectionChanged += (_, _) => Recalculate();
        InitializeDefaultCollaborationItems();
    }

    private void OnComponentRowPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ComponentRowViewModel.RequirementId) ||
            e.PropertyName == nameof(ComponentRowViewModel.ComponentType) ||
            e.PropertyName == nameof(ComponentRowViewModel.ChangeType) ||
            e.PropertyName == nameof(ComponentRowViewModel.Size) ||
            e.PropertyName == nameof(ComponentRowViewModel.Count))
            OnPropertyChanged(nameof(HasValidComponents));
    }

    /// <summary>
    /// Pre-populates the 5 fixed collaboration rows matching the Excel layout:
    /// WPRs (10), Client Meetings (5), Internal Meetings (5), Automation Test Collaboration (5), Consultant/Mentor Effort (0)
    /// </summary>
    private void InitializeDefaultCollaborationItems()
    {
        var defaults = new[]
        {
            new { Type = CollaborationType.WPRs, Name = "WPRs" },
            new { Type = CollaborationType.ClientMeetings, Name = "Client Meetings" },
            new { Type = CollaborationType.InternalMeetings, Name = "Internal Meetings" },
            new { Type = CollaborationType.AutomationTestCollaboration, Name = "Automation Test Collaboration" },
        };

        for (int i = 0; i < defaults.Length; i++)
        {
            var d = defaults[i];
            var row = new CollaborationRowViewModel
            {
                LineNumber = i + 1,
                TaskName = d.Name,
                CollabType = d.Type,
                NumberOfMeetings = 0,
                MeetingDurationMinutes = 0,
                NumberOfParticipants = 0,
                ParticipantPrepTimeMinutes = 0
            };
            row.PropertyChanged += OnCollaborationChanged;
            CollaborationItems.Add(row);
        }
    }

    // === Component Commands ===
    [RelayCommand]
    private void AddComponent()
    {
        var row = new ComponentRowViewModel
        {
            LineNumber = Components.Count + 1
        };
        row.UpdateBaseHours();
        row.PropertyChanged += OnComponentChanged;
        Components.Add(row);
        Recalculate();
    }

    [RelayCommand]
    private void RemoveComponent(ComponentRowViewModel? component)
    {
        if (component is null) return;
        component.PropertyChanged -= OnComponentChanged;
        Components.Remove(component);
        for (int i = 0; i < Components.Count; i++)
            Components[i].LineNumber = i + 1;
        Recalculate();
    }

    [RelayCommand]
    private void ClearAll()
    {
        _suppressRecalculate = true;

        foreach (var c in Components)
            c.PropertyChanged -= OnComponentChanged;
        Components.Clear();
        foreach (var item in CollaborationItems)
            item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Clear();
        InitializeDefaultCollaborationItems();

        // Reset header fields
        ProjectName = string.Empty;
        ChangeOrderId = string.Empty;
        ProjectDescription = string.Empty;
        EstimatedBy = string.Empty;
        ReviewedBy = string.Empty;
        _currentProjectId = null;

        // Reset PM configuration to defaults
        PmEffortPercentage = 15m;

        // Reset adjusted hours
        DevelopmentAdjustedHours = 0m;
        AnalysisAdjustedHours = 0m;
        BusinessDesignAdjustedHours = 0m;
        SystemTestingAdjustedHours = 0m;
        PromotionAdjustedHours = 0m;
        BaSystemDocAdjustedHours = 0m;
        ProductionValidationAdjustedHours = 0m;
        ProjectManagementAdjustedHours = 0m;
        CollaborationAdjustedHours = 0m;
        WprsAdjustedHours = 0m;
        ClientMeetingsAdjustedHours = 0m;
        InternalMeetingsAdjustedHours = 0m;
        AutomationTestCollabAdjustedHours = 0m;
        ConsultantMentorAdjustedHours = 0m;
        AdjustedHoursComments = string.Empty;

        // Reset assumptions
        SeAssumptions = string.Empty;
        BaAssumptions = string.Empty;
        CollaborationAssumptions = string.Empty;
        GeneralAssumptions = string.Empty;

        // Reset test case estimation
        UseTestCasesForEstimate = false;
        TestCasesSimple = 0;
        TestCasesMedium = 0;
        TestCasesComplex = 0;
        TestCasesVeryComplex = 0;
        TestCaseIterations = 0m;

        // Reset actual hours
        TotalActualHours = 0m;
        ActualHoursAsOfDate = null;
        TimeForEstimates = 0m;

        _suppressRecalculate = false;
        Recalculate();
    }

    // === Collaboration Commands ===
    [RelayCommand]
    private void AddCollaborationItem()
    {
        var row = new CollaborationRowViewModel
        {
            LineNumber = CollaborationItems.Count + 1,
            NumberOfMeetings = 0,
            MeetingDurationMinutes = 0,
            NumberOfParticipants = 0,
            ParticipantPrepTimeMinutes = 0
        };
        row.PropertyChanged += OnCollaborationChanged;
        CollaborationItems.Add(row);
        Recalculate();
    }

    [RelayCommand]
    private void RemoveCollaborationItem(CollaborationRowViewModel? item)
    {
        if (item is null) return;
        item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Remove(item);
        for (int i = 0; i < CollaborationItems.Count; i++)
            CollaborationItems[i].LineNumber = i + 1;
        Recalculate();
    }

    // === Event Handlers ===
    private void OnComponentChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ComponentRowViewModel.ComponentType)
            or nameof(ComponentRowViewModel.Size)
            or nameof(ComponentRowViewModel.ChangeType)
            or nameof(ComponentRowViewModel.Count))
        {
            if (sender is ComponentRowViewModel row)
                row.UpdateBaseHours();
            Recalculate();
        }
    }

    private void OnCollaborationChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(CollaborationRowViewModel.NumberOfMeetings)
            or nameof(CollaborationRowViewModel.MeetingDurationMinutes)
            or nameof(CollaborationRowViewModel.NumberOfParticipants)
            or nameof(CollaborationRowViewModel.ParticipantPrepTimeMinutes))
        {
            Recalculate();
        }
    }

    partial void OnPmEffortPercentageChanged(decimal value) => Recalculate();
    partial void OnDevelopmentAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnAnalysisAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBusinessDesignAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnSystemTestingAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnPromotionAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnBaSystemDocAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProductionValidationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnProjectManagementAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnCollaborationAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnWprsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnClientMeetingsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnInternalMeetingsAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnAutomationTestCollabAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnConsultantMentorAdjustedHoursChanged(decimal value) => Recalculate();
    partial void OnUseTestCasesForEstimateChanged(bool value) => Recalculate();
    partial void OnTestCasesSimpleChanged(decimal value) => Recalculate();
    partial void OnTestCasesMediumChanged(decimal value) => Recalculate();
    partial void OnTestCasesComplexChanged(decimal value) => Recalculate();
    partial void OnTestCasesVeryComplexChanged(decimal value) => Recalculate();
    partial void OnTestCaseIterationsChanged(decimal value) => Recalculate();
    partial void OnTotalActualHoursChanged(decimal value) => Recalculate();
    partial void OnTimeForEstimatesChanged(decimal value) => Recalculate();

    // === Core Calculation Engine (delegates to CalculationEngine service) ===
    private void Recalculate()
    {
        if (_suppressRecalculate) return;
        // Step 1: Development = sum of all component hours
        decimal dev = 0m;
        foreach (var c in Components)
            dev += c.TotalHours;

        TotalDevelopmentHours = dev;
        ComponentCount = Components.Count(c => c.ComponentType != ComponentType.None 
            && c.ChangeType != ChangeType.None 
            && c.Size != ComponentSize.None 
            && c.Count > 0);
        CollaborationCount = CollaborationItems.Count;

        // Calculate per-type collaboration hours
        decimal collab = 0m;
        decimal wprs = 0m, clientMtg = 0m, internalMtg = 0m, autoTest = 0m, consultant = 0m;
        foreach (var item in CollaborationItems)
        {
            collab += item.TotalHours;
            switch (item.CollabType)
            {
                case CollaborationType.WPRs:                      wprs       += item.TotalHours; break;
                case CollaborationType.ClientMeetings:            clientMtg  += item.TotalHours; break;
                case CollaborationType.InternalMeetings:          internalMtg+= item.TotalHours; break;
                case CollaborationType.AutomationTestCollaboration:autoTest  += item.TotalHours; break;
            }
        }
        TotalCollaborationHours   = collab;
        WprsHours                 = wprs;
        ClientMeetingsHours       = clientMtg;
        InternalMeetingsHours     = internalMtg;
        AutomationTestCollabHours = autoTest;
        ConsultantMentorHours     = consultant;

        // Delegate to CalculationEngine for the 10-step pipeline
        var input = new CalculationInput
        {
            DevelopmentHours = dev,
            ComponentCount = ComponentCount,
            UseTestCasesForEstimate = UseTestCasesForEstimate,
            TestCasesSimple = TestCasesSimple,
            TestCasesMedium = TestCasesMedium,
            TestCasesComplex = TestCasesComplex,
            TestCasesVeryComplex = TestCasesVeryComplex,
            TestCaseIterations = TestCaseIterations,
            PmEffortPercentage = PmEffortPercentage,
            DevelopmentAdjustedHours = DevelopmentAdjustedHours,
            SystemTestingAdjustedHours = SystemTestingAdjustedHours,
            AnalysisAdjustedHours = AnalysisAdjustedHours,
            BusinessDesignAdjustedHours = BusinessDesignAdjustedHours,
            PromotionAdjustedHours = PromotionAdjustedHours,
            BaSystemDocAdjustedHours = BaSystemDocAdjustedHours,
            ProductionValidationAdjustedHours = ProductionValidationAdjustedHours,
            ProjectManagementAdjustedHours = ProjectManagementAdjustedHours,
            WprsHours = wprs,
            ClientMeetingsHours = clientMtg,
            InternalMeetingsHours = internalMtg,
            AutomationTestCollabHours = autoTest,
            ConsultantMentorHours = consultant,
            WprsAdjustedHours = WprsAdjustedHours,
            ClientMeetingsAdjustedHours = ClientMeetingsAdjustedHours,
            InternalMeetingsAdjustedHours = InternalMeetingsAdjustedHours,
            AutomationTestCollabAdjustedHours = AutomationTestCollabAdjustedHours,
            ConsultantMentorAdjustedHours = ConsultantMentorAdjustedHours,
            TimeForEstimates = TimeForEstimates,
            TotalActualHours = TotalActualHours
        };

        var result = CalculationEngine.Calculate(input);

        // Apply results to ViewModel properties
        SystemTestingHours = result.SystemTestingHours;
        AnalysisHours = result.AnalysisHours;
        BusinessDesignHours = result.BusinessDesignHours;
        PromotionHours = result.PromotionHours;
        BaSystemDocHours = result.BaSystemDocHours;
        ProductionValidationHours = result.ProductionValidationHours;
        ProjectManagementHours = result.ProjectManagementHours;

        WprsTotalHours = result.WprsTotalHours;
        ClientMeetingsTotalHours = result.ClientMeetingsTotalHours;
        InternalMeetingsTotalHours = result.InternalMeetingsTotalHours;
        AutomationTestCollabTotalHours = result.AutomationTestCollabTotalHours;
        ConsultantMentorTotalHours = result.ConsultantMentorTotalHours;

        DevelopmentTotalHours = result.DevelopmentTotalHours;
        SystemTestingTotalHours = result.SystemTestingTotalHours;
        AnalysisTotalHours = result.AnalysisTotalHours;
        BusinessDesignTotalHours = result.BusinessDesignTotalHours;
        PromotionTotalHours = result.PromotionTotalHours;
        BaSystemDocTotalHours = result.BaSystemDocTotalHours;
        ProductionValidationTotalHours = result.ProductionValidationTotalHours;
        ProjectManagementTotalHours = result.ProjectManagementTotalHours;
        CollaborationTotalHours = result.CollaborationTotalHours;

        SubtotalHours = result.SubtotalHours;
        GrandTotalHours = result.GrandTotalHours;
        TShirtSize = result.TShirtSize;

        BaRoleHours = result.BaRoleHours;
        SeRoleHours = result.SeRoleHours;
        TesterRoleHours = result.TesterRoleHours;
        PmRoleHours = result.PmRoleHours;
        CollaborationRoleHours = result.CollaborationRoleHours;
    }

    /// <summary>
    /// Excel ROUNDUP(x, 2) — delegates to CalculationEngine.
    /// Kept as public static for backward compatibility with tests and CollaborationRowViewModel.
    /// </summary>
    public static decimal RoundUp(decimal value) => CalculationEngine.RoundUp(value);

    // === Persistence ===
    public string? SaveProject()
    {
        if (string.IsNullOrWhiteSpace(ProjectName))
            return "Project Name is required.";
        if (string.IsNullOrWhiteSpace(ChangeOrderId))
            return "CO / Defect # is required.";
        if (!Regex.IsMatch(ChangeOrderId.Trim(), @"^\d{5}( \d{3})?$"))
            return "CO / Defect # must be in 99999 or 99999 999 format.";
        if (string.IsNullOrWhiteSpace(ProjectDescription))
            return "Description is required.";
        if (string.IsNullOrWhiteSpace(EstimatedBy))
            return "Estimated By is required.";
        if (string.IsNullOrWhiteSpace(ReviewedBy))
            return "Reviewed By is required.";
        if (!Components.Any(c => c.ComponentType != ComponentType.None))
            return "At least one component must be added before saving.";

        using var db = new EstimateDbContext();
        db.EnsureSchema();

        ProjectEntity? existing = null;
        if (_currentProjectId != null)
            existing = db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                        .FirstOrDefault(p => p.ProjectId == _currentProjectId);

        if (existing == null)
            existing = db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                        .FirstOrDefault(p => p.ProjectName == ProjectName);

        if (existing != null)
        {
            ProjectMapper.MapToEntity(this, existing);
            existing.LastModifiedDate = DateTime.UtcNow;
            existing.VersionNumber++;

            db.ComponentEntries.RemoveRange(existing.Components);
            existing.Components = Components
                .Where(c => c.ComponentType != ComponentType.None)
                .Select(c => ProjectMapper.MapComponentToEntity(c, existing.ProjectId))
                .ToList();

            db.CollaborationItems.RemoveRange(existing.CollaborationItems);
            existing.CollaborationItems = CollaborationItems
                .Select(item => ProjectMapper.MapCollaborationToEntity(item, existing.ProjectId))
                .ToList();

            _currentProjectId = existing.ProjectId;
        }
        else
        {
            var project = new ProjectEntity();
            ProjectMapper.MapToEntity(this, project);
            project.Components = Components
                .Where(c => c.ComponentType != ComponentType.None)
                .Select(c => ProjectMapper.MapComponentToEntity(c, project.ProjectId))
                .ToList();
            project.CollaborationItems = CollaborationItems
                .Select(item => ProjectMapper.MapCollaborationToEntity(item, project.ProjectId))
                .ToList();
            db.Projects.Add(project);
            _currentProjectId = project.ProjectId;
        }

        db.SaveChanges();
        return null;
    }

    public void LoadProject(ProjectEntity project)
    {
        foreach (var c in Components)
            c.PropertyChanged -= OnComponentChanged;
        Components.Clear();

        foreach (var item in CollaborationItems)
            item.PropertyChanged -= OnCollaborationChanged;
        CollaborationItems.Clear();

        _currentProjectId = project.ProjectId;
        ProjectMapper.MapToViewModel(project, this);

        foreach (var entry in project.Components.OrderBy(c => c.LineNumber))
        {
            var row = ProjectMapper.MapEntityToComponent(entry);
            row.PropertyChanged += OnComponentChanged;
            Components.Add(row);
        }

        var savedCollab = project.CollaborationItems.OrderBy(c => c.LineNumber).ToList();
        if (savedCollab.Count > 0)
        {
            foreach (var entry in savedCollab)
            {
                var row = ProjectMapper.MapEntityToCollaboration(entry);
                row.PropertyChanged += OnCollaborationChanged;
                CollaborationItems.Add(row);
            }
        }
        else
        {
            InitializeDefaultCollaborationItems();
        }

        Recalculate();
    }

    public static List<ProjectEntity> GetAllProjects()
    {
        using var db = new EstimateDbContext();
        db.EnsureSchema();
        return db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
                 .OrderByDescending(p => p.LastModifiedDate)
                 .ToList();
    }
}

// === Component Row ViewModel ===
public partial class ComponentRowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _lineNumber;

    [ObservableProperty]
    private string _requirementId = string.Empty;

    [ObservableProperty]
    private ComponentType _componentType = ComponentType.None;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private ChangeType _changeType = ChangeType.None;

    [ObservableProperty]
    private ComponentSize _size = ComponentSize.None;

    [ObservableProperty]
    private int _count = 0;

    [ObservableProperty]
    private decimal _baseHoursPerUnit;

    [ObservableProperty]
    private string _notes = string.Empty;

    public decimal TotalHours => BaseHoursPerUnit * Count;

    public void UpdateBaseHours()
    {
        if (ComponentType == ComponentType.None || Size == ComponentSize.None || ChangeType == ChangeType.None || Count <= 0)
        {
            BaseHoursPerUnit = 0m;
            OnPropertyChanged(nameof(TotalHours));
            return;
        }
        BaseHoursPerUnit = WeightedValues.GetBaseHours(ComponentType, Size, ChangeType);
        OnPropertyChanged(nameof(TotalHours));
    }

    partial void OnComponentTypeChanged(ComponentType value) => UpdateBaseHours();
    partial void OnSizeChanged(ComponentSize value) => UpdateBaseHours();
    partial void OnChangeTypeChanged(ChangeType value) => UpdateBaseHours();
    partial void OnCountChanged(int value) => UpdateBaseHours();
}

// === Collaboration Row ViewModel ===
/// <summary>
/// Matches Excel formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × NumParticipants
/// Excel columns: J=Number of Meetings/WPRs, K=Meeting Duration (In Mins), L=Number of Participants, M=Participant Prep Time (In Mins)
/// </summary>
public partial class CollaborationRowViewModel : ObservableValidator
{
    [ObservableProperty]
    private int _lineNumber;

    [ObservableProperty]
    private string _taskName = string.Empty;

    [ObservableProperty]
    private CollaborationType _collabType = CollaborationType.WPRs;

    /// <summary>Column J: Number of Meetings / WPRs</summary>
    [ObservableProperty]
    [Range(0, 20, ErrorMessage = "Number of Meetings must be between 0 and 20.")]
    private int _numberOfMeetings = 1;

    /// <summary>Column K: Meeting Duration (In Mins)</summary>
    [ObservableProperty]
    [CustomValidation(typeof(CollaborationRowViewModel), nameof(ValidateMeetingDuration))]
    private int _meetingDurationMinutes = 60;

    /// <summary>Column L: Number of Participants</summary>
    [ObservableProperty]
    [Range(0, 20, ErrorMessage = "Number of Participants must be between 0 and 20.")]
    private int _numberOfParticipants = 3;

    /// <summary>Column M: Participant Prep Time (In Mins)</summary>
    [ObservableProperty]
    [CustomValidation(typeof(CollaborationRowViewModel), nameof(ValidatePrepTime))]
    private int _participantPrepTimeMinutes = 15;

    [ObservableProperty]
    private string _notes = string.Empty;

    public static ValidationResult? ValidateMeetingDuration(int value, ValidationContext context)
    {
        int[] valid = { 0, 15, 30, 45, 60 };
        return valid.Contains(value)
            ? ValidationResult.Success
            : new ValidationResult("Meeting Duration must be 0, 15, 30, 45, or 60 minutes.");
    }

    public static ValidationResult? ValidatePrepTime(int value, ValidationContext context)
    {
        return value >= 0 && value <= 180 && value % 15 == 0
            ? ValidationResult.Success
            : new ValidationResult("Prep Time must be a multiple of 15 between 0 and 180.");
    }

    /// <summary>
    /// Excel formula: ROUNDUP((Meetings × Participants × Duration/60) + (Meetings × Participants × PrepTime/60), 2)
    /// Matches Excel G36 = ROUNDUP((J36*L36*(K36/60))+(J36*L36*(M36/60)),2)
    /// </summary>
    public decimal TotalHours => InitialEstimateViewModel.RoundUp(NumberOfMeetings * ((MeetingDurationMinutes / 60m) + (ParticipantPrepTimeMinutes / 60m)) * NumberOfParticipants);

    partial void OnNumberOfMeetingsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnMeetingDurationMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnNumberOfParticipantsChanged(int value) => OnPropertyChanged(nameof(TotalHours));
    partial void OnParticipantPrepTimeMinutesChanged(int value) => OnPropertyChanged(nameof(TotalHours));
}
