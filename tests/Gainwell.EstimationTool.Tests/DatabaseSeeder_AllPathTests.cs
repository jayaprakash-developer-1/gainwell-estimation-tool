using Microsoft.EntityFrameworkCore;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for DatabaseSeeder — verifying initialization,
/// correction logic, and data integrity.
/// </summary>
public class DatabaseSeeder_AllPathTests
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

    #region Happy Path — Initialization

    [Fact]
    public void HappyPath_Initialize_Seeds66Values()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);
        Assert.Equal(66, db.WeightedValues.Count());
    }

    [Fact]
    public void HappyPath_Initialize_AllComponentTypesPresent()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var types = db.WeightedValues.Select(v => v.ComponentType).Distinct().ToList();
        Assert.Contains(ComponentType.PowerBuilderWindows, types);
        Assert.Contains(ComponentType.Reports, types);
        Assert.Contains(ComponentType.ProgramsDBStoredProcs, types);
        Assert.Contains(ComponentType.SupportModules, types);
        Assert.Contains(ComponentType.DBManipulation, types);
        Assert.Contains(ComponentType.DatabaseReview, types);
        Assert.Contains(ComponentType.Webpage, types);
        Assert.Contains(ComponentType.K2Workflow, types);
        Assert.Contains(ComponentType.K2SmartForm, types);
        Assert.Contains(ComponentType.TestAutomationUFT, types);
        Assert.Contains(ComponentType.MISC, types);
    }

    [Fact]
    public void HappyPath_Initialize_EachTypeHas6Combinations()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        foreach (ComponentType type in Enum.GetValues<ComponentType>().Where(t => t != ComponentType.None))
        {
            var count = db.WeightedValues.Count(v => v.ComponentType == type);
            Assert.Equal(6, count); // 3 sizes × 2 change types
        }
    }

    [Fact]
    public void HappyPath_Initialize_CorrectValuesForPB()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var pbSmallNew = db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.PowerBuilderWindows &&
            v.Size == ComponentSize.Small &&
            v.ChangeType == ChangeType.New);
        Assert.Equal(25.00m, pbSmallNew.BaseHours);
    }

    #endregion

    #region Positive Path — Idempotent Seeding

    [Fact]
    public void Positive_Initialize_Idempotent_NoDoubleSeeding()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);
        DatabaseSeeder.Initialize(db);
        Assert.Equal(66, db.WeightedValues.Count());
    }

    #endregion

    #region Positive Path — Precision Corrections

    [Fact]
    public void Positive_Corrections_SupportModulesMediumChange()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var value = db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.SupportModules &&
            v.Size == ComponentSize.Medium &&
            v.ChangeType == ChangeType.Change);
        Assert.Equal(9.6875m, value.BaseHours);
    }

    [Fact]
    public void Positive_Corrections_DBManipulationLargeNew()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var value = db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.DBManipulation &&
            v.Size == ComponentSize.Large &&
            v.ChangeType == ChangeType.New);
        Assert.Equal(31.875m, value.BaseHours);
    }

    [Fact]
    public void Positive_Corrections_ProgramsDBLargeChange()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var value = db.WeightedValues.First(v =>
            v.ComponentType == ComponentType.ProgramsDBStoredProcs &&
            v.Size == ComponentSize.Large &&
            v.ChangeType == ChangeType.Change);
        Assert.Equal(235.525m, value.BaseHours);
    }

    #endregion

    #region Sad Path — Empty Database

    [Fact]
    public void SadPath_EmptyDb_InitializeCreatesValues()
    {
        using var db = CreateInMemoryDb();
        Assert.Equal(0, db.WeightedValues.Count());
        DatabaseSeeder.Initialize(db);
        Assert.Equal(66, db.WeightedValues.Count());
    }

    #endregion

    #region Negative Path — Value Range

    [Fact]
    public void Negative_AllValues_NonNegative()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var negatives = db.WeightedValues.Where(v => v.BaseHours < 0).ToList();
        Assert.Empty(negatives);
    }

    [Fact]
    public void Negative_AllValues_NonZero()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var zeros = db.WeightedValues.Where(v => v.BaseHours == 0).ToList();
        Assert.Empty(zeros);
    }

    [Fact]
    public void Negative_AllValues_HaveModifiedBy()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);

        var noAuthor = db.WeightedValues.Where(v => string.IsNullOrEmpty(v.ModifiedBy)).ToList();
        Assert.Empty(noAuthor);
    }

    #endregion

    #region Positive Path — Load from Database

    [Fact]
    public void Positive_LoadFromDatabase_UpdatesInMemoryCache()
    {
        using var db = CreateInMemoryDb();
        DatabaseSeeder.Initialize(db);
        WeightedValues.LoadFromDatabase(db);

        // Verify cache matches database
        var value = WeightedValues.GetBaseHours(ComponentType.PowerBuilderWindows, ComponentSize.Small, ChangeType.New);
        Assert.Equal(25.00m, value);
    }

    #endregion
}
