using System.Windows;
using System.IO;
using System.Globalization;
using System.Threading;
using Gainwell.EstimationTool.Data;

namespace Gainwell.EstimationTool;

public partial class App : Application
{
    private static readonly string AppDataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Gainwell", "EstimationTool");

    protected override void OnStartup(StartupEventArgs e)
    {
        // Force US date/number formatting app-wide (MM/dd/yyyy)
        var usCulture = new CultureInfo("en-US");
        Thread.CurrentThread.CurrentCulture = usCulture;
        Thread.CurrentThread.CurrentUICulture = usCulture;
        CultureInfo.DefaultThreadCurrentCulture = usCulture;
        CultureInfo.DefaultThreadCurrentUICulture = usCulture;

        base.OnStartup(e);

        // Ensure secure app data directory exists
        Directory.CreateDirectory(AppDataFolder);

        DispatcherUnhandledException += (s, args) =>
        {
            try
            {
                var logPath = Path.Combine(AppDataFolder, "crash.log");
                var sanitized = SanitizeCrashLog(args.Exception);
                File.AppendAllText(logPath, sanitized + Environment.NewLine);
            }
            catch { /* Never throw from an exception handler */ }
            MessageBox.Show(args.Exception.Message, "Application Error");
            args.Handled = true;
        };

        // Migrate old database from app directory to secure location
        MigrateOldDatabase();

        // Initialize SQLite database and seed weighted values
        using var db = new EstimateDbContext();
        Gainwell.EstimationTool.Data.DatabaseSeeder.Initialize(db);
        WeightedValues.LoadFromDatabase(db);

        // Initialize detailed estimate weighted values
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        // Seed demo projects for presentation
        DemoDataSeeder.SeedDemoProjects(db);
    }

    private static void MigrateOldDatabase()
    {
        var oldPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "estimates.db");
        var newPath = Path.Combine(AppDataFolder, "estimates.db");
        if (File.Exists(oldPath) && !File.Exists(newPath))
        {
            try { File.Move(oldPath, newPath); }
            catch { /* Old path may be read-only or locked */ }
        }
    }

    private static string SanitizeCrashLog(Exception ex)
    {
        var message = ex.ToString();
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (!string.IsNullOrEmpty(userProfile))
            message = message.Replace(userProfile, "[USERPROFILE]");
        return $"[{DateTime.UtcNow:O}] {message}";
    }
}
