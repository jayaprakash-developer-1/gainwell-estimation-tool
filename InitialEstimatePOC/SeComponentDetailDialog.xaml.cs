using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC;

/// <summary>
/// Dialog for entering module-level details for a component type.
/// Shows a DataGrid with default 5 rows. Each row has: Experience Level, Module Name,
/// New/Existing, Simple, Moderate, Complex, Complexity Total, Adj Exp Level, Adjusted Hrs, Grand Total.
/// </summary>
public partial class SeComponentDetailDialog : Window
{
    public ComponentType ComponentType { get; }
    public ObservableCollection<ModuleEntryRow> Entries { get; } = new();
    public List<ModuleEntry>? ResultEntries { get; private set; }

    public SeComponentDetailDialog(ComponentType componentType, List<ModuleEntry>? existing = null)
    {
        InitializeComponent();
        ComponentType = componentType;

        ComponentTypeHeader.Text = GetComponentDisplayName(componentType);
        ComponentSubHeader.Text = $"Enter module details — {GetComponentDisplayName(componentType)}";

        // Setup combo box columns
        var expCol = (DataGridComboBoxColumn)ModuleGrid.Columns[0];
        expCol.ItemsSource = new[] { ExperienceLevel.SelectALevel, ExperienceLevel.NewToArea, ExperienceLevel.Proficient, ExperienceLevel.Expert };

        var statusCol = (DataGridComboBoxColumn)ModuleGrid.Columns[2];
        statusCol.ItemsSource = Enum.GetValues<ComponentStatus>();

        // Load existing entries or create default 5 rows
        if (existing != null && existing.Count > 0)
        {
            foreach (var entry in existing)
            {
                Entries.Add(new ModuleEntryRow(componentType)
                {
                    ExperienceLevel = entry.ExperienceLevel,
                    ModuleName = entry.ModuleName,
                    ComponentStatus = entry.ComponentStatus,
                    SimpleCount = entry.SimpleCount,
                    ModerateCount = entry.ModerateCount,
                    ComplexCount = entry.ComplexCount
                });
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
                Entries.Add(new ModuleEntryRow(componentType));
        }

        ModuleGrid.ItemsSource = Entries;
        RecalculateTotals();
    }

    private void OnAddRow(object sender, RoutedEventArgs e)
    {
        Entries.Add(new ModuleEntryRow(ComponentType));
    }

    private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        // Defer recalculation to after the edit is committed
        Dispatcher.BeginInvoke(new Action(() =>
        {
            foreach (var entry in Entries)
                entry.Recalculate();
            RecalculateTotals();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void RecalculateTotals()
    {
        decimal complexitySum = 0, adjExpSum = 0, grandSum = 0;
        foreach (var entry in Entries)
        {
            complexitySum += entry.ComplexityTotal;
            adjExpSum += entry.AdjustedExpLevel;
            grandSum += entry.GrandTotal;
        }
        ComplexityTotalText.Text = complexitySum.ToString("N2");
        AdjExpTotalText.Text = adjExpSum.ToString("N2");
        GrandTotalText.Text = grandSum.ToString("N2");
    }

    private void OnSave(object sender, RoutedEventArgs e)
    {
        ResultEntries = new List<ModuleEntry>();
        foreach (var row in Entries)
        {
            // Only save rows that have some data
            if (row.SimpleCount > 0 || row.ModerateCount > 0 || row.ComplexCount > 0 || !string.IsNullOrWhiteSpace(row.ModuleName))
            {
                row.Recalculate();
                ResultEntries.Add(new ModuleEntry
                {
                    ExperienceLevel = row.ExperienceLevel,
                    ModuleName = row.ModuleName,
                    ComponentStatus = row.ComponentStatus,
                    SimpleCount = row.SimpleCount,
                    ModerateCount = row.ModerateCount,
                    ComplexCount = row.ComplexCount,
                    ComplexityTotal = row.ComplexityTotal,
                    AdjustedExpLevel = row.AdjustedExpLevel,
                    AdjustedHrs = row.AdjustedHrs,
                    GrandTotal = row.GrandTotal
                });
            }
        }
        DialogResult = true;
        Close();
    }

    private void OnCancel(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
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
/// Row in the module entry DataGrid inside the component detail dialog.
/// </summary>
public class ModuleEntryRow : INotifyPropertyChanged
{
    private readonly ComponentType _componentType;
    private ExperienceLevel _experienceLevel = ExperienceLevel.SelectALevel;
    private string _moduleName = string.Empty;
    private ComponentStatus _componentStatus = ComponentStatus.Select;
    private int _simpleCount;
    private int _moderateCount;
    private int _complexCount;

    public ModuleEntryRow(ComponentType componentType)
    {
        _componentType = componentType;
    }

    public ExperienceLevel ExperienceLevel
    {
        get => _experienceLevel;
        set { _experienceLevel = value; OnPropertyChanged(); Recalculate(); }
    }

    public string ModuleName
    {
        get => _moduleName;
        set { _moduleName = value; OnPropertyChanged(); }
    }

    public ComponentStatus ComponentStatus
    {
        get => _componentStatus;
        set { _componentStatus = value; OnPropertyChanged(); Recalculate(); }
    }

    public int SimpleCount
    {
        get => _simpleCount;
        set { _simpleCount = value; OnPropertyChanged(); Recalculate(); }
    }

    public int ModerateCount
    {
        get => _moderateCount;
        set { _moderateCount = value; OnPropertyChanged(); Recalculate(); }
    }

    public int ComplexCount
    {
        get => _complexCount;
        set { _complexCount = value; OnPropertyChanged(); Recalculate(); }
    }

    public decimal ComplexityTotal { get; private set; }
    public decimal AdjustedExpLevel { get; private set; }

    private decimal _adjustedHrs;
    public decimal AdjustedHrs
    {
        get => _adjustedHrs;
        set { _adjustedHrs = value; OnPropertyChanged(); OnPropertyChanged(nameof(GrandTotal)); }
    }

    public decimal GrandTotal => AdjustedExpLevel + AdjustedHrs;

    public void Recalculate()
    {
        var sHrs = DetailedWeightedValues.GetSeTotalHours(_componentType, _componentStatus, Complexity.Simple) * _simpleCount;
        var mHrs = DetailedWeightedValues.GetSeTotalHours(_componentType, _componentStatus, Complexity.Moderate) * _moderateCount;
        var cHrs = DetailedWeightedValues.GetSeTotalHours(_componentType, _componentStatus, Complexity.Complex) * _complexCount;
        ComplexityTotal = sHrs + mHrs + cHrs;

        var multiplier = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, _experienceLevel);
        AdjustedExpLevel = ComplexityTotal * multiplier;

        OnPropertyChanged(nameof(ComplexityTotal));
        OnPropertyChanged(nameof(AdjustedExpLevel));
        OnPropertyChanged(nameof(GrandTotal));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
