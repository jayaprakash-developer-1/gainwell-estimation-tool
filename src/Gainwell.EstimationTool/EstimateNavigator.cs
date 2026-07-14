using System.Windows;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;
using Gainwell.EstimationTool.Views;

namespace Gainwell.EstimationTool;

/// <summary>
/// Manages smooth tab navigation between estimate windows using Show/Hide
/// instead of creating/closing windows, giving instant tab switching.
/// </summary>
public static class EstimateNavigator
{
    private static InitialEstimateWindow? _initialWindow;
    private static DetailedEstimateWindow? _detailedWindow;
    private static FinalEstimateWindow? _finalWindow;

    // Fixed dimensions for all estimate windows
    public const double WindowWidth = 1500;
    public const double WindowHeight = 1000;

    public static void SwitchToInitialEstimate(Window source)
    {
        if (_initialWindow == null || !_initialWindow.IsLoaded)
        {
            _initialWindow = new InitialEstimateWindow();
            _initialWindow.Closed += OnWindowClosed;
            ApplyDimensions(_initialWindow, source);
        }
        else
        {
            SyncPosition(source, _initialWindow);
        }

        // Transfer project info from detailed to initial
        if (source is DetailedEstimateWindow detailed && detailed.GetCurrentProjectEntity() is ProjectEntity project)
        {
            if (_initialWindow.DataContext is InitialEstimateViewModel vm)
            {
                vm.ProjectName = project.ProjectName;
                vm.ChangeOrderId = project.ChangeOrderId;
                vm.ProjectDescription = project.ProjectDescription;
                if (!string.IsNullOrWhiteSpace(project.EstimatedBy))
                    vm.EstimatedBy = project.EstimatedBy;
                if (!string.IsNullOrWhiteSpace(project.ReviewedBy))
                    vm.ReviewedBy = project.ReviewedBy;
            }
        }

        _initialWindow.Show();
        _initialWindow.Activate();
        source.Hide();
    }

    public static void SwitchToDetailedEstimate(Window source)
    {
        ProjectEntity? project = null;

        if (source is InitialEstimateWindow && source.DataContext is InitialEstimateViewModel vm)
        {
            // Look up the SAVED project from DB to get the correct ProjectId
            if (!string.IsNullOrWhiteSpace(vm.ProjectName))
            {
                using var db = new EstimateDbContext();
                db.EnsureSchema();
                project = db.Projects.FirstOrDefault(p => p.ProjectName == vm.ProjectName);
            }

            // If not found in DB (project not yet saved), create a temporary entity
            if (project == null)
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
                _detailedWindow.LoadFromProject(project);
        }

        _detailedWindow.Show();
        _detailedWindow.Activate();
        source.Hide();
    }

    public static void SwitchToFinalEstimate(Window source)
    {
        InitialEstimateViewModel? mainVm = null;

        if (source is InitialEstimateWindow mw && mw.DataContext is InitialEstimateViewModel vm)
            mainVm = vm;

        if (_finalWindow == null || !_finalWindow.IsLoaded)
        {
            _finalWindow = new FinalEstimateWindow();
            _finalWindow.Closed += OnWindowClosed;
            ApplyDimensions(_finalWindow, source);
        }
        else
        {
            SyncPosition(source, _finalWindow);
        }

        if (mainVm != null)
            _finalWindow.LoadFromSource(mainVm);

        _finalWindow.Show();
        _finalWindow.Activate();
        source.Hide();
    }

    public static void RegisterWindow(Window window)
    {
        if (window is InitialEstimateWindow mw)
            _initialWindow = mw;
        else if (window is DetailedEstimateWindow dw)
            _detailedWindow = dw;
        else if (window is FinalEstimateWindow fw)
            _finalWindow = fw;

        window.Closed += OnWindowClosed;
    }

    public static void GoHome(Window source)
    {
        var welcome = new WelcomeWindow();
        welcome.WindowStartupLocation = WindowStartupLocation.Manual;

        if (source.WindowState == WindowState.Maximized)
        {
            // Keep maximized — set restore bounds for if user un-maximizes later
            var bounds = source.RestoreBounds;
            welcome.Left = bounds.Left;
            welcome.Top = bounds.Top;
            welcome.Width = bounds.Width;
            welcome.Height = bounds.Height;
            welcome.WindowState = WindowState.Maximized;
        }
        else
        {
            // Normal state — center on same position
            var bounds = source.RestoreBounds;
            welcome.Left = bounds.Left;
            welcome.Top = bounds.Top;
            welcome.Width = bounds.Width;
            welcome.Height = bounds.Height;
        }

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
        if (_finalWindow != null && _finalWindow.IsLoaded)
        {
            _finalWindow.Closed -= OnWindowClosed;
            _finalWindow.Close();
        }
        _initialWindow = null;
        _detailedWindow = null;
        _finalWindow = null;
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
        if (_finalWindow != null && _finalWindow.IsLoaded && _finalWindow.IsVisible)
            return; // Other window still visible

        _initialWindow = null;
        _detailedWindow = null;
        _finalWindow = null;
        Application.Current.Shutdown();
    }

    private static void ApplyDimensions(Window target, Window source)
    {
        target.WindowStartupLocation = WindowStartupLocation.Manual;
        var bounds = source.RestoreBounds;
        target.Left = bounds.Left;
        target.Top = bounds.Top;
        target.Width = bounds.Width;
        target.Height = bounds.Height;
        // Set WindowState LAST — prevents WPF from de-maximizing when bounds are set
        target.WindowState = source.WindowState;
    }

    private static void SyncPosition(Window source, Window target)
    {
        var state = source.WindowState;
        var bounds = source.RestoreBounds;

        // If maximized, only sync the WindowState — don't change Width/Height
        // which would cause WPF to briefly de-maximize then re-maximize (flicker)
        if (state == WindowState.Maximized)
        {
            target.WindowState = WindowState.Maximized;
        }
        else
        {
            target.WindowState = WindowState.Normal;
            target.Left = bounds.Left;
            target.Top = bounds.Top;
            target.Width = bounds.Width;
            target.Height = bounds.Height;
        }
    }
}
