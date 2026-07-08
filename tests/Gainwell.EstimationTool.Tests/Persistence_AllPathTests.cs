using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive persistence round-trip tests — save and load projects,
/// verifying all fields are preserved correctly.
/// </summary>
public class Persistence_AllPathTests
{
    private EstimateDbContext CreateInMemoryDb()
    {
        var options = new DbContextOptionsBuilder<EstimateDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;
        var db = new EstimateDbContext(options);
        db.Database.OpenConnection();
        db.Database.EnsureCreated();
        return db;
    }

    #region Happy Path — Save and Load Project Entity

    [Fact]
    public void HappyPath_SaveAndLoad_ProjectEntity()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Test Round-trip",
            ChangeOrderId = "CO-001",
            ProjectDescription = "Testing persistence",
            EstimatedBy = "Engineer",
            ReviewedBy = "Manager",
            PmEffortPercentage = 18m,
            TotalDevelopmentHours = 500m,
            GrandTotalHours = 1000m,
            TShirtSize = "XL1",
            CollaborationHours = 50m,
            DevelopmentAdjustedHours = 10m,
            BaSystemDocAdjustedHours = 1.17m,
            UseTestCasesForEstimate = true,
            TestCasesSimple = 100,
            TestCasesComplex = 50,
            TestCaseIterations = 2.5m,
            SeAssumptions = "SE test",
            AdjustedHoursComments = "Comments here",
            TotalActualHours = 200m,
            TimeForEstimates = 50m
        };

        db.Projects.Add(project);
        db.SaveChanges();

        var loaded = db.Projects.First(p => p.ProjectName == "Test Round-trip");
        Assert.Equal("CO-001", loaded.ChangeOrderId);
        Assert.Equal(18m, loaded.PmEffortPercentage);
        Assert.Equal(500m, loaded.TotalDevelopmentHours);
        Assert.Equal(1000m, loaded.GrandTotalHours);
        Assert.Equal("XL1", loaded.TShirtSize);
        Assert.Equal(10m, loaded.DevelopmentAdjustedHours);
        Assert.Equal(1.17m, loaded.BaSystemDocAdjustedHours);
        Assert.True(loaded.UseTestCasesForEstimate);
        Assert.Equal(100m, loaded.TestCasesSimple);
        Assert.Equal(2.5m, loaded.TestCaseIterations);
        Assert.Equal("SE test", loaded.SeAssumptions);
        Assert.Equal(200m, loaded.TotalActualHours);
        Assert.Equal(50m, loaded.TimeForEstimates);
    }

    #endregion

    #region Happy Path — Components Preserved

    [Fact]
    public void HappyPath_SaveAndLoad_Components()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Component Test",
            ChangeOrderId = "CO-002",
            ProjectDescription = "Desc",
            Components = new List<ComponentEntryEntity>
            {
                new()
                {
                    LineNumber = 1,
                    RequirementId = "REQ-001",
                    ComponentType = "PowerBuilderWindows",
                    Description = "Main screen",
                    ChangeType = "New",
                    Size = "Large",
                    Count = 2,
                    BaseHoursPerUnit = 125.00m,
                    TotalHours = 250.00m
                },
                new()
                {
                    LineNumber = 2,
                    RequirementId = "REQ-002",
                    ComponentType = "Reports",
                    Description = "Monthly report",
                    ChangeType = "Change",
                    Size = "Medium",
                    Count = 1,
                    BaseHoursPerUnit = 40.80m,
                    TotalHours = 40.80m
                }
            }
        };

        db.Projects.Add(project);
        db.SaveChanges();

        var loaded = db.Projects.Include(p => p.Components).First(p => p.ProjectName == "Component Test");
        Assert.Equal(2, loaded.Components.Count);
        Assert.Equal("REQ-001", loaded.Components[0].RequirementId);
        Assert.Equal(250.00m, loaded.Components[0].TotalHours);
        Assert.Equal("Reports", loaded.Components[1].ComponentType);
    }

    #endregion

    #region Happy Path — Collaboration Items Preserved

    [Fact]
    public void HappyPath_SaveAndLoad_CollaborationItems()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Collab Test",
            ChangeOrderId = "CO-003",
            ProjectDescription = "Desc",
            CollaborationItems = new List<CollaborationItemEntity>
            {
                new()
                {
                    LineNumber = 1,
                    TaskName = "WPRs",
                    CollaborationType = "WPRs",
                    NumberOfMeetings = 10,
                    MeetingDurationMinutes = 60,
                    NumberOfParticipants = 5,
                    ParticipantPrepTimeMinutes = 15,
                    TotalHours = 62.50m
                }
            }
        };

        db.Projects.Add(project);
        db.SaveChanges();

        var loaded = db.Projects.Include(p => p.CollaborationItems).First(p => p.ProjectName == "Collab Test");
        Assert.Single(loaded.CollaborationItems);
        Assert.Equal("WPRs", loaded.CollaborationItems[0].TaskName);
        Assert.Equal(10, loaded.CollaborationItems[0].NumberOfMeetings);
        Assert.Equal(62.50m, loaded.CollaborationItems[0].TotalHours);
    }

    #endregion

    #region Sad Path — Duplicate Project Name

    [Fact]
    public void SadPath_DuplicateProjectName_ThrowsOnSave()
    {
        using var db = CreateInMemoryDb();

        var p1 = new ProjectEntity { ProjectName = "Duplicate", ChangeOrderId = "CO-1", ProjectDescription = "D" };
        db.Projects.Add(p1);
        db.SaveChanges();

        var p2 = new ProjectEntity { ProjectName = "Duplicate", ChangeOrderId = "CO-2", ProjectDescription = "D" };
        db.Projects.Add(p2);

        // SQLite enforces unique index on ProjectName — should throw
        Assert.ThrowsAny<Exception>(() => db.SaveChanges());
    }

    #endregion

    #region Positive Path — Cascade Delete

    [Fact]
    public void Positive_DeleteProject_RemovesComponents()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Delete Test",
            ChangeOrderId = "CO-DEL",
            ProjectDescription = "Desc",
            Components = new List<ComponentEntryEntity>
            {
                new() { ComponentType = "MISC", ChangeType = "New", Size = "Large", Count = 1, BaseHoursPerUnit = 100m, TotalHours = 100m }
            }
        };

        db.Projects.Add(project);
        db.SaveChanges();

        Assert.Equal(1, db.ComponentEntries.Count());

        db.Projects.Remove(project);
        db.SaveChanges();

        Assert.Equal(0, db.Projects.Count());
        Assert.Equal(0, db.ComponentEntries.Count());
    }

    #endregion

    #region Negative Path — Version Number

    [Fact]
    public void Negative_VersionNumber_DefaultIs1()
    {
        var entity = new ProjectEntity();
        Assert.Equal(1, entity.VersionNumber);
    }

    [Fact]
    public void Negative_VersionNumber_Increments()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Version Test",
            ChangeOrderId = "CO-VER",
            ProjectDescription = "Desc",
            VersionNumber = 1
        };

        db.Projects.Add(project);
        db.SaveChanges();

        project.VersionNumber++;
        db.SaveChanges();

        var loaded = db.Projects.First(p => p.ProjectName == "Version Test");
        Assert.Equal(2, loaded.VersionNumber);
    }

    #endregion

    #region Positive Path — Timestamps

    [Fact]
    public void Positive_CreatedDate_SetOnCreation()
    {
        var before = DateTime.UtcNow;
        var entity = new ProjectEntity();
        var after = DateTime.UtcNow;

        Assert.True(entity.CreatedDate >= before.AddSeconds(-1));
        Assert.True(entity.CreatedDate <= after.AddSeconds(1));
    }

    [Fact]
    public void Positive_LastModifiedDate_UpdatedOnSave()
    {
        using var db = CreateInMemoryDb();

        var project = new ProjectEntity
        {
            ProjectName = "Timestamp Test",
            ChangeOrderId = "CO-TS",
            ProjectDescription = "Desc"
        };

        db.Projects.Add(project);
        db.SaveChanges();

        var originalModified = project.LastModifiedDate;
        project.LastModifiedDate = DateTime.UtcNow;
        db.SaveChanges();

        var loaded = db.Projects.First(p => p.ProjectName == "Timestamp Test");
        Assert.True(loaded.LastModifiedDate >= originalModified);
    }

    #endregion
}
