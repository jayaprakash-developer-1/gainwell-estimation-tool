using Microsoft.EntityFrameworkCore;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Tests for DemoDataSeeder — verifies all demo projects are created correctly
/// with proper T-shirt sizes, calculations, and field completeness.
/// </summary>
public class DemoDataSeeder_AllPathTests
{
    private EstimateDbContext CreateInMemoryDb()
    {
        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }

    #region Happy Path — Demo Projects Created

    [Fact]
    public void HappyPath_SeedDemoProjects_Creates12Projects()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);
        Assert.Equal(12, db.Projects.Count());
    }

    [Fact]
    public void HappyPath_SeedDemoProjects_Idempotent()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);
        DemoDataSeeder.SeedDemoProjects(db);
        Assert.Equal(12, db.Projects.Count());
    }

    [Fact]
    public void HappyPath_AllProjectsHaveNames()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.ToList();
        foreach (var p in projects)
        {
            Assert.False(string.IsNullOrWhiteSpace(p.ProjectName));
            Assert.False(string.IsNullOrWhiteSpace(p.ChangeOrderId));
            Assert.False(string.IsNullOrWhiteSpace(p.ProjectDescription));
            Assert.False(string.IsNullOrWhiteSpace(p.EstimatedBy));
            Assert.False(string.IsNullOrWhiteSpace(p.ReviewedBy));
        }
    }

    [Fact]
    public void HappyPath_AllProjectsHaveComponents()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.Include(p => p.Components).ToList();
        foreach (var p in projects)
        {
            Assert.True(p.Components.Count > 0, $"Project '{p.ProjectName}' should have components");
        }
    }

    [Fact]
    public void HappyPath_AllProjectsHaveCollaboration()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.Include(p => p.CollaborationItems).ToList();
        foreach (var p in projects)
        {
            Assert.True(p.CollaborationItems.Count > 0, $"Project '{p.ProjectName}' should have collaboration items");
        }
    }

    #endregion

    #region Positive Path — T-Shirt Size Coverage

    [Fact]
    public void Positive_AllTShirtSizesCovered()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var tshirts = db.Projects.Select(p => p.TShirtSize).Distinct().OrderBy(t => t).ToList();
        Assert.Contains("Small", tshirts);
        Assert.Contains("Medium", tshirts);
        Assert.Contains("Large", tshirts);
    }

    [Fact]
    public void Positive_AllProjectsHavePositiveGrandTotal()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.ToList();
        foreach (var p in projects)
        {
            Assert.True(p.GrandTotalHours > 0, $"Project '{p.ProjectName}' grand total should be > 0");
            Assert.True(p.TotalDevelopmentHours > 0, $"Project '{p.ProjectName}' dev hours should be > 0");
        }
    }

    #endregion

    #region Positive Path — Assumptions Populated

    [Fact]
    public void Positive_AllProjectsHaveAssumptions()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.ToList();
        foreach (var p in projects)
        {
            Assert.False(string.IsNullOrWhiteSpace(p.SeAssumptions), $"Project '{p.ProjectName}' should have SE assumptions");
            Assert.False(string.IsNullOrWhiteSpace(p.BaAssumptions), $"Project '{p.ProjectName}' should have BA assumptions");
            Assert.False(string.IsNullOrWhiteSpace(p.CollaborationAssumptions), $"Project '{p.ProjectName}' should have Collab assumptions");
            Assert.False(string.IsNullOrWhiteSpace(p.GeneralAssumptions), $"Project '{p.ProjectName}' should have General assumptions");
        }
    }

    #endregion

    #region Negative Path — PM Percentage Range

    [Fact]
    public void Negative_PMPercentage_WithinValidRange()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var projects = db.Projects.ToList();
        foreach (var p in projects)
        {
            Assert.True(p.PmEffortPercentage >= 1m && p.PmEffortPercentage <= 20m,
                $"Project '{p.ProjectName}' PM% ({p.PmEffortPercentage}) should be 1-20");
        }
    }

    #endregion

    #region Happy Path — Small Project Verification

    [Fact]
    public void HappyPath_SmallProject_CorrectFields()
    {
        using var db = CreateInMemoryDb();
        DemoDataSeeder.SeedDemoProjects(db);

        var small = db.Projects.Include(p => p.Components).Include(p => p.CollaborationItems)
            .First(p => p.TShirtSize == "Small");
        Assert.True(small.GrandTotalHours < 100);
        Assert.True(small.Components.Count >= 1);
        Assert.True(small.CollaborationItems.Count >= 1);
    }

    #endregion
}
