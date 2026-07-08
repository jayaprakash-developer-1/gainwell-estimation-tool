using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC;

public partial class DetailedEstimateWindow : Window
{
    public ObservableCollection<ComponentTypeRow> ComponentRows { get; } = new();
    public ObservableCollection<BaTestCaseRow> BaTestCases { get; } = new();
    public ObservableCollection<BaTestCaseRow> BaRegressionRows { get; } = new();
    public ObservableCollection<BaValidationRow> BaValidationItems { get; } = new();
    public ObservableCollection<ConsultantRow> Consultants { get; } = new();
    public ObservableCollection<SummaryRow> SeSummaryRows { get; } = new();
    public ObservableCollection<SummaryRow> BaSummaryRows { get; } = new();
    public ObservableCollection<SummaryRow> CollabSummaryRows { get; } = new();

    /// <summary>Available experience level options for the BA grid dropdowns (includes "Select" placeholder).</summary>
    public string[] ExpLevelOptions { get; } = new[] { "Select", "New to Area", "Proficient", "Expert" };

    /// <summary>Available task names for the Write System Test Cases Task dropdown (includes "Select" placeholder).</summary>
    public string[] BaTaskNameOptions { get; } = new[]
    {
        "Select",
        "Understanding Requirements",
        "Write System Test Cases (# cases)",
        "Iteration",
        "Data Preparation",
        "ALM Upload, Linking and Generating Reports",
        "Sys Test Execution",
        "Pre Release Defects Creation and Retest"
    };

    private ProjectEntity? _currentProject;

    public DetailedEstimateWindow() : this(null) { }

    public DetailedEstimateWindow(ProjectEntity? project)
    {
        InitializeComponent();
        _currentProject = project;
        InitializeComponentRows();
        InitializeGrids();
        InitializeSummaryGrids();
        LoadProjectInfo();
    }

    private void LoadProjectInfo()
    {
        if (_currentProject != null)
        {
            ChangeOrderTextBox.Text = _currentProject.ChangeOrderId ?? string.Empty;
            BaChangeOrderTextBox.Text = _currentProject.ChangeOrderId ?? string.Empty;
            ProjectNameTextBox.Text = _currentProject.ProjectName ?? string.Empty;
            EstimatedByTextBox.Text = _currentProject.EstimatedBy ?? Environment.UserName;
            if (!string.IsNullOrWhiteSpace(_currentProject.ProjectName))
            {
                Title = $"Detailed Estimate — {_currentProject.ProjectName}";
                ProjectSubtitleText.Text = $"{_currentProject.ProjectName} | CO: {_currentProject.ChangeOrderId}";
            }
        }
        else
        {
            EstimatedByTextBox.Text = Environment.UserName;
        }
    }

    private void OnOpenHistoryClick(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow { Owner = this };
        if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
        {
            _currentProject = historyWindow.SelectedProject;
            ChangeOrderTextBox.Text = _currentProject.ChangeOrderId ?? string.Empty;
            BaChangeOrderTextBox.Text = _currentProject.ChangeOrderId ?? string.Empty;
            ProjectNameTextBox.Text = _currentProject.ProjectName ?? string.Empty;
            Title = $"Detailed Estimate — {_currentProject.ProjectName}";
            ProjectSubtitleText.Text = $"{_currentProject.ProjectName} | CO: {_currentProject.ChangeOrderId}";
        }
    }

    public void LoadFromProject(ProjectEntity project)
    {
        _currentProject = project;
        LoadProjectInfo();
    }

    public ProjectEntity? GetCurrentProjectEntity()
    {
        return _currentProject ?? new ProjectEntity
        {
            ProjectName = ProjectNameTextBox.Text.Trim(),
            ChangeOrderId = ChangeOrderTextBox.Text.Trim()
        };
    }

    private void InitializeComponentRows()
    {
        var componentTypes = new[]
        {
            ComponentType.PowerBuilderWindows,
            ComponentType.Reports,
            ComponentType.ProgramsDBStoredProcs,
            ComponentType.SupportModules,
            ComponentType.DBManipulation,
            ComponentType.DatabaseReview,
            ComponentType.Webpage,
            ComponentType.K2Workflow,
            ComponentType.K2SmartForm,
            ComponentType.TestAutomationUFT,
            ComponentType.MISC
        };

        int line = 1;
        foreach (var ct in componentTypes)
        {
            ComponentRows.Add(new ComponentTypeRow
            {
                LineNumber = line++,
                ComponentType = ct,
                ComponentTypeName = GetComponentDisplayName(ct)
            });
        }

        ComponentTypeGrid.ItemsSource = ComponentRows;
    }

    private void OnComponentRowDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (ComponentTypeGrid.SelectedItem is ComponentTypeRow row)
        {
            var dialog = new SeComponentDetailDialog(row.ComponentType, row.ModuleEntries) { Owner = this };
            if (dialog.ShowDialog() == true && dialog.ResultEntries != null)
            {
                row.ModuleEntries = dialog.ResultEntries;

                // Aggregate totals from all module entries
                int s = 0, m = 0, c = 0;
                decimal complexityTotal = 0, adjExpTotal = 0, adjHrs = 0, grandTotal = 0;
                foreach (var entry in row.ModuleEntries)
                {
                    s += entry.SimpleCount;
                    m += entry.ModerateCount;
                    c += entry.ComplexCount;
                    complexityTotal += entry.ComplexityTotal;
                    adjExpTotal += entry.AdjustedExpLevel;
                    adjHrs += entry.AdjustedHrs;
                    grandTotal += entry.GrandTotal;
                }
                row.SimpleTotal = s;
                row.ModerateTotal = m;
                row.ComplexTotal = c;
                row.HoursTotal = complexityTotal;
                row.AdjustedExpLevel = adjExpTotal;
                row.AdjustedHrs = adjHrs;
                row.GrandTotal = grandTotal;

                ComponentTypeGrid.Items.Refresh();
                UpdateSeTotals();
            }
        }
    }

    private void UpdateSeTotals()
    {
        var devTotal = ComponentRows.Sum(r => r.GrandTotal);
        decimal promotion = decimal.TryParse(PromotionHoursTextBox.Text, out var p) ? p : 0;
        decimal doc = decimal.TryParse(SystemDocHoursTextBox.Text, out var d) ? d : 0;
        var grandTotal = devTotal + promotion + doc;

        SeDevTotalText.Text = devTotal.ToString("N2");
        SePromotionTotalText.Text = promotion.ToString("N2");
        SeDocTotalText.Text = doc.ToString("N2");
        SePromoDocTotalText.Text = (promotion + doc).ToString("N2");
        SeTotalText.Text = grandTotal.ToString("N2");
        SeComponentCountText.Text = ComponentRows.Count(r => r.GrandTotal > 0).ToString();
    }

    private void InitializeGrids()
    {
        // Setup BA Test Cases Grid — 21 rows matching Excel Dtl BA_Considerations (rows R15-R37)
        // New to Area group (R15-R21)
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Understanding Requirements",                      Category = BaCategory.SystemTesting, TaskType = "UnderstandingRequirements", IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: Write System Test Cases (# cases)",          Category = BaCategory.SystemTesting, TaskType = "WriteSystemTestCases",       IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: Iteration",                                  Category = BaCategory.SystemTesting, TaskType = "Iteration",                  IsInfoRow = true,  ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: Data Preparation",                           Category = BaCategory.SystemTesting, TaskType = "DataPreparation",            IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: ALM Upload, Linking and Generating Reports", Category = BaCategory.SystemTesting, TaskType = "AlmTasks",                   IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: Sys Test Execution",                         Category = BaCategory.SystemTesting, TaskType = "TestExecution",              IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "New: Pre Release Defects Creation and Retest",    Category = BaCategory.SystemTesting, TaskType = "PreReleaseDefects",           IsInfoRow = false, ExperienceLevel = ExperienceLevel.NewToArea  });
        // Proficient group (R23-R29)
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Understanding Requirements",                           Category = BaCategory.SystemTesting, TaskType = "UnderstandingRequirements", IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: Write System Test Cases (# cases)",        Category = BaCategory.SystemTesting, TaskType = "WriteSystemTestCases",       IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: Iteration",                                Category = BaCategory.SystemTesting, TaskType = "Iteration",                  IsInfoRow = true,  ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: Data Preparation",                         Category = BaCategory.SystemTesting, TaskType = "DataPreparation",            IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: ALM Upload, Linking and Generating Reports", Category = BaCategory.SystemTesting, TaskType = "AlmTasks",                IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: Sys Test Execution",                       Category = BaCategory.SystemTesting, TaskType = "TestExecution",              IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Proficient: Pre Release Defects Creation and Retest",  Category = BaCategory.SystemTesting, TaskType = "PreReleaseDefects",           IsInfoRow = false, ExperienceLevel = ExperienceLevel.Proficient });
        // Expert group (R31-R37)
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Understanding Requirements",                    Category = BaCategory.SystemTesting, TaskType = "UnderstandingRequirements", IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: Write System Test Cases (# cases)",     Category = BaCategory.SystemTesting, TaskType = "WriteSystemTestCases",       IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: Iteration",                             Category = BaCategory.SystemTesting, TaskType = "Iteration",                  IsInfoRow = true,  ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: Data Preparation",                      Category = BaCategory.SystemTesting, TaskType = "DataPreparation",            IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: ALM Upload, Linking and Generating Reports", Category = BaCategory.SystemTesting, TaskType = "AlmTasks",             IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: Sys Test Execution",                    Category = BaCategory.SystemTesting, TaskType = "TestExecution",              IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Expert: Pre Release Defects Creation and Retest", Category = BaCategory.SystemTesting, TaskType = "PreReleaseDefects",         IsInfoRow = false, ExperienceLevel = ExperienceLevel.Expert     });
        BaTestCasesGrid.ItemsSource = BaTestCases;

        // Setup WTC→Iteration→Derived propagation for each experience-level group
        SetupWtcGroup(0);   // New to Area (rows 0–6)
        SetupWtcGroup(7);   // Proficient  (rows 7–13)
        SetupWtcGroup(14);  // Expert      (rows 14–20)

        // Setup Regression Testing — Proficient hardcoded (matching Excel Row 40)
        BaRegressionRows.Add(new BaTestCaseRow { TaskName = "Regression testing/document (# cases)", Category = BaCategory.SystemTesting, TaskType = "RegressionTesting", ExperienceLevel = ExperienceLevel.Proficient });
        BaRegressionGrid.ItemsSource = BaRegressionRows;

        // Setup Production Validation Grid — 9 rows matching Excel labels and order
        BaValidationItems.Add(new BaValidationRow { TaskName = "General validation",            TaskType = "GeneralValidation", ExperienceLevel = ExperienceLevel.NewToArea  });
        BaValidationItems.Add(new BaValidationRow { TaskName = "Pricing changes",               TaskType = "PricingChanges",    ExperienceLevel = ExperienceLevel.NewToArea  });
        BaValidationItems.Add(new BaValidationRow { TaskName = "Reference changes",             TaskType = "ReferenceChanges",  ExperienceLevel = ExperienceLevel.NewToArea  });
        BaValidationItems.Add(new BaValidationRow { TaskName = "2nd Exp General validation",    TaskType = "GeneralValidation", ExperienceLevel = ExperienceLevel.Proficient });
        BaValidationItems.Add(new BaValidationRow { TaskName = "2nd Exp Pricing changes",       TaskType = "PricingChanges",    ExperienceLevel = ExperienceLevel.Proficient });
        BaValidationItems.Add(new BaValidationRow { TaskName = "2nd Exp Reference changes",     TaskType = "ReferenceChanges",  ExperienceLevel = ExperienceLevel.Proficient });
        BaValidationItems.Add(new BaValidationRow { TaskName = "3rd Exp General validation",    TaskType = "GeneralValidation", ExperienceLevel = ExperienceLevel.Expert     });
        BaValidationItems.Add(new BaValidationRow { TaskName = "3rd Exp Pricing changes",       TaskType = "PricingChanges",    ExperienceLevel = ExperienceLevel.Expert     });
        BaValidationItems.Add(new BaValidationRow { TaskName = "3rd Exp Reference changes",     TaskType = "ReferenceChanges",  ExperienceLevel = ExperienceLevel.Expert     });
        BaProductionValidationGrid.ItemsSource = BaValidationItems;

        // Complexity columns are now separate count inputs — no ComboBox ItemsSource needed

        // Subscribe to PropertyChanged on all BA rows so summary strips update live
        // (equivalent to Excel's SUM formulas auto-recalculating on cell change)
        void OnBaRowChanged(object? s, System.ComponentModel.PropertyChangedEventArgs _)
        {
            if (IsLoaded) UpdateSummaryTab();
        }
        foreach (var row in BaTestCases)       row.PropertyChanged += OnBaRowChanged;
        foreach (var row in BaRegressionRows)  row.PropertyChanged += OnBaRowChanged;
        foreach (var row in BaValidationItems) row.PropertyChanged += OnBaRowChanged;

        // Setup Consultants Grid
        for (int i = 0; i < 5; i++)
            Consultants.Add(new ConsultantRow { Name = "", Hours = 0 });
        ConsultantGrid.ItemsSource = Consultants;
    }

    private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Commit any pending DataGrid edits before switching tabs to prevent validation dialogs
        if (!IsLoaded) return;
        if (e.OriginalSource is TabControl)
        {
            CommitAllDataGridEdits();
            UpdateSummaryTab();
        }
    }

    private static decimal MRound(decimal value, decimal multiple = 0.25m) =>
        multiple == 0 ? value : Math.Round(value / multiple, MidpointRounding.AwayFromZero) * multiple;

    private void SetupWtcGroup(int g)
    {
        var wtcRow  = BaTestCases[g + 1]; // WTC
        var iterRow = BaTestCases[g + 2]; // Iteration
        wtcRow.ApplyIteration = true;
        BaTestCases[g + 3].UseEffectiveCounts = true; // DataPrep
        BaTestCases[g + 4].UseEffectiveCounts = true; // ALM
        BaTestCases[g + 5].UseEffectiveCounts = true; // SysTest
        BaTestCases[g + 6].UseEffectiveCounts = true; // PreRelease
        PropagateWtcIteration(g);
        wtcRow.PropertyChanged  += (_, e) => { if (e.PropertyName is nameof(BaTestCaseRow.SimpleCount) or nameof(BaTestCaseRow.ModerateCount) or nameof(BaTestCaseRow.ComplexCount) or nameof(BaTestCaseRow.VeryComplexCount)) PropagateWtcIteration(g); };
        iterRow.PropertyChanged += (_, e) => { if (e.PropertyName is nameof(BaTestCaseRow.SimpleCount) or nameof(BaTestCaseRow.ModerateCount) or nameof(BaTestCaseRow.ComplexCount) or nameof(BaTestCaseRow.VeryComplexCount)) PropagateWtcIteration(g); };
    }

    private void PropagateWtcIteration(int g)
    {
        var wtc  = BaTestCases[g + 1];
        var iter = BaTestCases[g + 2];
        // Treat iteration count of 0 as 1 (tooltip: "minimum value of 1.00 is required")
        decimal iS = iter.SimpleCount      == 0 ? 1m : iter.SimpleCount;
        decimal iM = iter.ModerateCount    == 0 ? 1m : iter.ModerateCount;
        decimal iC = iter.ComplexCount     == 0 ? 1m : iter.ComplexCount;
        decimal iV = iter.VeryComplexCount == 0 ? 1m : iter.VeryComplexCount;
        wtc.SetIterationFactors(iS, iM, iC, iV);
        decimal es = wtc.SimpleCount     * iS;
        decimal em = wtc.ModerateCount   * iM;
        decimal ec = wtc.ComplexCount    * iC;
        decimal ev = wtc.VeryComplexCount * iV;
        BaTestCases[g + 3].SetEffectiveCounts(es, em, ec, ev);
        BaTestCases[g + 4].SetEffectiveCounts(es, em, ec, ev);
        BaTestCases[g + 5].SetEffectiveCounts(es, em, ec, ev);
        BaTestCases[g + 6].SetEffectiveCounts(es, em, ec, ev);
    }

    private void OnBaGridSingleClickEdit(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (sender is not DataGrid grid) return;
        var dep = (DependencyObject)e.OriginalSource;
        while (dep != null && dep is not DataGridCell)
            dep = System.Windows.Media.VisualTreeHelper.GetParent(dep);
        if (dep is DataGridCell cell && !cell.IsEditing && !cell.IsReadOnly)
        {
            // Fully exit any active row edit before opening a new cell.
            // CommitEdit(Row) is more thorough than Cell-level commit; if it fails
            // (e.g. validation error) CancelEdit ensures the grid is never left stuck.
            if (!grid.CommitEdit(DataGridEditingUnit.Row, true))
                grid.CancelEdit(DataGridEditingUnit.Row);
            grid.Focus();
            grid.CurrentCell = new DataGridCellInfo(cell);
            grid.BeginEdit(e);
        }
    }

    private void OnBaTestCasesGridBeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is not BaTestCaseRow row) return;
        var header = e.Column.Header?.ToString();
        if (row.AreCountsAutoCalculated && header is "Simple" or "Moderate" or "Complex" or "Very Complex")
        {
            e.Cancel = true;
            return;
        }
        // Adjusted Hrs is N/A for Iteration rows
        if (row.IsInfoRow && header == "Adjusted Hrs")
            e.Cancel = true;
    }

    private void OnSummaryFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateSummaryTab();
    }

    private void OnBaAssumptionsTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        int len = BaAssumptionsDetailTextBox?.Text.Length ?? 0;
        if (BaAssumptionsCounter != null)
        {
            BaAssumptionsCounter.Text = $"{len} / 4000";
            BaAssumptionsCounter.Foreground = len >= 3800
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Gray;
        }
    }

    private void OnBaAdjNotesTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        int len = BaAdjustedHrsCommentTextBox?.Text.Length ?? 0;
        if (BaAdjNotesCounter != null)
        {
            BaAdjNotesCounter.Text = $"{len} / 4000";
            BaAdjNotesCounter.Foreground = len >= 3800
                ? System.Windows.Media.Brushes.Red
                : System.Windows.Media.Brushes.Gray;
        }
    }

    // ===== New Premium UI: Card-based event handlers =====

    private static decimal ParseDecimal(string? text) =>
        decimal.TryParse(text?.Trim(), out var v) ? v : 0m;

    private void OnBaCardValueChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        SyncCardsToModel();
        UpdateBaCardDisplays();
        UpdateSummaryTab();
    }

    private void OnPvRadioChanged(object sender, RoutedEventArgs e)
    {
        if (!IsLoaded) return;
        SyncPvRadiosToModel();
        UpdateBaCardDisplays();
        UpdateSummaryTab();
    }

    /// <summary>Sync card TextBox values → BaTestCases model rows (preserves all calculation logic).</summary>
    private void SyncCardsToModel()
    {
        // New to Area group (indices 0-6)
        BaTestCases[0].SimpleCount = ParseDecimal(BaNewUR_Simple?.Text);
        BaTestCases[0].ModerateCount = ParseDecimal(BaNewUR_Moderate?.Text);
        BaTestCases[0].ComplexCount = ParseDecimal(BaNewUR_Complex?.Text);
        BaTestCases[0].VeryComplexCount = ParseDecimal(BaNewUR_VComplex?.Text);
        BaTestCases[0].ManualAdjHours = ParseDecimal(BaNewUR_AdjHrs?.Text);

        BaTestCases[1].SimpleCount = ParseDecimal(BaNewWTC_Simple?.Text);
        BaTestCases[1].ModerateCount = ParseDecimal(BaNewWTC_Moderate?.Text);
        BaTestCases[1].ComplexCount = ParseDecimal(BaNewWTC_Complex?.Text);
        BaTestCases[1].VeryComplexCount = ParseDecimal(BaNewWTC_VComplex?.Text);

        BaTestCases[2].SimpleCount = ParseDecimal(BaNewIter_Simple?.Text);
        BaTestCases[2].ModerateCount = ParseDecimal(BaNewIter_Moderate?.Text);
        BaTestCases[2].ComplexCount = ParseDecimal(BaNewIter_Complex?.Text);
        BaTestCases[2].VeryComplexCount = ParseDecimal(BaNewIter_VComplex?.Text);

        BaTestCases[3].ManualAdjHours = ParseDecimal(BaNewDP_AdjHrs?.Text);
        BaTestCases[4].ManualAdjHours = ParseDecimal(BaNewALM_AdjHrs?.Text);
        BaTestCases[5].ManualAdjHours = ParseDecimal(BaNewSTE_AdjHrs?.Text);
        BaTestCases[6].ManualAdjHours = ParseDecimal(BaNewPRD_AdjHrs?.Text);

        // Proficient group (indices 7-13)
        BaTestCases[7].SimpleCount = ParseDecimal(BaProfUR_Simple?.Text);
        BaTestCases[7].ModerateCount = ParseDecimal(BaProfUR_Moderate?.Text);
        BaTestCases[7].ComplexCount = ParseDecimal(BaProfUR_Complex?.Text);
        BaTestCases[7].VeryComplexCount = ParseDecimal(BaProfUR_VComplex?.Text);
        BaTestCases[7].ManualAdjHours = ParseDecimal(BaProfUR_AdjHrs?.Text);

        BaTestCases[8].SimpleCount = ParseDecimal(BaProfWTC_Simple?.Text);
        BaTestCases[8].ModerateCount = ParseDecimal(BaProfWTC_Moderate?.Text);
        BaTestCases[8].ComplexCount = ParseDecimal(BaProfWTC_Complex?.Text);
        BaTestCases[8].VeryComplexCount = ParseDecimal(BaProfWTC_VComplex?.Text);

        BaTestCases[9].SimpleCount = ParseDecimal(BaProfIter_Simple?.Text);
        BaTestCases[9].ModerateCount = ParseDecimal(BaProfIter_Moderate?.Text);
        BaTestCases[9].ComplexCount = ParseDecimal(BaProfIter_Complex?.Text);
        BaTestCases[9].VeryComplexCount = ParseDecimal(BaProfIter_VComplex?.Text);

        BaTestCases[10].ManualAdjHours = ParseDecimal(BaProfDP_AdjHrs?.Text);
        BaTestCases[11].ManualAdjHours = ParseDecimal(BaProfALM_AdjHrs?.Text);
        BaTestCases[12].ManualAdjHours = ParseDecimal(BaProfSTE_AdjHrs?.Text);
        BaTestCases[13].ManualAdjHours = ParseDecimal(BaProfPRD_AdjHrs?.Text);

        // Expert group (indices 14-20)
        BaTestCases[14].SimpleCount = ParseDecimal(BaExpUR_Simple?.Text);
        BaTestCases[14].ModerateCount = ParseDecimal(BaExpUR_Moderate?.Text);
        BaTestCases[14].ComplexCount = ParseDecimal(BaExpUR_Complex?.Text);
        BaTestCases[14].VeryComplexCount = ParseDecimal(BaExpUR_VComplex?.Text);
        BaTestCases[14].ManualAdjHours = ParseDecimal(BaExpUR_AdjHrs?.Text);

        BaTestCases[15].SimpleCount = ParseDecimal(BaExpWTC_Simple?.Text);
        BaTestCases[15].ModerateCount = ParseDecimal(BaExpWTC_Moderate?.Text);
        BaTestCases[15].ComplexCount = ParseDecimal(BaExpWTC_Complex?.Text);
        BaTestCases[15].VeryComplexCount = ParseDecimal(BaExpWTC_VComplex?.Text);

        BaTestCases[16].SimpleCount = ParseDecimal(BaExpIter_Simple?.Text);
        BaTestCases[16].ModerateCount = ParseDecimal(BaExpIter_Moderate?.Text);
        BaTestCases[16].ComplexCount = ParseDecimal(BaExpIter_Complex?.Text);
        BaTestCases[16].VeryComplexCount = ParseDecimal(BaExpIter_VComplex?.Text);

        BaTestCases[17].ManualAdjHours = ParseDecimal(BaExpDP_AdjHrs?.Text);
        BaTestCases[18].ManualAdjHours = ParseDecimal(BaExpALM_AdjHrs?.Text);
        BaTestCases[19].ManualAdjHours = ParseDecimal(BaExpSTE_AdjHrs?.Text);
        BaTestCases[20].ManualAdjHours = ParseDecimal(BaExpPRD_AdjHrs?.Text);
    }

    /// <summary>Sync PV radio button selections → BaValidationItems model (0=none, 1=selected).</summary>
    private void SyncPvRadiosToModel()
    {
        // Row 0: New GV
        BaValidationItems[0].SimpleCount = BaPvNewGV_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[0].ModerateCount = BaPvNewGV_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[0].ComplexCount = BaPvNewGV_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[0].VeryComplexCount = BaPvNewGV_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[0].ManualAdjHours = ParseDecimal(BaPvNewGV_Adj?.Text);
        // Row 1: New PC
        BaValidationItems[1].SimpleCount = BaPvNewPC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[1].ModerateCount = BaPvNewPC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[1].ComplexCount = BaPvNewPC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[1].VeryComplexCount = BaPvNewPC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[1].ManualAdjHours = ParseDecimal(BaPvNewPC_Adj?.Text);
        // Row 2: New RC
        BaValidationItems[2].SimpleCount = BaPvNewRC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[2].ModerateCount = BaPvNewRC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[2].ComplexCount = BaPvNewRC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[2].VeryComplexCount = BaPvNewRC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[2].ManualAdjHours = ParseDecimal(BaPvNewRC_Adj?.Text);
        // Row 3: Prof GV
        BaValidationItems[3].SimpleCount = BaPvProfGV_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[3].ModerateCount = BaPvProfGV_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[3].ComplexCount = BaPvProfGV_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[3].VeryComplexCount = BaPvProfGV_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[3].ManualAdjHours = ParseDecimal(BaPvProfGV_Adj?.Text);
        // Row 4: Prof PC
        BaValidationItems[4].SimpleCount = BaPvProfPC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[4].ModerateCount = BaPvProfPC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[4].ComplexCount = BaPvProfPC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[4].VeryComplexCount = BaPvProfPC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[4].ManualAdjHours = ParseDecimal(BaPvProfPC_Adj?.Text);
        // Row 5: Prof RC
        BaValidationItems[5].SimpleCount = BaPvProfRC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[5].ModerateCount = BaPvProfRC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[5].ComplexCount = BaPvProfRC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[5].VeryComplexCount = BaPvProfRC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[5].ManualAdjHours = ParseDecimal(BaPvProfRC_Adj?.Text);
        // Row 6: Exp GV
        BaValidationItems[6].SimpleCount = BaPvExpGV_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[6].ModerateCount = BaPvExpGV_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[6].ComplexCount = BaPvExpGV_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[6].VeryComplexCount = BaPvExpGV_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[6].ManualAdjHours = ParseDecimal(BaPvExpGV_Adj?.Text);
        // Row 7: Exp PC
        BaValidationItems[7].SimpleCount = BaPvExpPC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[7].ModerateCount = BaPvExpPC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[7].ComplexCount = BaPvExpPC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[7].VeryComplexCount = BaPvExpPC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[7].ManualAdjHours = ParseDecimal(BaPvExpPC_Adj?.Text);
        // Row 8: Exp RC
        BaValidationItems[8].SimpleCount = BaPvExpRC_Simple?.IsChecked == true ? 1 : 0;
        BaValidationItems[8].ModerateCount = BaPvExpRC_Moderate?.IsChecked == true ? 1 : 0;
        BaValidationItems[8].ComplexCount = BaPvExpRC_Complex?.IsChecked == true ? 1 : 0;
        BaValidationItems[8].VeryComplexCount = BaPvExpRC_VComplex?.IsChecked == true ? 1 : 0;
        BaValidationItems[8].ManualAdjHours = ParseDecimal(BaPvExpRC_Adj?.Text);
    }

    /// <summary>Update all card display labels from model calculated values.</summary>
    private void UpdateBaCardDisplays()
    {
        // New to Area
        if (BaNewUR_CT != null) BaNewUR_CT.Text = $"CT: {BaTestCases[0].Total:N2}";
        if (BaNewUR_AdjExp != null) BaNewUR_AdjExp.Text = $"Adj: {BaTestCases[0].AdjustedExpLevel:N2}";
        if (BaNewUR_Total != null) BaNewUR_Total.Text = $"Total: {BaTestCases[0].GrandTotal:N2} hrs";

        if (BaNewWTC_CT != null) BaNewWTC_CT.Text = $"CT: {BaTestCases[1].Total:N2}";
        if (BaNewWTC_AdjExp != null) BaNewWTC_AdjExp.Text = $"Adj: {BaTestCases[1].AdjustedExpLevel:N2}";
        if (BaNewWTC_Total != null) BaNewWTC_Total.Text = $"Total: {BaTestCases[1].GrandTotal:N2} hrs";

        if (BaNewDP_Calc != null) BaNewDP_Calc.Text = BaTestCases[3].AdjustedExpLevel.ToString("N2");
        if (BaNewDP_Total != null) BaNewDP_Total.Text = BaTestCases[3].GrandTotal.ToString("N2");
        if (BaNewALM_Calc != null) BaNewALM_Calc.Text = BaTestCases[4].AdjustedExpLevel.ToString("N2");
        if (BaNewALM_Total != null) BaNewALM_Total.Text = BaTestCases[4].GrandTotal.ToString("N2");
        if (BaNewSTE_Calc != null) BaNewSTE_Calc.Text = BaTestCases[5].AdjustedExpLevel.ToString("N2");
        if (BaNewSTE_Total != null) BaNewSTE_Total.Text = BaTestCases[5].GrandTotal.ToString("N2");
        if (BaNewPRD_Calc != null) BaNewPRD_Calc.Text = BaTestCases[6].AdjustedExpLevel.ToString("N2");
        if (BaNewPRD_Total != null) BaNewPRD_Total.Text = BaTestCases[6].GrandTotal.ToString("N2");

        decimal newSubtotal = BaTestCases.Take(7).Where(r => !r.IsInfoRow).Sum(r => r.GrandTotal);
        if (BaNewToAreaSubtotal != null) BaNewToAreaSubtotal.Text = $"Subtotal: {newSubtotal:N2} hrs";

        // Proficient
        if (BaProfUR_CT != null) BaProfUR_CT.Text = $"CT: {BaTestCases[7].Total:N2}";
        if (BaProfUR_AdjExp != null) BaProfUR_AdjExp.Text = $"Adj: {BaTestCases[7].AdjustedExpLevel:N2}";
        if (BaProfUR_Total != null) BaProfUR_Total.Text = $"Total: {BaTestCases[7].GrandTotal:N2} hrs";

        if (BaProfWTC_CT != null) BaProfWTC_CT.Text = $"CT: {BaTestCases[8].Total:N2}";
        if (BaProfWTC_AdjExp != null) BaProfWTC_AdjExp.Text = $"Adj: {BaTestCases[8].AdjustedExpLevel:N2}";
        if (BaProfWTC_Total != null) BaProfWTC_Total.Text = $"Total: {BaTestCases[8].GrandTotal:N2} hrs";

        if (BaProfDP_Calc != null) BaProfDP_Calc.Text = BaTestCases[10].AdjustedExpLevel.ToString("N2");
        if (BaProfDP_Total != null) BaProfDP_Total.Text = BaTestCases[10].GrandTotal.ToString("N2");
        if (BaProfALM_Calc != null) BaProfALM_Calc.Text = BaTestCases[11].AdjustedExpLevel.ToString("N2");
        if (BaProfALM_Total != null) BaProfALM_Total.Text = BaTestCases[11].GrandTotal.ToString("N2");
        if (BaProfSTE_Calc != null) BaProfSTE_Calc.Text = BaTestCases[12].AdjustedExpLevel.ToString("N2");
        if (BaProfSTE_Total != null) BaProfSTE_Total.Text = BaTestCases[12].GrandTotal.ToString("N2");
        if (BaProfPRD_Calc != null) BaProfPRD_Calc.Text = BaTestCases[13].AdjustedExpLevel.ToString("N2");
        if (BaProfPRD_Total != null) BaProfPRD_Total.Text = BaTestCases[13].GrandTotal.ToString("N2");

        decimal profSubtotal = BaTestCases.Skip(7).Take(7).Where(r => !r.IsInfoRow).Sum(r => r.GrandTotal);
        if (BaProficientSubtotal != null) BaProficientSubtotal.Text = $"Subtotal: {profSubtotal:N2} hrs";

        // Expert
        if (BaExpUR_CT != null) BaExpUR_CT.Text = $"CT: {BaTestCases[14].Total:N2}";
        if (BaExpUR_AdjExp != null) BaExpUR_AdjExp.Text = $"Adj: {BaTestCases[14].AdjustedExpLevel:N2}";
        if (BaExpUR_Total != null) BaExpUR_Total.Text = $"Total: {BaTestCases[14].GrandTotal:N2} hrs";

        if (BaExpWTC_CT != null) BaExpWTC_CT.Text = $"CT: {BaTestCases[15].Total:N2}";
        if (BaExpWTC_AdjExp != null) BaExpWTC_AdjExp.Text = $"Adj: {BaTestCases[15].AdjustedExpLevel:N2}";
        if (BaExpWTC_Total != null) BaExpWTC_Total.Text = $"Total: {BaTestCases[15].GrandTotal:N2} hrs";

        if (BaExpDP_Calc != null) BaExpDP_Calc.Text = BaTestCases[17].AdjustedExpLevel.ToString("N2");
        if (BaExpDP_Total != null) BaExpDP_Total.Text = BaTestCases[17].GrandTotal.ToString("N2");
        if (BaExpALM_Calc != null) BaExpALM_Calc.Text = BaTestCases[18].AdjustedExpLevel.ToString("N2");
        if (BaExpALM_Total != null) BaExpALM_Total.Text = BaTestCases[18].GrandTotal.ToString("N2");
        if (BaExpSTE_Calc != null) BaExpSTE_Calc.Text = BaTestCases[19].AdjustedExpLevel.ToString("N2");
        if (BaExpSTE_Total != null) BaExpSTE_Total.Text = BaTestCases[19].GrandTotal.ToString("N2");
        if (BaExpPRD_Calc != null) BaExpPRD_Calc.Text = BaTestCases[20].AdjustedExpLevel.ToString("N2");
        if (BaExpPRD_Total != null) BaExpPRD_Total.Text = BaTestCases[20].GrandTotal.ToString("N2");

        decimal expSubtotal = BaTestCases.Skip(14).Take(7).Where(r => !r.IsInfoRow).Sum(r => r.GrandTotal);
        if (BaExpertSubtotal != null) BaExpertSubtotal.Text = $"Subtotal: {expSubtotal:N2} hrs";

        // PV displays
        UpdatePvCardDisplay(BaPvNewGV_Result, BaPvNewGV_Total, BaValidationItems[0]);
        UpdatePvCardDisplay(BaPvNewPC_Result, BaPvNewPC_Total, BaValidationItems[1]);
        UpdatePvCardDisplay(BaPvNewRC_Result, BaPvNewRC_Total, BaValidationItems[2]);
        UpdatePvCardDisplay(BaPvProfGV_Result, BaPvProfGV_Total, BaValidationItems[3]);
        UpdatePvCardDisplay(BaPvProfPC_Result, BaPvProfPC_Total, BaValidationItems[4]);
        UpdatePvCardDisplay(BaPvProfRC_Result, BaPvProfRC_Total, BaValidationItems[5]);
        UpdatePvCardDisplay(BaPvExpGV_Result, BaPvExpGV_Total, BaValidationItems[6]);
        UpdatePvCardDisplay(BaPvExpPC_Result, BaPvExpPC_Total, BaValidationItems[7]);
        UpdatePvCardDisplay(BaPvExpRC_Result, BaPvExpRC_Total, BaValidationItems[8]);

        // PV subtotals per experience level
        decimal pvNewSub = BaValidationItems.Take(3).Sum(r => r.GrandTotal);
        decimal pvProfSub = BaValidationItems.Skip(3).Take(3).Sum(r => r.GrandTotal);
        decimal pvExpSub = BaValidationItems.Skip(6).Take(3).Sum(r => r.GrandTotal);
        if (BaPvNewSubtotal != null) BaPvNewSubtotal.Text = $"{pvNewSub:N2} hrs";
        if (BaPvProfSubtotal != null) BaPvProfSubtotal.Text = $"{pvProfSub:N2} hrs";
        if (BaPvExpSubtotal != null) BaPvExpSubtotal.Text = $"{pvExpSub:N2} hrs";

        // Sticky summary bar
        decimal remainingBdd = ParseDecimal(RemainingBddHoursTextBox?.Text);
        decimal sysDocProdVal = ParseDecimal(SysDocProdValidTextBox?.Text);
        decimal baSysDoc = ParseDecimal(BaSysDocHoursTextBox?.Text);
        decimal commPlan = ParseDecimal(CommPlanHoursTextBox?.Text);
        decimal wtcGT = BaTestCases.Where(r => !r.IsInfoRow).Sum(r => r.GrandTotal) + BaRegressionRows.Sum(r => r.GrandTotal);
        decimal pvGT = BaValidationItems.Sum(r => r.GrandTotal) + sysDocProdVal;

        if (BaStickyBddText != null) BaStickyBddText.Text = $"{remainingBdd:N2} hrs";
        if (BaStickyWtcText != null) BaStickyWtcText.Text = $"{wtcGT:N2} hrs";
        if (BaStickyPvText != null) BaStickyPvText.Text = $"{pvGT:N2} hrs";
        if (BaStickySysDocText != null) BaStickySysDocText.Text = $"{(baSysDoc + commPlan):N2} hrs";
    }

    private static void UpdatePvCardDisplay(System.Windows.Controls.TextBlock? resultTb, System.Windows.Controls.TextBlock? totalTb, BaValidationRow row)
    {
        if (resultTb != null) resultTb.Text = $"{row.AdjustedExpLevel:N2} hrs";
        if (totalTb != null) totalTb.Text = $"Total: {row.GrandTotal:N2}";
    }

    // Stepper button handlers
    private void OnBddDecrement(object sender, RoutedEventArgs e) { AdjustTextBox(RemainingBddHoursTextBox, -1); }
    private void OnBddIncrement(object sender, RoutedEventArgs e) { AdjustTextBox(RemainingBddHoursTextBox, 1); }
    private void OnSysDocPvDecrement(object sender, RoutedEventArgs e) { AdjustTextBox(SysDocProdValidTextBox, -1); }
    private void OnSysDocPvIncrement(object sender, RoutedEventArgs e) { AdjustTextBox(SysDocProdValidTextBox, 1); }
    private void OnSysDocDecrement(object sender, RoutedEventArgs e) { AdjustTextBox(BaSysDocHoursTextBox, -1); }
    private void OnSysDocIncrement(object sender, RoutedEventArgs e) { AdjustTextBox(BaSysDocHoursTextBox, 1); }
    private void OnCommPlanDecrement(object sender, RoutedEventArgs e) { AdjustTextBox(CommPlanHoursTextBox, -1); }
    private void OnCommPlanIncrement(object sender, RoutedEventArgs e) { AdjustTextBox(CommPlanHoursTextBox, 1); }

    private static void AdjustTextBox(System.Windows.Controls.TextBox? tb, decimal delta)
    {
        if (tb == null) return;
        decimal current = decimal.TryParse(tb.Text, out var v) ? v : 0;
        decimal newVal = current + delta;
        tb.Text = newVal.ToString("0.##");
    }

    private void OnCollabFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RecalculateCollaboration();
    }

    private void OnSeAssumptionTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        SeAssumptionCharCount.Text = $"{SeModuleAssumptionsTextBox.Text.Length}/1000";
    }

    private void OnSeAdjustedTextChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        SeAdjustedCharCount.Text = $"{SeAdjustedHrsCommentTextBox.Text.Length}/1000";
    }

    private void OnConsultantCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // Defer recalculation to after the cell edit is committed
        Dispatcher.BeginInvoke(new Action(() =>
        {
            if (IsLoaded) RecalculateCollaboration();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    /// <summary>
    /// Recalculates ALL Collaboration/Quality totals matching Excel formulas exactly.
    /// Excel: G14=B14*MtgHrs*E14, G15=C15*E14*B14, I(row)=G(row)+H(row)
    /// G12=SUM(G14:G21,D23,D31,D35), H12=SUM(H14:H21), I12=SUM(I14:I21,D23,D31,D35)
    /// </summary>
    private void RecalculateCollaboration()
    {
        // Parse all inputs
        decimal wprCount = ParseDecimal(WprCountTextBox);
        decimal wprHrs = ParseDecimal(WprHoursTextBox);
        decimal wprAtt = ParseDecimal(WprAttendeesTextBox);
        decimal wprPrepHrs = ParseDecimal(WprPrepHoursTextBox);
        decimal wprAdj = ParseDecimal(WprAdjustedTextBox);
        decimal wprPrepAdj = ParseDecimal(WprPrepAdjustedTextBox);

        decimal clientCount = ParseDecimal(ClientMtgCountTextBox);
        decimal clientHrs = ParseDecimal(ClientMtgHoursTextBox);
        decimal clientAtt = ParseDecimal(ClientMtgAttendeesTextBox);
        decimal clientPrepHrs = ParseDecimal(ClientMtgPrepHoursTextBox);
        decimal clientAdj = ParseDecimal(ClientMtgAdjustedTextBox);
        decimal clientPrepAdj = ParseDecimal(ClientMtgPrepAdjustedTextBox);

        decimal intCount = ParseDecimal(InternalMtgCountTextBox);
        decimal intHrs = ParseDecimal(InternalMtgHoursTextBox);
        decimal intAtt = ParseDecimal(InternalMtgAttendeesTextBox);
        decimal intPrepHrs = ParseDecimal(InternalMtgPrepHoursTextBox);
        decimal intAdj = ParseDecimal(InternalMtgAdjustedTextBox);
        decimal intPrepAdj = ParseDecimal(InternalMtgPrepAdjustedTextBox);

        decimal detailEst = ParseDecimal(CreateDetailEstHoursTextBox);
        decimal finalEst = ParseDecimal(CreateFinalEstHoursTextBox);
        decimal pmEffort = ParseDecimal(PmEffortHoursTextBox);

        // Meeting Hour Totals (Excel: Count × MtgHrs × Attendees)
        decimal wprMtgTotal = wprCount * wprHrs * wprAtt;
        decimal wprPrepTotal = wprPrepHrs * wprAtt * wprCount;
        decimal clientMtgTotal = clientCount * clientHrs * clientAtt;
        decimal clientPrepTotal = clientPrepHrs * clientAtt * clientCount;
        decimal intMtgTotal = intCount * intHrs * intAtt;
        decimal intPrepTotal = intPrepHrs * intAtt * intCount;

        // Grand Totals per line (Excel: I = G + H)
        decimal wprMtgGrand = wprMtgTotal + wprAdj;
        decimal wprPrepGrand = wprPrepTotal + wprPrepAdj;
        decimal clientMtgGrand = clientMtgTotal + clientAdj;
        decimal clientPrepGrand = clientPrepTotal + clientPrepAdj;
        decimal intMtgGrand = intMtgTotal + intAdj;
        decimal intPrepGrand = intPrepTotal + intPrepAdj;

        // Consultant total
        decimal consultantTotal = Consultants.Sum(c => c.Hours);

        // Estimates total (D31 = D32 + D33)
        decimal estimatesTotal = detailEst + finalEst;

        // Update UI - Hour Totals (G column)
        WprTotalText.Text = wprMtgTotal.ToString("N2");
        WprPrepTotalText.Text = wprPrepTotal.ToString("N2");
        ClientMtgTotalText.Text = clientMtgTotal.ToString("N2");
        ClientMtgPrepTotalText.Text = clientPrepTotal.ToString("N2");
        InternalMtgTotalText.Text = intMtgTotal.ToString("N2");
        InternalMtgPrepTotalText.Text = intPrepTotal.ToString("N2");

        // Update UI - Grand Totals per line (I column)
        WprGrandTotalText.Text = wprMtgGrand.ToString("N2");
        WprPrepGrandTotalText.Text = wprPrepGrand.ToString("N2");
        ClientMtgGrandTotalText.Text = clientMtgGrand.ToString("N2");
        ClientMtgPrepGrandTotalText.Text = clientPrepGrand.ToString("N2");
        InternalMtgGrandTotalText.Text = intMtgGrand.ToString("N2");
        InternalMtgPrepGrandTotalText.Text = intPrepGrand.ToString("N2");

        // Subtotals
        ConsultantTotalText.Text = consultantTotal.ToString("N2");
        EstimatesTotalText.Text = estimatesTotal.ToString("N2");
        PmEffortTotalText.Text = pmEffort.ToString("N2");

        // Overall totals (Excel row 12)
        // G12 = SUM(G14:G21) + D23 + D31 + D35
        decimal hourTotal = wprMtgTotal + wprPrepTotal + clientMtgTotal + clientPrepTotal
                          + intMtgTotal + intPrepTotal + consultantTotal + estimatesTotal + pmEffort;
        // H12 = SUM(H14:H21) - only meeting adjusted hours
        decimal adjTotal = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
        // I12 = SUM(I14:I21) + D23 + D31 + D35
        decimal grandTotal = hourTotal + adjTotal;

        CollabHourTotalText.Text = hourTotal.ToString("N2");
        CollabAdjTotalText.Text = adjTotal.ToString("N2");
        CollabTotalText.Text = grandTotal.ToString("N2");

        // Also update TOP summary (Excel R12 - appears before detail rows)
        CollabHourTotalTopText.Text = hourTotal.ToString("N2");
        CollabAdjTotalTopText.Text = adjTotal.ToString("N2");
        CollabGrandTotalTopText.Text = grandTotal.ToString("N2");
    }

    private static decimal ParseDecimal(TextBox? tb)
    {
        if (tb == null) return 0m;
        return decimal.TryParse(tb.Text, out var v) ? v : 0m;
    }

    private void CommitAllDataGridEdits()
    {
        ComponentTypeGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        ComponentTypeGrid?.CancelEdit();
        BaTestCasesGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        BaTestCasesGrid?.CancelEdit();
        BaRegressionGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        BaRegressionGrid?.CancelEdit();
        BaProductionValidationGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        BaProductionValidationGrid?.CancelEdit();
        ConsultantGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        ConsultantGrid?.CancelEdit();
    }

    private void OnHomeClick(object sender, RoutedEventArgs e)
    {
        var welcome = new WelcomeWindow();
        welcome.WindowStartupLocation = WindowStartupLocation.Manual;
        welcome.Left = Left;
        welcome.Top = Top;
        welcome.Width = Width;
        welcome.Height = Height;
        welcome.WindowState = WindowState;
        welcome.Show();
        Close();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        SaveEstimate();
    }

    private void OnSaveCommand(object sender, ExecutedRoutedEventArgs e)
    {
        SaveEstimate();
    }

    private void OnUndoCommand(object sender, ExecutedRoutedEventArgs e)
    {
        // Undo last edit in the focused DataGrid
        if (Keyboard.FocusedElement is DependencyObject focused)
        {
            var grid = FindParent<DataGrid>(focused);
            grid?.CancelEdit(DataGridEditingUnit.Row);
        }
    }

    private void SaveEstimate()
    {
        CommitAllDataGridEdits();

        // Validation: must have a project loaded from Initial Estimate history
        if (_currentProject == null || string.IsNullOrWhiteSpace(_currentProject.ProjectName))
        {
            MessageBox.Show("Please load a project from Initial Estimate history using the \"📂 Open\" button before saving.",
                "No Project Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new EstimateDbContext();
            db.Database.EnsureCreated();

            var existing = db.Projects.FirstOrDefault(p => p.ProjectId == _currentProject.ProjectId);
            if (existing == null)
            {
                MessageBox.Show("The selected project could not be found in the database.",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Persist SE data
            decimal promotion = decimal.TryParse(PromotionHoursTextBox.Text, out var p) ? p : 0;
            decimal doc = decimal.TryParse(SystemDocHoursTextBox.Text, out var d) ? d : 0;
            decimal devTotal = ComponentRows.Sum(r => r.GrandTotal);
            existing.TotalDevelopmentHours = devTotal + promotion + doc;

            // Persist SE assumptions and adjusted hours
            existing.SeAssumptions = SeModuleAssumptionsTextBox?.Text ?? string.Empty;

            // Persist BA assumptions
            existing.BaAssumptions = BaAssumptionsDetailTextBox?.Text ?? string.Empty;

            // Persist Collaboration assumptions
            existing.CollaborationAssumptions = CollabAssumptionsDetailTextBox?.Text ?? string.Empty;

            // Persist adjusted hours comments (aggregated)
            var adjComments = new System.Text.StringBuilder();
            if (!string.IsNullOrWhiteSpace(SeAdjustedHrsCommentTextBox?.Text))
                adjComments.AppendLine($"SE: {SeAdjustedHrsCommentTextBox.Text}");
            if (!string.IsNullOrWhiteSpace(BaAdjustedHrsCommentTextBox?.Text))
                adjComments.AppendLine($"BA: {BaAdjustedHrsCommentTextBox.Text}");
            if (!string.IsNullOrWhiteSpace(CollabAdjustedHrsCommentTextBox?.Text))
                adjComments.AppendLine($"Collab: {CollabAdjustedHrsCommentTextBox.Text}");
            existing.AdjustedHoursComments = adjComments.ToString().TrimEnd();

            // Persist BA total hours
            decimal remainingBdd = decimal.TryParse(RemainingBddHoursTextBox?.Text, out var rb2) ? rb2 : 0;
            decimal sysDocProdVal = decimal.TryParse(SysDocProdValidTextBox?.Text, out var sdp2) ? sdp2 : 0;
            decimal baSysDoc = decimal.TryParse(BaSysDocHoursTextBox?.Text, out var bsd2) ? bsd2 : 0;
            decimal commPlan = decimal.TryParse(CommPlanHoursTextBox?.Text, out var cp2) ? cp2 : 0;
            decimal baTotal = BaTestCases.Sum(r => r.AdjustedHours) + BaRegressionRows.Sum(r => r.AdjustedHours)
                + BaValidationItems.Sum(r => r.AdjustedHours) + remainingBdd + sysDocProdVal + baSysDoc + commPlan;
            existing.BaAdjustedHours = baTotal;

            // Persist Collaboration hours (full calculation matching Excel)
            decimal wprCount = ParseDecimal(WprCountTextBox);
            decimal wprHrs = ParseDecimal(WprHoursTextBox);
            decimal wprAtt = ParseDecimal(WprAttendeesTextBox);
            decimal wprPrepHrs = ParseDecimal(WprPrepHoursTextBox);
            decimal wprAdj = ParseDecimal(WprAdjustedTextBox);
            decimal wprPrepAdj = ParseDecimal(WprPrepAdjustedTextBox);
            decimal clientCount = ParseDecimal(ClientMtgCountTextBox);
            decimal clientHrs = ParseDecimal(ClientMtgHoursTextBox);
            decimal clientAtt = ParseDecimal(ClientMtgAttendeesTextBox);
            decimal clientPrepHrs = ParseDecimal(ClientMtgPrepHoursTextBox);
            decimal clientAdj = ParseDecimal(ClientMtgAdjustedTextBox);
            decimal clientPrepAdj = ParseDecimal(ClientMtgPrepAdjustedTextBox);
            decimal intCount = ParseDecimal(InternalMtgCountTextBox);
            decimal intHrs = ParseDecimal(InternalMtgHoursTextBox);
            decimal intAtt = ParseDecimal(InternalMtgAttendeesTextBox);
            decimal intPrepHrs = ParseDecimal(InternalMtgPrepHoursTextBox);
            decimal intAdj = ParseDecimal(InternalMtgAdjustedTextBox);
            decimal intPrepAdj = ParseDecimal(InternalMtgPrepAdjustedTextBox);
            decimal detailEst = ParseDecimal(CreateDetailEstHoursTextBox);
            decimal finalEst = ParseDecimal(CreateFinalEstHoursTextBox);
            decimal pmEffort = ParseDecimal(PmEffortHoursTextBox);
            decimal consultantTotal = Consultants.Sum(c => c.Hours);

            decimal meetingHours = (wprCount * wprHrs * wprAtt) + (wprPrepHrs * wprAtt * wprCount)
                                 + (clientCount * clientHrs * clientAtt) + (clientPrepHrs * clientAtt * clientCount)
                                 + (intCount * intHrs * intAtt) + (intPrepHrs * intAtt * intCount);
            decimal meetingAdj = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
            decimal collabTotal = meetingHours + consultantTotal + (detailEst + finalEst) + pmEffort + meetingAdj;
            existing.CollaborationHours = collabTotal;

            // Persist total hours with PM reserve
            decimal pmPct = decimal.TryParse(PmReserveTextBox?.Text, out var pm) ? pm : 5;
            decimal totalBeforeReserve = devTotal + promotion + doc + baTotal + collabTotal;
            existing.GrandTotalHours = totalBeforeReserve + (totalBeforeReserve * pmPct / 100m);

            // Persist Estimated By
            existing.EstimatedBy = EstimatedByTextBox?.Text ?? Environment.UserName;

            // Update metadata
            existing.LastModifiedDate = DateTime.UtcNow;
            existing.VersionNumber++;

            db.SaveChanges();
            MessageBox.Show("Detailed Estimate saved successfully.", "Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving estimate: {ex.Message}", "Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = System.Windows.Media.VisualTreeHelper.GetParent(child);
        while (parent != null && parent is not T)
            parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
        return parent as T;
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }

    private void InitializeSummaryGrids()
    {
        // SE Summary rows (matching Excel rows 44-69)
        SeSummaryRows.Add(new SummaryRow { TaskName = "SE Total", IsTotalRow = true });
        SeSummaryRows.Add(new SummaryRow { TaskName = "PowerBuilder Windows" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Reports" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Programs/DB Stored Procedures" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Database Manipulation (SQL, PL/SQL)" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Support Modules / Jobs" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Database Review" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Webpage (Includes UI, Portal & Intranet)" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "K2 Workflow" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "K2 Smart Form" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Test Automation Suites (UFT)" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "MISC (Server Setup, Webserver)" });
        SeSummaryRows.Add(new SummaryRow { TaskName = "Promotion (Includes Dev, Model Office)" });
        SeSummaryGrid.ItemsSource = SeSummaryRows;

        // BA Summary rows (matching Excel rows 72-113)
        BaSummaryRows.Add(new SummaryRow { TaskName = "BA Total", IsTotalRow = true });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Remaining BDD Work" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Write System Test Cases Summary" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Data Preparation Summary" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Testing and Documentation Summary" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Regression Testing/Documentation" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Perform Production Validation" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "Production Validation Documentation" });
        BaSummaryRows.Add(new SummaryRow { TaskName = "System Documentation / Communication" });
        BaSummaryGrid.ItemsSource = BaSummaryRows;

        // Collaboration Summary rows
        CollabSummaryRows.Add(new SummaryRow { TaskName = "Collaboration_Quality Summary", IsTotalRow = true });
        CollabSummaryGrid.ItemsSource = CollabSummaryRows;
    }

    public void UpdateSummaryTab()
    {
        // Update SE Summary from ComponentRows
        decimal seTotalStraight = 0, seTotalAdjExp = 0, seTotalAdjMisc = 0, seTotalGrand = 0;
        for (int i = 1; i < SeSummaryRows.Count - 1; i++) // Skip total row (0) and Promotion row (last)
        {
            if (i - 1 < ComponentRows.Count)
            {
                var cr = ComponentRows[i - 1];
                SeSummaryRows[i].StraightHours = cr.HoursTotal;
                SeSummaryRows[i].AdjustedExpLevel = cr.AdjustedExpLevel;
                SeSummaryRows[i].AdjustedMisc = cr.AdjustedHrs;
                SeSummaryRows[i].GrandTotal = cr.GrandTotal;
                seTotalStraight += cr.HoursTotal;
                seTotalAdjExp += cr.AdjustedExpLevel;
                seTotalAdjMisc += cr.AdjustedHrs;
                seTotalGrand += cr.GrandTotal;
            }
        }
        // Promotion row
        decimal promotion = decimal.TryParse(PromotionHoursTextBox?.Text, out var pv) ? pv : 0;
        decimal doc = decimal.TryParse(SystemDocHoursTextBox?.Text, out var dv) ? dv : 0;
        SeSummaryRows[^1].GrandTotal = promotion;
        seTotalGrand += promotion + doc;

        SeSummaryRows[0].StraightHours = seTotalStraight;
        SeSummaryRows[0].AdjustedExpLevel = seTotalAdjExp;
        SeSummaryRows[0].AdjustedMisc = seTotalAdjMisc;
        SeSummaryRows[0].GrandTotal = seTotalGrand;
        SeSummaryGrid?.Items.Refresh();

        // Update BA Summary
        decimal remainingBdd = decimal.TryParse(RemainingBddHoursTextBox?.Text, out var rbv) ? rbv : 0;
        decimal sysDocProdVal = decimal.TryParse(SysDocProdValidTextBox?.Text, out var sdv) ? sdv : 0;
        decimal baSysDoc = decimal.TryParse(BaSysDocHoursTextBox?.Text, out var bsdv) ? bsdv : 0;
        decimal commPlan = decimal.TryParse(CommPlanHoursTextBox?.Text, out var cpv) ? cpv : 0;
        if (SysDocCommPlanTotalText != null)
            SysDocCommPlanTotalText.Text = (baSysDoc + commPlan).ToString("N2");
        decimal baTotalStraight = BaTestCases.Sum(r => r.Total) + BaRegressionRows.Sum(r => r.Total) + BaValidationItems.Sum(r => r.Hours);
        decimal baTotalAdj = BaTestCases.Sum(r => r.AdjustedHours) + BaRegressionRows.Sum(r => r.AdjustedHours) + BaValidationItems.Sum(r => r.AdjustedHours) + remainingBdd + sysDocProdVal + baSysDoc + commPlan;
        BaSummaryRows[0].StraightHours = baTotalStraight;
        BaSummaryRows[0].AdjustedExpLevel = baTotalAdj;
        BaSummaryRows[0].GrandTotal = baTotalAdj;
        BaSummaryGrid?.Items.Refresh();

        // WTC Summary: counts from WTC rows only; CT/AdjExp/AdjHrs/GT from all non-Iteration rows
        var wtcRows = BaTestCases.Where(r => r.TaskType == "WriteSystemTestCases").ToList();
        var tcRows  = BaTestCases.Where(r => !r.IsInfoRow).Concat(BaRegressionRows).ToList();
        if (BaTcSummarySimple != null)       BaTcSummarySimple.Text       = wtcRows.Sum(r => r.SimpleCount).ToString("N2");
        if (BaTcSummaryModerate != null)     BaTcSummaryModerate.Text     = wtcRows.Sum(r => r.ModerateCount).ToString("N2");
        if (BaTcSummaryComplex != null)      BaTcSummaryComplex.Text      = wtcRows.Sum(r => r.ComplexCount).ToString("N2");
        if (BaTcSummaryVeryComplex != null)  BaTcSummaryVeryComplex.Text  = wtcRows.Sum(r => r.VeryComplexCount).ToString("N2");
        if (BaTcSummaryCT != null)           BaTcSummaryCT.Text           = tcRows.Sum(r => r.Total).ToString("N2");
        if (BaTcSummaryAdjExp != null)       BaTcSummaryAdjExp.Text       = tcRows.Sum(r => r.AdjustedExpLevel).ToString("N2");
        if (BaTcSummaryAdjHrs != null)       BaTcSummaryAdjHrs.Text       = tcRows.Sum(r => r.ManualAdjHours).ToString("N2");
        if (BaTcSummaryGrandTotal != null)   BaTcSummaryGrandTotal.Text   = tcRows.Sum(r => r.GrandTotal).ToString("N2");

        // Update Perform Production Validation summary row (Gap 3: include SysDocProdVal in Grand Total)
        decimal pvGrandWithSysDoc = BaValidationItems.Sum(r => r.GrandTotal) + sysDocProdVal;
        if (BaPvSummaryCT != null)         BaPvSummaryCT.Text         = BaValidationItems.Sum(r => r.ComplexityTotal).ToString("N2");
        if (BaPvSummaryAdjExp != null)     BaPvSummaryAdjExp.Text     = BaValidationItems.Sum(r => r.AdjustedExpLevel).ToString("N2");
        if (BaPvSummaryAdjHrs != null)     BaPvSummaryAdjHrs.Text     = BaValidationItems.Sum(r => r.ManualAdjHours).ToString("N2");
        if (BaPvSummaryGrandTotal != null) BaPvSummaryGrandTotal.Text = MRound(pvGrandWithSysDoc).ToString("N2");

        // Gap 4: BA Grand Total = MROUND(RemainingBDD + WTCSummary + PVSummary(incl SysDocProdVal) + SysDoc/CommPlan, 0.25)
        decimal wtcGrandTotal = BaTestCases.Where(r => !r.IsInfoRow).Sum(r => r.GrandTotal) + BaRegressionRows.Sum(r => r.GrandTotal);
        decimal baGrandTotal = MRound(remainingBdd + wtcGrandTotal + pvGrandWithSysDoc + baSysDoc + commPlan);
        // BA Grand Total CT = sum of all row Complexity Totals + BDD (matches Excel H10=SUM(C11,H13,H43,C61))
        decimal baGrandTotalCT = remainingBdd + BaTestCases.Sum(r => r.Total) + BaRegressionRows.Sum(r => r.Total) + BaValidationItems.Sum(r => r.ComplexityTotal);
        // BA Grand Total Adjusted Exp Level = sum of all AdjustedExpLevel values
        decimal baGrandTotalAdjExp = BaTestCases.Where(r => !r.IsInfoRow).Sum(r => r.AdjustedExpLevel) + BaRegressionRows.Sum(r => r.AdjustedExpLevel) + BaValidationItems.Sum(r => r.AdjustedExpLevel);
        // BA Grand Total Adjusted Hrs = manual adj hours from BA rows + SysDoc + CommPlan
        // Note: sysDocProdVal and remainingBdd flow directly to Grand Total (col 11 only in Excel) — not Adjusted Hrs (col 10)
        decimal baGrandTotalAdjHrs = BaTestCases.Sum(r => r.ManualAdjHours) + BaRegressionRows.Sum(r => r.ManualAdjHours) + BaValidationItems.Sum(r => r.ManualAdjHours) + baSysDoc + commPlan;
        if (BaGrandTotalCT != null)     BaGrandTotalCT.Text     = baGrandTotalCT.ToString("N2");
        if (BaGrandTotalAdjExp != null)  BaGrandTotalAdjExp.Text  = baGrandTotalAdjExp.ToString("N2");
        if (BaGrandTotalAdjHrs != null)  BaGrandTotalAdjHrs.Text  = baGrandTotalAdjHrs.ToString("N2");
        if (BaGrandTotalText != null) BaGrandTotalText.Text = baGrandTotal.ToString("N2");

        // Update Collaboration Summary - full calculation matching Excel
        decimal wprCount = ParseDecimal(WprCountTextBox);
        decimal wprHrs = ParseDecimal(WprHoursTextBox);
        decimal wprAtt = ParseDecimal(WprAttendeesTextBox);
        decimal wprPrepHrs = ParseDecimal(WprPrepHoursTextBox);
        decimal wprAdj = ParseDecimal(WprAdjustedTextBox);
        decimal wprPrepAdj = ParseDecimal(WprPrepAdjustedTextBox);
        decimal clientCount = ParseDecimal(ClientMtgCountTextBox);
        decimal clientHrs = ParseDecimal(ClientMtgHoursTextBox);
        decimal clientAtt = ParseDecimal(ClientMtgAttendeesTextBox);
        decimal clientPrepHrs = ParseDecimal(ClientMtgPrepHoursTextBox);
        decimal clientAdj = ParseDecimal(ClientMtgAdjustedTextBox);
        decimal clientPrepAdj = ParseDecimal(ClientMtgPrepAdjustedTextBox);
        decimal intCount = ParseDecimal(InternalMtgCountTextBox);
        decimal intHrs = ParseDecimal(InternalMtgHoursTextBox);
        decimal intAtt = ParseDecimal(InternalMtgAttendeesTextBox);
        decimal intPrepHrs = ParseDecimal(InternalMtgPrepHoursTextBox);
        decimal intAdj = ParseDecimal(InternalMtgAdjustedTextBox);
        decimal intPrepAdj = ParseDecimal(InternalMtgPrepAdjustedTextBox);
        decimal detailEst = ParseDecimal(CreateDetailEstHoursTextBox);
        decimal finalEst = ParseDecimal(CreateFinalEstHoursTextBox);
        decimal pmEffort = ParseDecimal(PmEffortHoursTextBox);
        decimal consultantTotal = Consultants.Sum(r => r.Hours);

        decimal meetingHours = (wprCount * wprHrs * wprAtt) + (wprPrepHrs * wprAtt * wprCount)
                             + (clientCount * clientHrs * clientAtt) + (clientPrepHrs * clientAtt * clientCount)
                             + (intCount * intHrs * intAtt) + (intPrepHrs * intAtt * intCount);
        decimal meetingAdj = wprAdj + wprPrepAdj + clientAdj + clientPrepAdj + intAdj + intPrepAdj;
        decimal collabHourTotal = meetingHours + consultantTotal + (detailEst + finalEst) + pmEffort;
        decimal collabTotal = collabHourTotal + meetingAdj;

        CollabSummaryRows[0].StraightHours = collabHourTotal;
        CollabSummaryRows[0].AdjustedMisc = meetingAdj;
        CollabSummaryRows[0].GrandTotal = collabTotal;
        CollabSummaryGrid?.Items.Refresh();

        // Update header KPIs
        SeHoursKpi.Text = seTotalGrand.ToString("N2");
        BaHoursKpi.Text = baTotalAdj.ToString("N2");
        CollabHoursKpi.Text = collabTotal.ToString("N2");

        // Map SE/BA/Collab totals to their respective tab total displays
        SeTotalText.Text = seTotalGrand.ToString("N2");
        CollabTotalText.Text = collabTotal.ToString("N2");

        // Total = Actual Hours + SE + BA + Collab + PM Reserve
        decimal actualHours = decimal.TryParse(ActualHoursTextBox?.Text, out var ah) ? ah : 0;
        decimal grandTotal = seTotalGrand + baTotalAdj + collabTotal;
        decimal pmPct = decimal.TryParse(PmReserveTextBox?.Text, out var pm) ? pm : 5;
        decimal pmHours = grandTotal * (pmPct / 100m);
        PmReserveHoursText.Text = pmHours.ToString("N2");
        decimal totalEstimate = actualHours + grandTotal + pmHours;
        TotalWithReserveText.Text = totalEstimate.ToString("N2");
        TotalHoursKpi.Text = $"{totalEstimate:N2} Hrs";

        // Aggregate Assumptions from each tab
        SummarySeAssumptionText.Text = SeModuleAssumptionsTextBox?.Text ?? string.Empty;
        SummaryBaAssumptionText.Text = BaAssumptionsDetailTextBox?.Text ?? string.Empty;
        SummaryCollabAssumptionText.Text = CollabAssumptionsDetailTextBox?.Text ?? string.Empty;

        // Aggregate Adjusted Hours Comments from each tab
        SummarySeAdjustedText.Text = SeAdjustedHrsCommentTextBox?.Text ?? string.Empty;
        SummaryBaAdjustedText.Text = BaAdjustedHrsCommentTextBox?.Text ?? string.Empty;
        SummaryCollabAdjustedText.Text = CollabAdjustedHrsCommentTextBox?.Text ?? string.Empty;

        // Sync Estimate By from each tab to Summary
        EstimatedByTextBox.Text = SeEstimateByTextBox?.Text ?? string.Empty;
        BaEstimatedByTextBox.Text = BaEstimateByTextBox?.Text ?? string.Empty;
        CollabEstimatedByTextBox.Text = CollabEstimateByTextBox?.Text ?? string.Empty;
    }

    private static string GetComponentDisplayName(ComponentType ct) => ct switch
    {
        ComponentType.PowerBuilderWindows => "PowerBuilder Windows",
        ComponentType.Reports => "Reports",
        ComponentType.ProgramsDBStoredProcs => "Programs / DB Stored Procs",
        ComponentType.SupportModules => "Support Modules",
        ComponentType.DBManipulation => "DB Manipulation",
        ComponentType.DatabaseReview => "Database Review",
        ComponentType.Webpage => "Webpage (UI, Portal & Intranet)",
        ComponentType.K2Workflow => "K2 Workflow",
        ComponentType.K2SmartForm => "K2 Smart Form",
        ComponentType.TestAutomationUFT => "Test Automation (UFT)",
        ComponentType.MISC => "MISC (Model Office, UAT, E2E, Prod)",
        _ => ct.ToString()
    };
}

/// <summary>
/// Row representing a component type in the Development grid.
/// Double-click opens the module entry dialog.
/// </summary>
public class ComponentTypeRow
{
    public int LineNumber { get; set; }
    public ComponentType ComponentType { get; set; }
    public string ComponentTypeName { get; set; } = string.Empty;
    public int SimpleTotal { get; set; }
    public int ModerateTotal { get; set; }
    public int ComplexTotal { get; set; }
    public decimal HoursTotal { get; set; }
    public decimal AdjustedExpLevel { get; set; }
    public decimal AdjustedHrs { get; set; }
    public decimal GrandTotal { get; set; }
    public List<ModuleEntry> ModuleEntries { get; set; } = new();
}

/// <summary>
/// A single module entry within a component type (rows in the detail dialog).
/// </summary>
public class ModuleEntry
{
    public ExperienceLevel ExperienceLevel { get; set; } = ExperienceLevel.Proficient;
    public string AssociatedRequirement { get; set; } = string.Empty;
    public string ModuleName { get; set; } = string.Empty;
    public ComponentStatus ComponentStatus { get; set; } = ComponentStatus.New;
    public int SimpleCount { get; set; }
    public int ModerateCount { get; set; }
    public int ComplexCount { get; set; }
    public decimal ComplexityTotal { get; set; }
    public decimal AdjustedExpLevel { get; set; }
    public decimal AdjustedHrs { get; set; }
    public decimal GrandTotal { get; set; }
}

/// <summary>
/// Row model for the BA System Test Cases grid.
/// </summary>
public class BaTestCaseRow : INotifyPropertyChanged
{
    private ExperienceLevel _experienceLevel = ExperienceLevel.SelectALevel;
    private string _taskName = string.Empty;
    private decimal _simpleCount;
    private decimal _moderateCount;
    private decimal _complexCount;
    private decimal _veryComplexCount;
    private decimal _manualAdjHours;
    // Per-complexity iteration factors (WTC row) and effective WTC×Iteration counts (derived rows)
    private decimal _iterSimple, _iterModerate, _iterComplex, _iterVeryComplex;
    private decimal _effSimple, _effModerate, _effComplex, _effVeryComplex;
    /// <summary>When true (WTC rows): own counts are multiplied by per-complexity Iter* factors.</summary>
    public bool ApplyIteration { get; set; }
    /// <summary>When true (derived rows): Complexity Total uses Eff* counts from WTC × Iteration.</summary>
    public bool UseEffectiveCounts { get; set; }

    public string TaskName
    {
        get => _taskName;
        set
        {
            _taskName = value;
            (TaskType, IsInfoRow) = value switch
            {
                "Understanding Requirements"                 => ("UnderstandingRequirements", false),
                "Write System Test Cases (# cases)"          => ("WriteSystemTestCases",      false),
                "Iteration"                                  => ("Iteration",                 true),
                "Data Preparation"                           => ("DataPreparation",           false),
                "ALM Upload, Linking and Generating Reports" => ("AlmTasks",                  false),
                "Sys Test Execution"                         => ("TestExecution",             false),
                "Pre Release Defects Creation and Retest"    => ("PreReleaseDefects",         false),
                _                                            => (string.Empty,               false)
            };
            OnPropertyChanged();
            OnPropertyChanged(nameof(HelpText));
            RecalculateTotals();
        }
    }
    public BaCategory Category { get; set; }
    public string TaskType { get; set; } = string.Empty;
    /// <summary>When true, this row is informational only (e.g. Iteration). It shows counts but contributes 0 hours.</summary>
    public bool IsInfoRow { get; set; } = false;
    /// <summary>True for rows where Simple/Moderate/Complex/VeryComplex are auto-calculated from test cases (not user-entered).</summary>
    public bool AreCountsAutoCalculated => TaskType is "DataPreparation" or "AlmTasks" or "TestExecution" or "PreReleaseDefects";

    /// <summary>Tooltip text sourced from the Excel BA_Considerations cell comments.</summary>
    public string? HelpText => TaskType switch
    {
        "UnderstandingRequirements" => "Enter hours needed for the Tester in situations where the Tester is not included in the BDD review, or has additional questions about the CO or supporting material. This should be entered as a single value for all the requirements in the CO. A value of \"1\" in one of Simple/Moderate/Complex/Very Complex.",
        "WriteSystemTestCases"      => "Enter the number of test cases that are projected based on the complexity of the work item. Ensure that the number of iterations are accurate based on the environment.\n\nSystem Test Cases, data preparation and documentation will be calculated based on the number of test cases, complexity of the test cases, number of iterations and the experience of the BA.\n\nInclude Regression Test cases",
        "Iteration"                 => "This area allows a BA to identify if multiple iterations are needed to represent multiple passes for things like claim types, EVS methods, TPL, etc. This value is used as a multiplier to calculate total hours for system test cases, data preparation and documentation. A minimum value of 1.00 is required.",
        "DataPreparation"           => "These hours are automatically calculated based on the number of test cases, iterations, complexity and BA experience level.",
        "AlmTasks"                  => "These hours are automatically calculated based on the number of test cases, iterations, complexity and BA experience level.",
        "TestExecution"             => "These hours are automatically calculated based on the number of test cases, iterations, complexity and BA experience level.",
        "PreReleaseDefects"         => "These hours are automatically calculated based on the number of test cases, iterations, complexity and BA experience level.",
        "RegressionTesting"         => "This task has been disabled. Add the number of regression test cases to the task \"Write System Test Cases\".",
        _                           => (string?)null
    };

    public ExperienceLevel ExperienceLevel { get => _experienceLevel; set { _experienceLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExperienceLevelDisplay)); RecalculateTotals(); } }
    public decimal SimpleCount     { get => _simpleCount;     set { _simpleCount = value;     OnPropertyChanged(); RecalculateTotals(); } }
    public decimal ModerateCount   { get => _moderateCount;   set { _moderateCount = value;   OnPropertyChanged(); RecalculateTotals(); } }
    public decimal ComplexCount    { get => _complexCount;    set { _complexCount = value;    OnPropertyChanged(); RecalculateTotals(); } }
    public decimal VeryComplexCount { get => _veryComplexCount; set { _veryComplexCount = value; OnPropertyChanged(); RecalculateTotals(); } }
    public decimal ManualAdjHours { get => _manualAdjHours; set { _manualAdjHours = value; OnPropertyChanged(); RecalculateTotals(); } }

    /// <summary>Display-friendly experience level name (read-only column in grid).</summary>
    public string ExperienceLevelDisplay => ExperienceLevel switch
    {
        ExperienceLevel.NewToArea  => "New to Area",
        ExperienceLevel.Proficient => "Proficient",
        ExperienceLevel.Expert     => "Expert",
        _                          => ExperienceLevel.ToString()
    };

    /// <summary>Complexity Total — weighted hours from counts (same as before).</summary>
    public decimal Total => IsInfoRow ? 0m : CalculateTotal();
    /// <summary>Adjusted Exp Level = Complexity Total × experience multiplier.</summary>
    public decimal AdjustedExpLevel => Total * DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.BA, ExperienceLevel);
    /// <summary>Grand Total = Adjusted Exp Level + Manual Adj Hours. Regression row applies MROUND(,0.25) matching Excel K40.</summary>
    public decimal GrandTotal => TaskType == "RegressionTesting" ? MRound(AdjustedExpLevel + ManualAdjHours) : AdjustedExpLevel + ManualAdjHours;
    /// <summary>Kept for backward compatibility with summary calculations.</summary>
    public decimal AdjustedHours => GrandTotal;

    // Display properties — blank for Iteration (IsInfoRow) rows, formatted value otherwise
    public string TotalDisplay            => IsInfoRow ? "" : Total.ToString("N2");
    public string AdjustedExpLevelDisplay => IsInfoRow ? "" : AdjustedExpLevel.ToString("N2");
    public string GrandTotalDisplay       => IsInfoRow ? "" : GrandTotal.ToString("N2");
    public string ManualAdjHoursDisplay   => IsInfoRow ? "" : ManualAdjHours.ToString("N2");

    /// <summary>Sets iteration factors used by the WTC row. Called by PropagateWtcIteration.</summary>
    public void SetIterationFactors(decimal s, decimal m, decimal c, decimal vc)
    {
        _iterSimple = s; _iterModerate = m; _iterComplex = c; _iterVeryComplex = vc;
        RecalculateTotals();
    }

    /// <summary>Sets effective WTC×Iteration counts for derived rows. Called by PropagateWtcIteration.</summary>
    public void SetEffectiveCounts(decimal s, decimal m, decimal c, decimal vc)
    {
        _effSimple = s; _effModerate = m; _effComplex = c; _effVeryComplex = vc;
        RecalculateTotals();
    }

    private decimal CalculateTotal()
    {
        decimal sn, mn, cn, vn;
        if (UseEffectiveCounts)
        {
            // Derived rows (DataPrep, ALM, SysTest, PreRelease):
            // CT = Σ( WTC_count_i × rate_i × iteration_i ) — WTC×Iteration already pre-computed as Eff* counts
            sn = _effSimple; mn = _effModerate; cn = _effComplex; vn = _effVeryComplex;
        }
        else if (ApplyIteration)
        {
            // WTC row: own counts × per-complexity iteration factor
            sn = SimpleCount * _iterSimple;
            mn = ModerateCount * _iterModerate;
            cn = ComplexCount * _iterComplex;
            vn = VeryComplexCount * _iterVeryComplex;
        }
        else
        {
            // Understanding Requirements, Regression: own counts, no iteration
            sn = SimpleCount; mn = ModerateCount; cn = ComplexCount; vn = VeryComplexCount;
        }
        var s  = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Simple)      * sn;
        var m  = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Moderate)    * mn;
        var c  = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Complex)     * cn;
        var vc = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.VeryComplex) * vn;
        return s + m + c + vc;
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(AdjustedExpLevel));
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(AdjustedHours));
        OnPropertyChanged(nameof(TotalDisplay));
        OnPropertyChanged(nameof(AdjustedExpLevelDisplay));
        OnPropertyChanged(nameof(GrandTotalDisplay));
        OnPropertyChanged(nameof(ManualAdjHoursDisplay));
    }

    private static decimal MRound(decimal value, decimal multiple = 0.25m) =>
        multiple == 0 ? value : Math.Round(value / multiple, MidpointRounding.AwayFromZero) * multiple;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Row model for the BA Production Validation grid.
/// Experience level is fixed per row (not user-selectable).
/// Complexity counts (Simple/Moderate/Complex/VeryComplex) drive the Complexity Total.
/// </summary>
public class BaValidationRow : INotifyPropertyChanged
{
    private ExperienceLevel _experienceLevel = ExperienceLevel.Proficient;
    private int _simpleCount;
    private int _moderateCount;
    private int _complexCount;
    private int _veryComplexCount;
    private decimal _manualAdjHours;

    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;

    /// <summary>Fixed experience level for this row (set at initialisation, not user-editable).</summary>
    public ExperienceLevel ExperienceLevel
    {
        get => _experienceLevel;
        set { _experienceLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(ExperienceLevelDisplay)); RecalculateTotals(); }
    }

    /// <summary>Display-friendly name used in the read-only Experience column.</summary>
    public string ExperienceLevelDisplay => ExperienceLevel switch
    {
        ExperienceLevel.NewToArea => "New to Area",
        ExperienceLevel.Proficient => "Proficient",
        ExperienceLevel.Expert => "Expert",
        _ => ExperienceLevel.ToString()
    };

    /// <summary>Tooltip text sourced from the Excel BA_Considerations cell comments.</summary>
    public string? HelpText => TaskType switch
    {
        "GeneralValidation"  => "Enter an \"x\" in the column that best represents the projected complexity of the validation.",
        "PricingChanges"     => "Enter an \"x\" in the column that best represents the projected complexity of the pricing change validation.",
        "ReferenceChanges"   => "Enter an \"x\" in the column that best represents the projected complexity of the reference change validation.",
        _                    => (string?)null
    };

    public int SimpleCount      { get => _simpleCount;      set { _simpleCount = value;      OnPropertyChanged(); RecalculateTotals(); } }
    public int ModerateCount    { get => _moderateCount;    set { _moderateCount = value;    OnPropertyChanged(); RecalculateTotals(); } }
    public int ComplexCount     { get => _complexCount;     set { _complexCount = value;     OnPropertyChanged(); RecalculateTotals(); } }
    public int VeryComplexCount { get => _veryComplexCount; set { _veryComplexCount = value; OnPropertyChanged(); RecalculateTotals(); } }

    /// <summary>Manual adjustment hours (additional hours beyond the weighted calculation).</summary>
    public decimal ManualAdjHours
    {
        get => _manualAdjHours;
        set { _manualAdjHours = value; OnPropertyChanged(); RecalculateTotals(); }
    }

    /// <summary>Complexity Total = sum of (count × weighted hours) across all complexity levels.</summary>
    public decimal ComplexityTotal =>
        DetailedWeightedValues.GetBaHours(BaCategory.ProductionValidation, TaskType, BaComplexity.Simple)      * SimpleCount +
        DetailedWeightedValues.GetBaHours(BaCategory.ProductionValidation, TaskType, BaComplexity.Moderate)    * ModerateCount +
        DetailedWeightedValues.GetBaHours(BaCategory.ProductionValidation, TaskType, BaComplexity.Complex)     * ComplexCount +
        DetailedWeightedValues.GetBaHours(BaCategory.ProductionValidation, TaskType, BaComplexity.VeryComplex) * VeryComplexCount;

    /// <summary>Adjusted Exp Level = Complexity Total × experience multiplier.</summary>
    public decimal AdjustedExpLevel => ComplexityTotal * DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.BA, ExperienceLevel);
    /// <summary>Grand Total = Adjusted Exp Level + Manual Adj Hours (straight sum per Excel K45-K55). MROUND applied only at PV summary level.</summary>
    public decimal GrandTotal => AdjustedExpLevel + ManualAdjHours;
    /// <summary>Kept for backward compatibility with summary calculations (returns GrandTotal).</summary>
    public decimal AdjustedHours => GrandTotal;
    /// <summary>Kept for backward compatibility (returns ComplexityTotal).</summary>
    public decimal Hours => ComplexityTotal;

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(ComplexityTotal));
        OnPropertyChanged(nameof(AdjustedExpLevel));
        OnPropertyChanged(nameof(GrandTotal));
        OnPropertyChanged(nameof(AdjustedHours));
        OnPropertyChanged(nameof(Hours));
    }

    private static decimal MRound(decimal value, decimal multiple = 0.25m) =>
        multiple == 0 ? value : Math.Round(value / multiple, MidpointRounding.AwayFromZero) * multiple;

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Row model for the Consultant/Mentor effort grid.
/// </summary>
public class ConsultantRow : INotifyPropertyChanged
{
    private string _name = string.Empty;
    private decimal _hours;

    public string Name { get => _name; set { _name = value; OnPropertyChanged(); } }
    public decimal Hours { get => _hours; set { _hours = value; OnPropertyChanged(); } }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Row model for summary grids in the Estimate Summary tab.
/// Matches Excel columns: Task | Straight Hours | Adjusted Exp Level | Adjusted Misc | Grand Total
/// </summary>
public class SummaryRow
{
    public string TaskName { get; set; } = string.Empty;
    public decimal StraightHours { get; set; }
    public decimal AdjustedExpLevel { get; set; }
    public decimal AdjustedMisc { get; set; }
    public decimal GrandTotal { get; set; }
    public bool IsTotalRow { get; set; }
}
