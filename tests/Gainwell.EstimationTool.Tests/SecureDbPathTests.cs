using System.IO;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Tests verifying secure database path, encryption infrastructure,
/// DatabasePathOverride, and EnsureSchema behavior.
/// </summary>
public class SecureDbPathTests : IDisposable
{
    private readonly string? _originalOverride;

    public SecureDbPathTests()
    {
        _originalOverride = EstimateDbContext.DatabasePathOverride;
    }

    public void Dispose()
    {
        EstimateDbContext.DatabasePathOverride = _originalOverride;
    }

    #region Database Path Resolution

    [Fact]
    public void DefaultPath_UsesLocalAppData()
    {
        EstimateDbContext.DatabasePathOverride = null;
        var expectedFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Gainwell", "EstimationTool");
        var expectedPath = Path.Combine(expectedFolder, "estimates.db");

        // Verify the context resolves to LOCALAPPDATA (not app directory)
        using var db = new EstimateDbContext();
        var connStr = db.Database.GetConnectionString()!;
        Assert.Contains(expectedFolder.Replace("\\", "/"), connStr.Replace("\\", "/"),
            StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\'),
            connStr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DefaultPath_DoesNotPointToAppDirectory()
    {
        EstimateDbContext.DatabasePathOverride = null;
        using var db = new EstimateDbContext();
        var connStr = db.Database.GetConnectionString()!;

        // Must NOT contain the test runner's bin directory
        var binDir = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
        Assert.DoesNotContain(binDir, connStr, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DatabasePathOverride_UsesCustomPath()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");
        EstimateDbContext.DatabasePathOverride = tempPath;
        try
        {
            using var db = new EstimateDbContext();
            var connStr = db.Database.GetConnectionString()!;
            Assert.Contains(tempPath, connStr, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void DatabasePathOverride_Null_FallsBackToSecurePath()
    {
        EstimateDbContext.DatabasePathOverride = null;
        using var db = new EstimateDbContext();
        var connStr = db.Database.GetConnectionString()!;
        Assert.Contains("Gainwell", connStr, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("EstimationTool", connStr, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region EnsureSchema

    [Fact]
    public void EnsureSchema_CreatesDatabase()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"schema_test_{Guid.NewGuid():N}.db");
        EstimateDbContext.DatabasePathOverride = tempPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();
            Assert.True(File.Exists(tempPath));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void EnsureSchema_Idempotent_NoExceptionOnSecondCall()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"schema_idem_{Guid.NewGuid():N}.db");
        EstimateDbContext.DatabasePathOverride = tempPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();
            db.EnsureSchema(); // Second call must not throw
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    #endregion

    #region Encryption Infrastructure

    [Fact]
    public void EncryptionPassword_DefaultIsNull()
    {
        // Default state: no encryption configured
        Assert.Null(EstimateDbContext.EncryptionPassword);
    }

    [Fact]
    public void EncryptionPassword_IncludedInConnectionString_WhenSet()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"enc_test_{Guid.NewGuid():N}.db");
        EstimateDbContext.DatabasePathOverride = tempPath;
        var originalPwd = EstimateDbContext.EncryptionPassword;
        EstimateDbContext.EncryptionPassword = "test-key-123";
        try
        {
            using var db = new EstimateDbContext();
            var connStr = db.Database.GetConnectionString()!;
            Assert.Contains("Password=test-key-123", connStr);
        }
        finally
        {
            EstimateDbContext.EncryptionPassword = originalPwd;
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    [Fact]
    public void EncryptionPassword_NotInConnectionString_WhenNull()
    {
        EstimateDbContext.DatabasePathOverride = Path.Combine(Path.GetTempPath(), "null_enc.db");
        var originalPwd = EstimateDbContext.EncryptionPassword;
        EstimateDbContext.EncryptionPassword = null;
        try
        {
            using var db = new EstimateDbContext();
            var connStr = db.Database.GetConnectionString()!;
            Assert.DoesNotContain("Password", connStr, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            EstimateDbContext.EncryptionPassword = originalPwd;
        }
    }

    [Fact]
    public void DbEncryption_GetOrCreateKey_ReturnsNonEmptyString()
    {
        var key = DbEncryption.GetOrCreateKey();
        Assert.False(string.IsNullOrWhiteSpace(key));
    }

    [Fact]
    public void DbEncryption_GetOrCreateKey_ReturnsSameKeyOnSubsequentCalls()
    {
        var key1 = DbEncryption.GetOrCreateKey();
        var key2 = DbEncryption.GetOrCreateKey();
        Assert.Equal(key1, key2);
    }

    [Fact]
    public void DbEncryption_GetOrCreateKey_Returns256BitKey()
    {
        var key = DbEncryption.GetOrCreateKey();
        var bytes = Convert.FromBase64String(key);
        Assert.Equal(32, bytes.Length); // 256 bits = 32 bytes
    }

    #endregion

    #region Full Save/Load with Secure Path

    [Fact]
    public void SaveAndLoad_WorksWithSecurePath()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"saveload_{Guid.NewGuid():N}.db");
        EstimateDbContext.DatabasePathOverride = tempPath;
        try
        {
            // Initialize DB
            using (var db = new EstimateDbContext())
            {
                db.EnsureSchema();
                DatabaseSeeder.Initialize(db);
                WeightedValues.LoadFromDatabase(db);
            }

            // Save a project
            var vm = new Gainwell.EstimationTool.ViewModels.InitialEstimateViewModel();
            vm.ProjectName = "Secure Path Test";
            vm.ChangeOrderId = "23327 002";
            vm.ProjectDescription = "Testing secure DB path";
            vm.EstimatedBy = "Architect";
            vm.ReviewedBy = "Lead";
            vm.AddComponentCommand.Execute(null);
            var row = vm.Components[^1];
            row.ComponentType = Gainwell.EstimationTool.Models.ComponentType.Reports;
            row.Size = Gainwell.EstimationTool.Models.ComponentSize.Medium;
            row.ChangeType = Gainwell.EstimationTool.Models.ChangeType.New;
            row.Count = 2;

            var saveResult = vm.SaveProject();
            Assert.Null(saveResult); // null = success

            // Reload and verify
            var projects = Gainwell.EstimationTool.ViewModels.InitialEstimateViewModel.GetAllProjects();
            var saved = projects.First(p => p.ProjectName == "Secure Path Test");
            Assert.Equal("23327 002", saved.ChangeOrderId);
            Assert.Equal("Testing secure DB path", saved.ProjectDescription);
            Assert.Single(saved.Components);
            Assert.Equal("Reports", saved.Components[0].ComponentType);
            Assert.Equal(2, saved.Components[0].Count);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            if (File.Exists(tempPath)) File.Delete(tempPath);
        }
    }

    #endregion
}
