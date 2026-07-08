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
    public ObservableCollection<BaValidationRow> BaValidationItems { get; } = new();
    public ObservableCollection<ConsultantRow> Consultants { get; } = new();
    public ObservableCollection<SummaryRow> SeSummaryRows { get; } = new();
    public ObservableCollection<SummaryRow> BaSummaryRows { get; } = new();
    public ObservableCollection<SummaryRow> CollabSummaryRows { get; } = new();

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
            ProjectNameTextBox.Text = _currentProject.ProjectName ?? string.Empty;
            EstimatedByTextBox.Text = _currentProject.EstimatedBy ?? Environment.UserName;
            UpdateTabContext();
        }
        else
        {
            EstimatedByTextBox.Text = Environment.UserName;
        }
    }

    private void UpdateTabContext()
    {
        var co = ChangeOrderTextBox.Text.Trim();
        var name = ProjectNameTextBox.Text.Trim();
        var parts = new System.Collections.Generic.List<string>();
        if (!string.IsNullOrWhiteSpace(co)) parts.Add(co);
        if (!string.IsNullOrWhiteSpace(name)) parts.Add(name);
        parts.Add("Detailed Estimate");
        Title = string.Join(" - ", parts);
    }

    private void OnOpenHistoryClick(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow { Owner = this };
        if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
        {
            _currentProject = historyWindow.SelectedProject;
            ChangeOrderTextBox.Text = _currentProject.ChangeOrderId ?? string.Empty;
            ProjectNameTextBox.Text = _currentProject.ProjectName ?? string.Empty;
            UpdateTabContext();
        }
    }

    public void LoadFromProject(ProjectEntity project)
    {
        _currentProject = project;
        LoadProjectInfo();
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
        // Setup BA Test Cases Grid with default rows
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Understanding Requirements", Category = BaCategory.SystemTesting, TaskType = "UnderstandingRequirements" });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Write System Test Cases (# cases)", Category = BaCategory.SystemTesting, TaskType = "WriteSystemTestCases" });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Data Preparation", Category = BaCategory.SystemTesting, TaskType = "DataPreparation" });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "ALM Upload, Linking and Reports", Category = BaCategory.SystemTesting, TaskType = "AlmTasks" });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Sys Test Execution", Category = BaCategory.SystemTesting, TaskType = "TestExecution" });
        BaTestCases.Add(new BaTestCaseRow { TaskName = "Regression testing/document", Category = BaCategory.SystemTesting, TaskType = "RegressionTesting" });
        BaTestCasesGrid.ItemsSource = BaTestCases;

        // Setup BA Production Validation Grid
        BaValidationItems.Add(new BaValidationRow { TaskName = "General Validation", TaskType = "GeneralValidation" });
        BaValidationItems.Add(new BaValidationRow { TaskName = "Pricing Changes", TaskType = "PricingChanges" });
        BaValidationItems.Add(new BaValidationRow { TaskName = "Reference Changes", TaskType = "ReferenceChanges" });
        BaProductionValidationGrid.ItemsSource = BaValidationItems;

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

    private void OnSummaryFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        UpdateSummaryTab();
    }

    private void OnCollabFieldChanged(object sender, TextChangedEventArgs e)
    {
        if (!IsLoaded) return;
        RecalculateCollaboration();
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
        BaProductionValidationGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        BaProductionValidationGrid?.CancelEdit();
        ConsultantGrid?.CommitEdit(DataGridEditingUnit.Row, true);
        ConsultantGrid?.CancelEdit();
    }

    private void OnHomeClick(object sender, RoutedEventArgs e)
    {
        EstimateNavigator.GoHome(this);
    }

    private void OnInitialTabClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        EstimateNavigator.SwitchToInitialEstimate(this);
    }

    private void OnFinalTabClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        MessageBox.Show("Final Estimate is coming soon.", "Not Yet Available", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public ProjectEntity? GetCurrentProject()
    {
        return _currentProject ?? new ProjectEntity
        {
            ProjectName = ProjectNameTextBox.Text.Trim(),
            ChangeOrderId = ChangeOrderTextBox.Text.Trim()
        };
    }

    public void UpdateProjectInfo(ProjectEntity project)
    {
        _currentProject = project;
        LoadProjectInfo();
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
            decimal baTotal = BaTestCases.Sum(r => r.AdjustedHours) + BaValidationItems.Sum(r => r.AdjustedHours);
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
        decimal baTotalStraight = BaTestCases.Sum(r => r.Total) + BaValidationItems.Sum(r => r.Hours);
        decimal baTotalAdj = BaTestCases.Sum(r => r.AdjustedHours) + BaValidationItems.Sum(r => r.AdjustedHours);
        BaSummaryRows[0].StraightHours = baTotalStraight;
        BaSummaryRows[0].AdjustedExpLevel = baTotalAdj;
        BaSummaryRows[0].GrandTotal = baTotalAdj;
        BaSummaryGrid?.Items.Refresh();

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
        BaTotalText.Text = baTotalAdj.ToString("N2");
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
    private ExperienceLevel _experienceLevel = ExperienceLevel.Proficient;
    private int _simpleCount;
    private int _moderateCount;
    private int _complexCount;
    private int _veryComplexCount;

    public string TaskName { get; set; } = string.Empty;
    public BaCategory Category { get; set; }
    public string TaskType { get; set; } = string.Empty;

    public ExperienceLevel ExperienceLevel { get => _experienceLevel; set { _experienceLevel = value; OnPropertyChanged(); RecalculateTotals(); } }
    public int SimpleCount { get => _simpleCount; set { _simpleCount = value; OnPropertyChanged(); RecalculateTotals(); } }
    public int ModerateCount { get => _moderateCount; set { _moderateCount = value; OnPropertyChanged(); RecalculateTotals(); } }
    public int ComplexCount { get => _complexCount; set { _complexCount = value; OnPropertyChanged(); RecalculateTotals(); } }
    public int VeryComplexCount { get => _veryComplexCount; set { _veryComplexCount = value; OnPropertyChanged(); RecalculateTotals(); } }

    public decimal Total => CalculateTotal();
    public decimal AdjustedHours => Total * DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.BA, ExperienceLevel);

    private decimal CalculateTotal()
    {
        var s = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Simple) * SimpleCount;
        var m = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Moderate) * ModerateCount;
        var c = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.Complex) * ComplexCount;
        var vc = DetailedWeightedValues.GetBaHours(Category, TaskType, BaComplexity.VeryComplex) * VeryComplexCount;
        return s + m + c + vc;
    }

    private void RecalculateTotals()
    {
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(AdjustedHours));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

/// <summary>
/// Row model for the BA Production Validation grid.
/// </summary>
public class BaValidationRow : INotifyPropertyChanged
{
    private ExperienceLevel _experienceLevel = ExperienceLevel.Proficient;
    private decimal _hours;

    public string TaskName { get; set; } = string.Empty;
    public string TaskType { get; set; } = string.Empty;

    public ExperienceLevel ExperienceLevel { get => _experienceLevel; set { _experienceLevel = value; OnPropertyChanged(); OnPropertyChanged(nameof(AdjustedHours)); } }
    public decimal Hours { get => _hours; set { _hours = value; OnPropertyChanged(); OnPropertyChanged(nameof(AdjustedHours)); } }
    public decimal AdjustedHours => Hours * DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.BA, ExperienceLevel);

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
