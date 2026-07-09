using System.Windows;
using System.Windows.Controls;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool;

public partial class WelcomeWindow : Window
{
    private ProjectEntity? _selectedProject;

    public WelcomeWindow()
    {
        InitializeComponent();
        Closed += OnWelcomeClosed;
        LoadRecentProjects();
    }

    private void OnWelcomeClosed(object? sender, EventArgs e)
    {
        // If no other windows are open, shut down the app
        foreach (Window w in Application.Current.Windows)
        {
            if (w != this && w.IsVisible)
                return;
        }
        Application.Current.Shutdown();
    }

    private void LoadRecentProjects()
    {
        using var db = new EstimateDbContext();
        var projects = db.Projects
            .OrderByDescending(p => p.LastModifiedDate)
            .Take(10)
            .ToList();

        if (projects.Count == 0)
        {
            NoProjectsText.Visibility = Visibility.Visible;
            RecentProjectsList.Visibility = Visibility.Collapsed;
        }
        else
        {
            NoProjectsText.Visibility = Visibility.Collapsed;
            RecentProjectsList.Visibility = Visibility.Visible;
            RecentProjectsList.ItemsSource = projects;
        }
    }

    private void OnCreateClick(object sender, RoutedEventArgs e)
    {
        var projectName = ProjectNameTextBox.Text.Trim();
        var changeOrder = ChangeOrderTextBox.Text.Trim();
        var description = DescriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Please enter a Project Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ProjectNameTextBox.Focus();
            return;
        }

        var mainWindow = new MainWindow();
        mainWindow.Width = EstimateNavigator.WindowWidth;
        mainWindow.Height = EstimateNavigator.WindowHeight;
        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        EstimateNavigator.RegisterWindow(mainWindow);
        mainWindow.Show();
        if (mainWindow.DataContext is ViewModels.MainViewModel vm)
        {
            vm.ProjectName = projectName;
            vm.ChangeOrderId = changeOrder;
            vm.ProjectDescription = description;
        }
        Close();
    }

    private void OnOpenClick(object sender, RoutedEventArgs e)
    {
        if (_selectedProject == null)
        {
            var historyWindow = new HistoryWindow { Owner = this };
            if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
            {
                _selectedProject = historyWindow.SelectedProject;
                OpenSelectedProject();
            }
            return;
        }

        OpenSelectedProject();
    }

    private void OpenSelectedProject()
    {
        if (_selectedProject == null) return;

        var mainWindow = new MainWindow();
        mainWindow.Width = EstimateNavigator.WindowWidth;
        mainWindow.Height = EstimateNavigator.WindowHeight;
        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        EstimateNavigator.RegisterWindow(mainWindow);
        mainWindow.Show();
        if (mainWindow.DataContext is ViewModels.MainViewModel vm)
        {
            vm.LoadProject(_selectedProject);
        }
        Close();
    }

    private void OnRecentProjectSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RecentProjectsList.SelectedItem is ProjectEntity project)
        {
            _selectedProject = project;
            ProjectNameTextBox.Text = project.ProjectName;
            ChangeOrderTextBox.Text = project.ChangeOrderId;
            DescriptionTextBox.Text = project.ProjectDescription ?? string.Empty;
        }
    }

    private void OnInitialEstimateClick(object sender, RoutedEventArgs e)
    {
        var projectName = ProjectNameTextBox.Text.Trim();
        var changeOrder = ChangeOrderTextBox.Text.Trim();
        var description = DescriptionTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Please enter a Project Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ProjectNameTextBox.Focus();
            return;
        }

        var mainWindow = new MainWindow();
        mainWindow.Width = EstimateNavigator.WindowWidth;
        mainWindow.Height = EstimateNavigator.WindowHeight;
        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        EstimateNavigator.RegisterWindow(mainWindow);
        mainWindow.Show();
        if (mainWindow.DataContext is ViewModels.MainViewModel vm)
        {
            vm.ProjectName = projectName;
            vm.ChangeOrderId = changeOrder;
            vm.ProjectDescription = description;
        }
        Close();
    }

    private void OnDetailedEstimateClick(object sender, RoutedEventArgs e)
    {
        var projectName = ProjectNameTextBox.Text.Trim();
        var changeOrder = ChangeOrderTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Please enter a Project Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ProjectNameTextBox.Focus();
            return;
        }

        var project = new ProjectEntity
        {
            ProjectName = projectName,
            ChangeOrderId = changeOrder,
            ProjectDescription = DescriptionTextBox.Text.Trim()
        };

        var detailedWindow = new DetailedEstimateWindow(project);
        detailedWindow.Width = EstimateNavigator.WindowWidth;
        detailedWindow.Height = EstimateNavigator.WindowHeight;
        detailedWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        EstimateNavigator.RegisterWindow(detailedWindow);
        detailedWindow.Show();
        Close();
    }

    private void OnFinalEstimateClick(object sender, RoutedEventArgs e)
    {
        var projectName = ProjectNameTextBox.Text.Trim();
        var changeOrder = ChangeOrderTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(projectName))
        {
            MessageBox.Show("Please enter a Project Name.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ProjectNameTextBox.Focus();
            return;
        }

        if (string.IsNullOrWhiteSpace(changeOrder))
        {
            MessageBox.Show("Please enter a CO / Defect #.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            ChangeOrderTextBox.Focus();
            return;
        }

        var description = DescriptionTextBox.Text.Trim();

        // Launch Final Estimate via the Initial Estimate window (which holds the data)
        var mainWindow = new MainWindow();
        mainWindow.Width = EstimateNavigator.WindowWidth;
        mainWindow.Height = EstimateNavigator.WindowHeight;
        mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        EstimateNavigator.RegisterWindow(mainWindow);

        if (mainWindow.DataContext is MainViewModel vm)
        {
            // If a project was loaded from history, use its full data
            if (_selectedProject != null)
            {
                vm.LoadProject(_selectedProject);
            }
            else
            {
                vm.ProjectName = projectName;
                vm.ChangeOrderId = changeOrder;
                vm.ProjectDescription = description;
            }
        }

        // Switch directly to final estimate tab
        mainWindow.Show();
        EstimateNavigator.SwitchToFinalEstimate(mainWindow);
        Close();
    }

    private void OnHistoryClick(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow { Owner = this };
        if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
        {
            _selectedProject = historyWindow.SelectedProject;
            ProjectNameTextBox.Text = _selectedProject.ProjectName;
            ChangeOrderTextBox.Text = _selectedProject.ChangeOrderId;
            DescriptionTextBox.Text = _selectedProject.ProjectDescription ?? string.Empty;
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }
}
