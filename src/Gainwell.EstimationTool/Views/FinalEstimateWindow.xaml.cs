using System.Windows;
using System.Windows.Input;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Views;

public partial class FinalEstimateWindow : Window
{
    private readonly FinalEstimateViewModel _viewModel;

    public FinalEstimateWindow()
    {
        InitializeComponent();
        _viewModel = new FinalEstimateViewModel();
        DataContext = _viewModel;
    }

    public FinalEstimateWindow(InitialEstimateViewModel source) : this()
    {
        LoadFromSource(source);
    }

    public void LoadFromSource(InitialEstimateViewModel source)
    {
        _viewModel.LoadFromMainViewModel(source);
        UpdateAssumptionsVisibility();
        UpdateCommentsVisibility();
        SysTestMethodText.Text = _viewModel.UseTestCases ? "Test Cases" : "30% Formula";
    }

    private void UpdateAssumptionsVisibility()
    {
        // Show assumptions panel only if at least one has content
        bool hasAssumptions = !string.IsNullOrWhiteSpace(_viewModel.SeAssumptions)
                           || !string.IsNullOrWhiteSpace(_viewModel.BaAssumptions)
                           || !string.IsNullOrWhiteSpace(_viewModel.CollaborationAssumptions)
                           || !string.IsNullOrWhiteSpace(_viewModel.GeneralAssumptions);

        AssumptionsCard.Visibility = hasAssumptions ? Visibility.Visible : Visibility.Collapsed;
        SeAssumptionsPanel.Visibility = string.IsNullOrWhiteSpace(_viewModel.SeAssumptions) ? Visibility.Collapsed : Visibility.Visible;
        BaAssumptionsPanel.Visibility = string.IsNullOrWhiteSpace(_viewModel.BaAssumptions) ? Visibility.Collapsed : Visibility.Visible;
        CollabAssumptionsPanel.Visibility = string.IsNullOrWhiteSpace(_viewModel.CollaborationAssumptions) ? Visibility.Collapsed : Visibility.Visible;
        GeneralAssumptionsPanel.Visibility = string.IsNullOrWhiteSpace(_viewModel.GeneralAssumptions) ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateCommentsVisibility()
    {
        NoCommentsText.Visibility = string.IsNullOrWhiteSpace(_viewModel.AdjustedHoursComments)
            ? Visibility.Visible : Visibility.Collapsed;
    }

    private void OnHomeClick(object sender, RoutedEventArgs e)
    {
        EstimateNavigator.GoHome(this);
    }

    private void OnInitialTabClick(object sender, MouseButtonEventArgs e)
    {
        EstimateNavigator.SwitchToInitialEstimate(this);
    }

    private void OnDetailedTabClick(object sender, MouseButtonEventArgs e)
    {
        EstimateNavigator.SwitchToDetailedEstimate(this);
    }

    private void OnPrintClick(object sender, RoutedEventArgs e)
    {
        var printDialog = new System.Windows.Controls.PrintDialog();
        if (printDialog.ShowDialog() == true)
        {
            printDialog.PrintVisual(this, "Final Estimate - " + _viewModel.ProjectName);
        }
    }

    public FinalEstimateViewModel GetViewModel() => _viewModel;
}
