using System.Windows;
using InitialEstimatePOC.Models;
using InitialEstimatePOC.ViewModels;

namespace InitialEstimatePOC;

/// <summary>
/// Manages smooth tab navigation between estimate windows using Show/Hide
/// instead of creating/closing windows, giving instant tab switching.
/// </summary>
public static class EstimateNavigator
{
    private static MainWindow? _initialWindow;
    private static DetailedEstimateWindow? _detailedWindow;

    // Fixed dimensions for all estimate windows
    public const double WindowWidth = 1500;
    public const double WindowHeight = 1000;

    public static void SwitchToInitialEstimate(Window source)
    {
        if (_initialWindow == null || !_initialWindow.IsLoaded)
        {
            _initialWindow = new MainWindow();
            _initialWindow.Closed += OnWindowClosed;
            ApplyDimensions(_initialWindow, source);
        }
        else
        {
            SyncPosition(source, _initialWindow);
        }

        // Transfer project info from detailed to initial
        if (source is DetailedEstimateWindow detailed)
        {
            var project = detailed.GetCurrentProject();
            if (_initialWindow.DataContext is MainViewModel vm && project != null)
            {
                vm.ProjectName = project.ProjectName;
                vm.ChangeOrderId = project.ChangeOrderId;
                vm.ProjectDescription = project.ProjectDescription;
            }
        }

        _initialWindow.Show();
        _initialWindow.Activate();
        source.Hide();
    }

    public static void SwitchToDetailedEstimate(Window source)
    {
        ProjectEntity? project = null;

        if (source is MainWindow && source.DataContext is MainViewModel vm)
        {
            project = new ProjectEntity
            {
                ProjectName = vm.ProjectName,
                ChangeOrderId = vm.ChangeOrderId,
                ProjectDescription = vm.ProjectDescription,
                EstimatedBy = vm.EstimatedBy,
                ReviewedBy = vm.ReviewedBy
            };
        }

        if (_detailedWindow == null || !_detailedWindow.IsLoaded)
        {
            _detailedWindow = new DetailedEstimateWindow(project);
            _detailedWindow.Closed += OnWindowClosed;
            ApplyDimensions(_detailedWindow, source);
        }
        else
        {
            SyncPosition(source, _detailedWindow);
            if (project != null)
                _detailedWindow.UpdateProjectInfo(project);
        }

        _detailedWindow.Show();
        _detailedWindow.Activate();
        source.Hide();
    }

    public static void RegisterWindow(Window window)
    {
        if (window is MainWindow mw)
            _initialWindow = mw;
        else if (window is DetailedEstimateWindow dw)
            _detailedWindow = dw;

        window.Closed += OnWindowClosed;
    }

    public static void GoHome(Window source)
    {
        var welcome = new WelcomeWindow();
        welcome.WindowStartupLocation = WindowStartupLocation.Manual;
        welcome.Left = source.RestoreBounds.Left + (source.RestoreBounds.Width - 900) / 2;
        welcome.Top = source.RestoreBounds.Top + (source.RestoreBounds.Height - 700) / 2;
        welcome.Show();

        // Close all estimate windows
        CloseAll();
    }

    public static void CloseAll()
    {
        if (_initialWindow != null && _initialWindow.IsLoaded)
        {
            _initialWindow.Closed -= OnWindowClosed;
            _initialWindow.Close();
        }
        if (_detailedWindow != null && _detailedWindow.IsLoaded)
        {
            _detailedWindow.Closed -= OnWindowClosed;
            _detailedWindow.Close();
        }
        _initialWindow = null;
        _detailedWindow = null;
    }

    private static void OnWindowClosed(object? sender, EventArgs e)
    {
        // If a window is closed (via X button), shut down the app
        if (sender is Window w)
            w.Closed -= OnWindowClosed;

        // Close all windows and exit
        if (_initialWindow != null && _initialWindow.IsLoaded && _initialWindow.IsVisible)
            return; // Other window still visible
        if (_detailedWindow != null && _detailedWindow.IsLoaded && _detailedWindow.IsVisible)
            return; // Other window still visible

        _initialWindow = null;
        _detailedWindow = null;
        Application.Current.Shutdown();
    }

    private static void ApplyDimensions(Window target, Window source)
    {
        target.WindowStartupLocation = WindowStartupLocation.Manual;
        // Use RestoreBounds to get the position on the correct monitor even when maximized
        var bounds = source.RestoreBounds;
        target.Left = bounds.Left;
        target.Top = bounds.Top;
        target.Width = bounds.Width;
        target.Height = bounds.Height;
        target.WindowState = source.WindowState;
    }

    private static void SyncPosition(Window source, Window target)
    {
        // Always sync restore bounds so the window lands on the correct monitor
        var bounds = source.RestoreBounds;
        target.Left = bounds.Left;
        target.Top = bounds.Top;
        target.Width = bounds.Width;
        target.Height = bounds.Height;
        target.WindowState = source.WindowState;
    }
}
