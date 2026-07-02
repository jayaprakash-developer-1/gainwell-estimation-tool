using System.Windows;
using System.Windows.Controls;
using InitialEstimatePOC.Data;
using InitialEstimatePOC.Models;

namespace InitialEstimatePOC;

public partial class WelcomeWindow : Window
{
    private ProjectEntity? _selectedProject;

    public WelcomeWindow()
    {
        InitializeComponent();
        LoadRecentProjects();
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

    private void CopyWindowPosition(Window target)
    {
        target.WindowStartupLocation = WindowStartupLocation.Manual;
        target.Left = Left;
        target.Top = Top;
        target.Width = Width;
        target.Height = Height;
        target.WindowState = WindowState;
    }

    private void OnInitialEstimateClick(object sender, RoutedEventArgs e)
    {
        var mainWindow = new MainWindow();
        CopyWindowPosition(mainWindow);
        mainWindow.Show();
        if (mainWindow.DataContext is ViewModels.MainViewModel vm)
        {
            if (_selectedProject != null)
            {
                vm.LoadProject(_selectedProject);
            }
            else
            {
                vm.ProjectName = ProjectNameTextBox.Text.Trim();
                vm.ChangeOrderId = ChangeOrderTextBox.Text.Trim();
            }
        }
        Close();
    }

    private void OnDetailedEstimateClick(object sender, RoutedEventArgs e)
    {
        var project = _selectedProject ?? new ProjectEntity
        {
            ProjectName = ProjectNameTextBox.Text.Trim(),
            ChangeOrderId = ChangeOrderTextBox.Text.Trim()
        };
        var detailedWindow = new DetailedEstimateWindow(project);
        CopyWindowPosition(detailedWindow);
        detailedWindow.Show();
        Close();
    }

    private void OnFinalEstimateClick(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Final Estimate is coming soon.", "Not Yet Available", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void OnHistoryClick(object sender, RoutedEventArgs e)
    {
        var historyWindow = new HistoryWindow { Owner = this };
        if (historyWindow.ShowDialog() == true && historyWindow.SelectedProject != null)
        {
            var mainWindow = new MainWindow();
            CopyWindowPosition(mainWindow);
            mainWindow.Show();
            if (mainWindow.DataContext is ViewModels.MainViewModel vm)
            {
                vm.LoadProject(historyWindow.SelectedProject);
            }
            Close();
        }
    }

    private void OnSettingsClick(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow { Owner = this };
        settingsWindow.ShowDialog();
    }

    private void OnRecentProjectSelected(object sender, SelectionChangedEventArgs e)
    {
        if (RecentProjectsList.SelectedItem is ProjectEntity project)
        {
            _selectedProject = project;
            ProjectNameTextBox.Text = project.ProjectName;
            ChangeOrderTextBox.Text = project.ChangeOrderId;
        }
    }
}
