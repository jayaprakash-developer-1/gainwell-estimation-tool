using Microsoft.EntityFrameworkCore;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.Views;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Comprehensive tests for ALL critical coverage gaps:
/// 1. DetailedWeightedValues — GetBaHours, GetExperienceMultiplier, GetSeTotalHours, RoundToQuarter, etc.
/// 2. DetailedDatabaseSeeder — seed counts, data correctness
/// 3. DetailedEstimateEntities — property defaults, persistence round-trip
/// 4. BaTestCaseRow / BaValidationRow / ConsultantRow — calculation logic
/// 5. ModuleEntryRow (SeComponentDetailDialog) — Recalculate logic
/// </summary>
public class DetailedEstimate_CriticalGapTests
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

    // ═══════════════════════════════════════════════════════════════
    // SECTION 1: DetailedWeightedValues — GetBaHours
    // ═══════════════════════════════════════════════════════════════

    #region GetBaHours — BDD Creation

    [Theory]
    [InlineData("PowerBuilderWindows", BaComplexity.Simple, 4.5)]
    [InlineData("PowerBuilderWindows", BaComplexity.Moderate, 9)]
    [InlineData("PowerBuilderWindows", BaComplexity.Complex, 13.5)]
    [InlineData("PowerBuilderWindows", BaComplexity.VeryComplex, 18)]
    [InlineData("Reports", BaComplexity.Simple, 2.25)]
    [InlineData("Reports", BaComplexity.Moderate, 4.5)]
    [InlineData("Reports", BaComplexity.Complex, 6.75)]
    [InlineData("ProgramsDBStoredProcs", BaComplexity.Simple, 2)]
    [InlineData("ProgramsDBStoredProcs", BaComplexity.Complex, 6)]
    [InlineData("Webpage", BaComplexity.Simple, 5)]
    [InlineData("K2WorkflowSmartForm", BaComplexity.Simple, 4.5)]
    [InlineData("K2WorkflowSmartForm", BaComplexity.Complex, 13.5)]
    [InlineData("ClaimsEdits", BaComplexity.Simple, 0.5)]
    [InlineData("ClaimsAudits", BaComplexity.Simple, 0.75)]
    [InlineData("Tables", BaComplexity.Simple, 1)]
    [InlineData("Tables", BaComplexity.Moderate, 1.5)]
    public void GetBaHours_BddCreation_ReturnsCorrectValues(string taskType, BaComplexity complexity, decimal expected)
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetBaHours(BaCategory.BddCreation, taskType, complexity);
        Assert.Equal(expected, result);
    }

    #endregion

    #region GetBaHours — System Testing

    [Theory]
    [InlineData("UnderstandingRequirements", BaComplexity.Simple, 1)]
    [InlineData("UnderstandingRequirements", BaComplexity.Moderate, 1.5)]
    [InlineData("UnderstandingRequirements", BaComplexity.Complex, 2.5)]
    [InlineData("UnderstandingRequirements", BaComplexity.VeryComplex, 4)]
    [InlineData("WriteSystemTestCases", BaComplexity.Simple, 0.5)]
    [InlineData("WriteSystemTestCases", BaComplexity.Moderate, 1)]
    [InlineData("WriteSystemTestCases", BaComplexity.Complex, 2.5)]
    [InlineData("WriteSystemTestCases", BaComplexity.VeryComplex, 4)]
    [InlineData("DataPreparation", BaComplexity.Simple, 0.5)]
    [InlineData("DataPreparation", BaComplexity.Moderate, 0.5)]
    [InlineData("DataPreparation", BaComplexity.Complex, 1)]
    [InlineData("AlmTasks", BaComplexity.Simple, 0.25)]
    [InlineData("TestExecution", BaComplexity.Simple, 0.5)]
    [InlineData("TestExecution", BaComplexity.Complex, 3)]
    [InlineData("PreReleaseDefects", BaComplexity.Simple, 0.125)]
    [InlineData("RegressionTesting", BaComplexity.Simple, 0.75)]
    [InlineData("RegressionTesting", BaComplexity.Moderate, 1.25)]
    public void GetBaHours_SystemTesting_ReturnsCorrectValues(string taskType, BaComplexity complexity, decimal expected)
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetBaHours(BaCategory.SystemTesting, taskType, complexity);
        Assert.Equal(expected, result);
    }

    #endregion

    #region GetBaHours — Production Validation

    [Theory]
    [InlineData("GeneralValidation", BaComplexity.Simple, 5)]
    [InlineData("GeneralValidation", BaComplexity.Moderate, 10)]
    [InlineData("GeneralValidation", BaComplexity.Complex, 20)]
    [InlineData("GeneralValidation", BaComplexity.VeryComplex, 40)]
    [InlineData("PricingChanges", BaComplexity.Simple, 5)]
    [InlineData("PricingChanges", BaComplexity.Complex, 20)]
    [InlineData("ReferenceChanges", BaComplexity.Simple, 5)]
    [InlineData("ReferenceChanges", BaComplexity.VeryComplex, 40)]
    public void GetBaHours_ProductionValidation_ReturnsCorrectValues(string taskType, BaComplexity complexity, decimal expected)
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetBaHours(BaCategory.ProductionValidation, taskType, complexity);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetBaHours_InvalidKey_ReturnsZero()
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetBaHours(BaCategory.BddCreation, "NonExistentTask", BaComplexity.Simple);
        Assert.Equal(0m, result);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 2: DetailedWeightedValues — GetExperienceMultiplier
    // ═══════════════════════════════════════════════════════════════

    #region GetExperienceMultiplier

    [Theory]
    [InlineData(EstimateRole.SE, ExperienceLevel.SelectALevel, 0)]
    [InlineData(EstimateRole.SE, ExperienceLevel.NewToArea, 1.25)]
    [InlineData(EstimateRole.SE, ExperienceLevel.Proficient, 1)]
    [InlineData(EstimateRole.SE, ExperienceLevel.Expert, 0.85)]
    [InlineData(EstimateRole.BA, ExperienceLevel.SelectALevel, 0)]
    [InlineData(EstimateRole.BA, ExperienceLevel.NewToArea, 1.25)]
    [InlineData(EstimateRole.BA, ExperienceLevel.Proficient, 1)]
    [InlineData(EstimateRole.BA, ExperienceLevel.Expert, 0.95)]
    public void GetExperienceMultiplier_AllCombinations(EstimateRole role, ExperienceLevel level, decimal expected)
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetExperienceMultiplier(role, level);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetExperienceMultiplier_SE_NewToArea_HigherThanProficient()
    {
        DetailedWeightedValues.ResetToDefaults();
        var newToArea = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.NewToArea);
        var proficient = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Proficient);
        Assert.True(newToArea > proficient);
    }

    [Fact]
    public void GetExperienceMultiplier_SE_Expert_LowerThanProficient()
    {
        DetailedWeightedValues.ResetToDefaults();
        var expert = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Expert);
        var proficient = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Proficient);
        Assert.True(expert < proficient);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 3: DetailedWeightedValues — GetSeTotalHours / RoundToQuarter
    // ═══════════════════════════════════════════════════════════════

    #region GetSeTotalHours

    [Fact]
    public void GetSeTotalHours_PowerBuilder_New_Simple_SumsAllPhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        // PB New Simple: 4+6+1+2.5+2.5+2.5+1+0.5 = 20.0 (rounds to 20.0)
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.PowerBuilderWindows, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(20m, result);
    }

    [Fact]
    public void GetSeTotalHours_PowerBuilder_New_Moderate_SumsAllPhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        // PB New Moderate: 12+18+3+7.5+7.5+7.5+3+1.5 = 60.0
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.PowerBuilderWindows, ComponentStatus.New, Complexity.Moderate);
        Assert.Equal(60m, result);
    }

    [Fact]
    public void GetSeTotalHours_PowerBuilder_New_Complex_SumsAllPhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        // PB New Complex: 20+30+5+12.5+12.5+12.5+5+2.5 = 100.0
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.PowerBuilderWindows, ComponentStatus.New, Complexity.Complex);
        Assert.Equal(100m, result);
    }

    [Fact]
    public void GetSeTotalHours_PowerBuilder_Existing_Simple()
    {
        DetailedWeightedValues.ResetToDefaults();
        // PB Existing Simple: 3.25+5+1+2+2+2+1+0.5 = 16.75
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.PowerBuilderWindows, ComponentStatus.Existing, Complexity.Simple);
        Assert.Equal(16.75m, result);
    }

    [Fact]
    public void GetSeTotalHours_Reports_New_Simple()
    {
        DetailedWeightedValues.ResetToDefaults();
        // Reports New Simple: 4.8+3.6+0.4+0.8+2+0.4+0.8+0.8 = 13.6 → rounds to 13.5
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.Reports, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(13.5m, result);
    }

    [Fact]
    public void GetSeTotalHours_InvalidCombo_ReturnsZero()
    {
        DetailedWeightedValues.ResetToDefaults();
        // ComponentType.None doesn't exist in the matrix
        var result = DetailedWeightedValues.GetSeTotalHours(ComponentType.None, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(0m, result);
    }

    #endregion

    #region RoundToQuarter

    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(1.1, 1)]
    [InlineData(1.125, 1.25)]
    [InlineData(1.13, 1.25)]
    [InlineData(1.25, 1.25)]
    [InlineData(1.37, 1.25)]
    [InlineData(1.375, 1.5)]
    [InlineData(1.5, 1.5)]
    [InlineData(1.625, 1.75)]
    [InlineData(1.75, 1.75)]
    [InlineData(1.875, 2)]
    [InlineData(13.6, 13.5)]
    [InlineData(99.99, 100)]
    public void RoundToQuarter_VariousValues(decimal input, decimal expected)
    {
        var result = DetailedWeightedValues.RoundToQuarter(input);
        Assert.Equal(expected, result);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 4: DetailedWeightedValues — GetSeBreakdownPercentage
    // ═══════════════════════════════════════════════════════════════

    #region GetSeBreakdownPercentage

    [Theory]
    [InlineData("Development", 0.74)]
    [InlineData("Testing", 0.10)]
    [InlineData("Documentation", 0.13)]
    [InlineData("Miscellaneous", 0.03)]
    public void GetSeBreakdownPercentage_KnownCategories(string category, decimal expected)
    {
        DetailedWeightedValues.ResetToDefaults();
        var result = DetailedWeightedValues.GetSeBreakdownPercentage(category);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetSeBreakdownPercentage_SumsTo100Percent()
    {
        DetailedWeightedValues.ResetToDefaults();
        var sum = DetailedWeightedValues.GetSeBreakdownPercentage("Development")
                + DetailedWeightedValues.GetSeBreakdownPercentage("Testing")
                + DetailedWeightedValues.GetSeBreakdownPercentage("Documentation")
                + DetailedWeightedValues.GetSeBreakdownPercentage("Miscellaneous");
        Assert.Equal(1.00m, sum);
    }

    [Fact]
    public void GetSeBreakdownPercentage_UnknownCategory_ReturnsZero()
    {
        var result = DetailedWeightedValues.GetSeBreakdownPercentage("Unknown");
        Assert.Equal(0m, result);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 5: DetailedWeightedValues — GetTaskPhasesForComponent
    // ═══════════════════════════════════════════════════════════════

    #region GetTaskPhasesForComponent

    [Fact]
    public void GetTaskPhasesForComponent_PowerBuilder_Has8Phases()
    {
        DetailedWeightedValues.ResetToDefaults();
        var phases = DetailedWeightedValues.GetTaskPhasesForComponent(ComponentType.PowerBuilderWindows);
        Assert.True(phases.Count >= 8);
        Assert.Contains(SeTaskPhase.Analysis, phases);
        Assert.Contains(SeTaskPhase.ProductionImplementation, phases);
    }

    [Fact]
    public void GetTaskPhasesForComponent_DBManipulation_HasSqlPhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        var phases = DetailedWeightedValues.GetTaskPhasesForComponent(ComponentType.DBManipulation);
        Assert.Contains(SeTaskPhase.SqlDesign, phases);
        Assert.Contains(SeTaskPhase.SqlConstruction, phases);
        Assert.Contains(SeTaskPhase.SqlTesting, phases);
        Assert.Contains(SeTaskPhase.SqlReview, phases);
    }

    [Fact]
    public void GetTaskPhasesForComponent_SupportModules_HasModulePhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        var phases = DetailedWeightedValues.GetTaskPhasesForComponent(ComponentType.SupportModules);
        Assert.Contains(SeTaskPhase.DesignModule, phases);
        Assert.Contains(SeTaskPhase.BuildModule, phases);
        Assert.Contains(SeTaskPhase.TestModule, phases);
        Assert.Contains(SeTaskPhase.ReviewModule, phases);
    }

    [Fact]
    public void GetTaskPhasesForComponent_DatabaseReview_HasReviewPhases()
    {
        DetailedWeightedValues.ResetToDefaults();
        var phases = DetailedWeightedValues.GetTaskPhasesForComponent(ComponentType.DatabaseReview);
        Assert.Contains(SeTaskPhase.Forms, phases);
        Assert.Contains(SeTaskPhase.PreReview, phases);
        Assert.Contains(SeTaskPhase.ReviewMeeting, phases);
        Assert.Contains(SeTaskPhase.PostReview, phases);
        Assert.Contains(SeTaskPhase.DbChange, phases);
        Assert.Contains(SeTaskPhase.Erwin, phases);
    }

    [Fact]
    public void GetTaskPhasesForComponent_NonExistent_ReturnsEmpty()
    {
        DetailedWeightedValues.ResetToDefaults();
        var phases = DetailedWeightedValues.GetTaskPhasesForComponent(ComponentType.None);
        Assert.Empty(phases);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 6: DetailedWeightedValues — Display Name Methods
    // ═══════════════════════════════════════════════════════════════

    #region GetTaskPhaseDisplayName

    [Theory]
    [InlineData(SeTaskPhase.Analysis, "Analysis")]
    [InlineData(SeTaskPhase.GenerateTechnicalDesign, "Generate Technical Design")]
    [InlineData(SeTaskPhase.DesignReviewAndAcceptance, "Design Review and Acceptance")]
    [InlineData(SeTaskPhase.UnitTestCasesScenarios, "Unit Test Cases / Scenarios")]
    [InlineData(SeTaskPhase.CodeConstructionAndUnitTest, "Code Construction and Unit Test")]
    [InlineData(SeTaskPhase.CodeAndUnitTestReviewPreWPR, "Code and Unit Test Review and Acceptance - Pre WPR")]
    [InlineData(SeTaskPhase.UpdateDocumentation, "Update Documentation")]
    [InlineData(SeTaskPhase.ProductionImplementation, "Production Implementation")]
    [InlineData(SeTaskPhase.SqlDesign, "SQL Design")]
    [InlineData(SeTaskPhase.SqlConstruction, "SQL Construction")]
    [InlineData(SeTaskPhase.SqlTesting, "SQL Testing")]
    [InlineData(SeTaskPhase.SqlReview, "SQL Review")]
    [InlineData(SeTaskPhase.DesignModule, "Design Module")]
    [InlineData(SeTaskPhase.BuildModule, "Build Module")]
    [InlineData(SeTaskPhase.TestModule, "Test Module")]
    [InlineData(SeTaskPhase.ReviewModule, "Review Module")]
    [InlineData(SeTaskPhase.Forms, "Forms")]
    [InlineData(SeTaskPhase.PreReview, "Pre-Review")]
    [InlineData(SeTaskPhase.ReviewMeeting, "Review Meeting")]
    [InlineData(SeTaskPhase.PostReview, "Post Review")]
    [InlineData(SeTaskPhase.DbChange, "DB Change")]
    [InlineData(SeTaskPhase.Erwin, "Erwin")]
    [InlineData(SeTaskPhase.TestExecution, "Test Execution")]
    [InlineData(SeTaskPhase.ModelOffice, "Model Office")]
    [InlineData(SeTaskPhase.UAT, "UAT")]
    [InlineData(SeTaskPhase.E2E, "E2E")]
    [InlineData(SeTaskPhase.Production, "Production")]
    public void GetTaskPhaseDisplayName_AllValues(SeTaskPhase phase, string expected)
    {
        Assert.Equal(expected, DetailedWeightedValues.GetTaskPhaseDisplayName(phase));
    }

    #endregion

    #region GetBaTaskTypeDisplayName

    [Theory]
    [InlineData("PowerBuilderWindows", "PowerBuilder Windows")]
    [InlineData("Reports", "Reports")]
    [InlineData("ProgramsDBStoredProcs", "Programs/DB Stored Procedures")]
    [InlineData("Webpage", "Webpage (Includes UI, Portal & Intranet)")]
    [InlineData("K2WorkflowSmartForm", "K2 Workflow-Smart Form")]
    [InlineData("ClaimsEdits", "Claims-Edits")]
    [InlineData("ClaimsAudits", "Claims-Audits")]
    [InlineData("ApplicationFunctions", "Application Functions")]
    [InlineData("EvsRec", "EVS/REC")]
    [InlineData("Extracts", "Extracts")]
    [InlineData("ExternalInterfaces", "External Interfaces")]
    [InlineData("ReferenceUpdates", "Reference Updates")]
    [InlineData("Tables", "Tables")]
    [InlineData("UnderstandingRequirements", "Understanding Requirements")]
    [InlineData("WriteSystemTestCases", "Write System Test Cases (# cases)")]
    [InlineData("DataPreparation", "Data Preparation")]
    [InlineData("AlmTasks", "ALM Upload, Linking and Generating Reports")]
    [InlineData("TestExecution", "Sys Test Execution")]
    [InlineData("RegressionTesting", "Regression testing/document (# cases)")]
    [InlineData("GeneralValidation", "General Validation")]
    [InlineData("PricingChanges", "Pricing Changes")]
    [InlineData("ReferenceChanges", "Reference Changes")]
    [InlineData("UnknownType", "UnknownType")]
    public void GetBaTaskTypeDisplayName_AllValues(string taskType, string expected)
    {
        Assert.Equal(expected, DetailedWeightedValues.GetBaTaskTypeDisplayName(taskType));
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 7: DetailedWeightedValues — LoadFromDatabase / Update
    // ═══════════════════════════════════════════════════════════════

    #region LoadFromDatabase and UpdateValue

    [Fact]
    public void LoadFromDatabase_LoadsSeValues()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        // Verify a known value loaded correctly
        var result = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(4m, result);
    }

    [Fact]
    public void LoadFromDatabase_LoadsBaValues()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        var result = DetailedWeightedValues.GetBaHours(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Simple);
        Assert.Equal(4.5m, result);
    }

    [Fact]
    public void LoadFromDatabase_LoadsExperienceLevels()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        var result = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.NewToArea);
        Assert.Equal(1.25m, result);
    }

    [Fact]
    public void UpdateSeValue_PersistsAndUpdatesCache()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);

        DetailedWeightedValues.UpdateSeValue(db, ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 99m, "Tester");

        // Cache updated
        var result = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(99m, result);

        // DB updated
        var entity = db.DetailedSeWeightedValues.First(v =>
            v.ComponentType == ComponentType.PowerBuilderWindows &&
            v.TaskPhase == SeTaskPhase.Analysis &&
            v.ComponentStatus == ComponentStatus.New &&
            v.Complexity == Complexity.Simple);
        Assert.Equal(99m, entity.Hours);
        Assert.Equal("Tester", entity.ModifiedBy);

        DetailedWeightedValues.ResetToDefaults();
    }

    [Fact]
    public void UpdateBaValue_PersistsAndUpdatesCache()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);

        DetailedWeightedValues.UpdateBaValue(db, BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Simple, 77m, "QA");

        var result = DetailedWeightedValues.GetBaHours(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Simple);
        Assert.Equal(77m, result);

        DetailedWeightedValues.ResetToDefaults();
    }

    [Fact]
    public void UpdateExperienceLevel_PersistsAndUpdatesCache()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);

        DetailedWeightedValues.UpdateExperienceLevel(db, EstimateRole.SE, ExperienceLevel.Expert, 0.5m, "Admin");

        var result = DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Expert);
        Assert.Equal(0.5m, result);

        DetailedWeightedValues.ResetToDefaults();
    }

    [Fact]
    public void ResetToDefaults_RestoresOriginalValues()
    {
        // Corrupt the cache
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.UpdateSeValue(db, ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 999m);

        // Reset
        DetailedWeightedValues.ResetToDefaults();

        var result = DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple);
        Assert.Equal(4m, result);
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 8: DetailedDatabaseSeeder
    // ═══════════════════════════════════════════════════════════════

    #region DetailedDatabaseSeeder

    [Fact]
    public void DetailedDatabaseSeeder_Initialize_CreatesSeWeightedValues()
    {
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        Assert.True(db.DetailedSeWeightedValues.Count() > 0);
    }

    [Fact]
    public void DetailedDatabaseSeeder_Initialize_CreatesBaWeightedValues()
    {
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        Assert.True(db.DetailedBaWeightedValues.Count() > 0);
    }

    [Fact]
    public void DetailedDatabaseSeeder_Initialize_Creates8ExperienceLevels()
    {
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        Assert.Equal(8, db.ExperienceLevels.Count()); // 2 roles × 4 levels
    }

    [Fact]
    public void DetailedDatabaseSeeder_Initialize_IsIdempotent()
    {
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        int countAfterFirst = db.DetailedSeWeightedValues.Count();

        DetailedDatabaseSeeder.Initialize(db);
        int countAfterSecond = db.DetailedSeWeightedValues.Count();

        Assert.Equal(countAfterFirst, countAfterSecond);
    }

    [Fact]
    public void DetailedDatabaseSeeder_SE_MatchesHardcodedDefaults()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        // Verify several key values match hardcoded defaults
        Assert.Equal(4m, DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple));
        Assert.Equal(12m, DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate));
        Assert.Equal(20m, DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex));
        Assert.Equal(3.25m, DetailedWeightedValues.GetSeHours(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple));

        DetailedWeightedValues.ResetToDefaults();
    }

    [Fact]
    public void DetailedDatabaseSeeder_ExperienceLevels_MatchDefaults()
    {
        DetailedWeightedValues.ResetToDefaults();
        using var db = CreateInMemoryDb();
        DetailedDatabaseSeeder.Initialize(db);
        DetailedWeightedValues.LoadFromDatabase(db);

        Assert.Equal(1.25m, DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.NewToArea));
        Assert.Equal(1m, DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Proficient));
        Assert.Equal(0.85m, DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.SE, ExperienceLevel.Expert));
        Assert.Equal(0.95m, DetailedWeightedValues.GetExperienceMultiplier(EstimateRole.BA, ExperienceLevel.Expert));

        DetailedWeightedValues.ResetToDefaults();
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 9: DetailedEstimateEntities — Property Defaults + Round-Trip
    // ═══════════════════════════════════════════════════════════════

    #region Entity Defaults

    [Fact]
    public void DetailedSeComponentEntity_Defaults()
    {
        var e = new DetailedSeComponentEntity();
        Assert.Equal(0, e.Id);
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(0, e.LineNumber);
        Assert.Equal(string.Empty, e.ComponentType);
        Assert.Equal(0, e.SimpleTotal);
        Assert.Equal(0, e.ModerateTotal);
        Assert.Equal(0, e.ComplexTotal);
        Assert.Equal(0m, e.HoursTotal);
        Assert.Equal(0m, e.AdjustedExpLevel);
        Assert.Equal(0m, e.AdjustedHrs);
        Assert.Equal(0m, e.GrandTotal);
    }

    [Fact]
    public void DetailedSeModuleEntity_Defaults()
    {
        var e = new DetailedSeModuleEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(string.Empty, e.ComponentType);
        Assert.Equal(string.Empty, e.ExperienceLevel);
        Assert.Equal(string.Empty, e.AssociatedRequirement);
        Assert.Equal(string.Empty, e.ModuleName);
        Assert.Equal(string.Empty, e.ComponentStatus);
        Assert.Equal(0, e.SimpleCount);
        Assert.Equal(0, e.ModerateCount);
        Assert.Equal(0, e.ComplexCount);
    }

    [Fact]
    public void DetailedBaTestCaseEntity_Defaults()
    {
        var e = new DetailedBaTestCaseEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(string.Empty, e.TaskName);
        Assert.Equal(string.Empty, e.TaskType);
        Assert.Equal(string.Empty, e.Category);
        Assert.Equal(string.Empty, e.ExperienceLevel);
        Assert.Equal("TestCases", e.GridType);
        Assert.False(e.IsInfoRow);
        Assert.Equal(0m, e.SimpleCount);
        Assert.Equal(0m, e.ManualAdjHours);
    }

    [Fact]
    public void DetailedBaValidationEntity_Defaults()
    {
        var e = new DetailedBaValidationEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(string.Empty, e.TaskName);
        Assert.Equal(string.Empty, e.TaskType);
        Assert.Equal(string.Empty, e.ExperienceLevel);
        Assert.Equal(0, e.SimpleCount);
        Assert.Equal(0m, e.ManualAdjHours);
    }

    [Fact]
    public void DetailedConsultantEntity_Defaults()
    {
        var e = new DetailedConsultantEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(string.Empty, e.Name);
        Assert.Equal(0m, e.Hours);
    }

    [Fact]
    public void DetailedCollabMeetingEntity_Defaults()
    {
        var e = new DetailedCollabMeetingEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(string.Empty, e.MeetingType);
        Assert.Equal(0m, e.MeetingCount);
        Assert.Equal(0m, e.MeetingHours);
        Assert.Equal(0m, e.Attendees);
        Assert.Equal(0m, e.PrepHours);
        Assert.Equal(0m, e.AdjustedMeeting);
        Assert.Equal(0m, e.AdjustedPrep);
    }

    [Fact]
    public void DetailedMiscFieldsEntity_Defaults()
    {
        var e = new DetailedMiscFieldsEntity();
        Assert.Equal(string.Empty, e.ProjectId);
        Assert.Equal(0m, e.PromotionHours);
        Assert.Equal(0m, e.SystemDocHours);
        Assert.Equal(0m, e.PmReservePercentage);
        Assert.Equal(0m, e.CreateDetailEstHours);
        Assert.Equal(0m, e.CreateFinalEstHours);
        Assert.Equal(0m, e.PmEffortHours);
        Assert.Equal(0m, e.RemainingBddHours);
        Assert.Equal(0m, e.SysDocProdValHours);
        Assert.Equal(0m, e.BaSysDocHours);
        Assert.Equal(0m, e.CommPlanHours);
        Assert.Equal(string.Empty, e.SeAdjustedComment);
        Assert.Equal(string.Empty, e.BaAdjustedComment);
        Assert.Equal(string.Empty, e.CollabAdjustedComment);
    }

    #endregion

    #region Entity Persistence Round-Trip

    [Fact]
    public void DetailedSeComponent_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip SE", ChangeOrderId = "RT-001" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedSeComponents.Add(new DetailedSeComponentEntity
        {
            ProjectId = project.ProjectId, LineNumber = 1, ComponentType = "PowerBuilderWindows",
            SimpleTotal = 3, ModerateTotal = 2, ComplexTotal = 1,
            HoursTotal = 150m, AdjustedExpLevel = 187.5m, AdjustedHrs = 10m, GrandTotal = 197.5m
        });
        db.SaveChanges();

        var loaded = db.DetailedSeComponents.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal("PowerBuilderWindows", loaded.ComponentType);
        Assert.Equal(3, loaded.SimpleTotal);
        Assert.Equal(197.5m, loaded.GrandTotal);
    }

    [Fact]
    public void DetailedSeModule_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip Module", ChangeOrderId = "RT-002" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedSeModules.Add(new DetailedSeModuleEntity
        {
            ProjectId = project.ProjectId, ComponentType = "Reports", LineNumber = 1,
            ExperienceLevel = "Proficient", AssociatedRequirement = "REQ-100",
            ModuleName = "Auth Letter Report", ComponentStatus = "New",
            SimpleCount = 0, ModerateCount = 1, ComplexCount = 0,
            ComplexityTotal = 51m, AdjustedExpLevel = 51m, AdjustedHrs = 5m, GrandTotal = 56m
        });
        db.SaveChanges();

        var loaded = db.DetailedSeModules.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal("Auth Letter Report", loaded.ModuleName);
        Assert.Equal(56m, loaded.GrandTotal);
    }

    [Fact]
    public void DetailedBaTestCase_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip BA", ChangeOrderId = "RT-003" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedBaTestCases.Add(new DetailedBaTestCaseEntity
        {
            ProjectId = project.ProjectId, LineNumber = 1, GridType = "TestCases",
            TaskName = "Write System Test Cases", TaskType = "WriteSystemTestCases",
            Category = "SystemTesting", ExperienceLevel = "Proficient",
            SimpleCount = 10, ModerateCount = 5, ComplexCount = 2, VeryComplexCount = 0,
            ManualAdjHours = 3m
        });
        db.SaveChanges();

        var loaded = db.DetailedBaTestCases.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal(10m, loaded.SimpleCount);
        Assert.Equal(5m, loaded.ModerateCount);
        Assert.Equal(3m, loaded.ManualAdjHours);
    }

    [Fact]
    public void DetailedConsultant_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip Consultant", ChangeOrderId = "RT-004" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedConsultants.Add(new DetailedConsultantEntity
        {
            ProjectId = project.ProjectId, LineNumber = 1, Name = "John Smith", Hours = 40m
        });
        db.SaveChanges();

        var loaded = db.DetailedConsultants.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal("John Smith", loaded.Name);
        Assert.Equal(40m, loaded.Hours);
    }

    [Fact]
    public void DetailedCollabMeeting_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip Meeting", ChangeOrderId = "RT-005" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedCollabMeetings.Add(new DetailedCollabMeetingEntity
        {
            ProjectId = project.ProjectId, MeetingType = "WPR",
            MeetingCount = 5, MeetingHours = 1m, Attendees = 4, PrepHours = 0.25m,
            AdjustedMeeting = 2m, AdjustedPrep = 1m
        });
        db.SaveChanges();

        var loaded = db.DetailedCollabMeetings.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal("WPR", loaded.MeetingType);
        Assert.Equal(5m, loaded.MeetingCount);
        Assert.Equal(4m, loaded.Attendees);
    }

    [Fact]
    public void DetailedMiscFields_RoundTrip()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "RoundTrip Misc", ChangeOrderId = "RT-006" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedMiscFields.Add(new DetailedMiscFieldsEntity
        {
            ProjectId = project.ProjectId,
            PromotionHours = 10m, SystemDocHours = 5m, PmReservePercentage = 7m,
            CreateDetailEstHours = 4m, CreateFinalEstHours = 2m, PmEffortHours = 15m,
            RemainingBddHours = 8m, SysDocProdValHours = 3m, BaSysDocHours = 2m, CommPlanHours = 1m,
            SeAdjustedComment = "SE note", BaAdjustedComment = "BA note", CollabAdjustedComment = "Collab note"
        });
        db.SaveChanges();

        var loaded = db.DetailedMiscFields.First(x => x.ProjectId == project.ProjectId);
        Assert.Equal(10m, loaded.PromotionHours);
        Assert.Equal(7m, loaded.PmReservePercentage);
        Assert.Equal("SE note", loaded.SeAdjustedComment);
    }

    [Fact]
    public void CascadeDelete_RemovesDetailedData()
    {
        using var db = CreateInMemoryDb();
        var project = new ProjectEntity { ProjectName = "Cascade Test", ChangeOrderId = "CD-001" };
        db.Projects.Add(project);
        db.SaveChanges();

        db.DetailedSeComponents.Add(new DetailedSeComponentEntity { ProjectId = project.ProjectId, LineNumber = 1, ComponentType = "Reports" });
        db.DetailedConsultants.Add(new DetailedConsultantEntity { ProjectId = project.ProjectId, LineNumber = 1, Name = "Test" });
        db.DetailedCollabMeetings.Add(new DetailedCollabMeetingEntity { ProjectId = project.ProjectId, MeetingType = "WPR" });
        db.DetailedMiscFields.Add(new DetailedMiscFieldsEntity { ProjectId = project.ProjectId });
        db.SaveChanges();

        Assert.Equal(1, db.DetailedSeComponents.Count());
        Assert.Equal(1, db.DetailedConsultants.Count());

        db.Projects.Remove(project);
        db.SaveChanges();

        Assert.Equal(0, db.DetailedSeComponents.Count());
        Assert.Equal(0, db.DetailedConsultants.Count());
        Assert.Equal(0, db.DetailedCollabMeetings.Count());
        Assert.Equal(0, db.DetailedMiscFields.Count());
    }

    #endregion

    // ═══════════════════════════════════════════════════════════════
    // SECTION 10: BaTestCaseRow / BaValidationRow / ConsultantRow Calculations
    // ═══════════════════════════════════════════════════════════════

    #region BaTestCaseRow Calculations

    [Fact]
    public void BaTestCaseRow_WriteSystemTestCases_Proficient_Simple()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaTestCaseRow
        {
            TaskName = "Understanding Requirements",
            Category = BaCategory.SystemTesting,
            TaskType = "UnderstandingRequirements",
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 1
        };
        // Rate = 1.0 per simple understanding req, multiplier = 1.0
        Assert.Equal(1m, row.Total);
        Assert.Equal(1m, row.AdjustedExpLevel);
        Assert.Equal(1m, row.GrandTotal);
    }

    [Fact]
    public void BaTestCaseRow_WriteSystemTestCases_NewToArea_Multiplier()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaTestCaseRow
        {
            TaskName = "Write System Test Cases (# cases)",
            Category = BaCategory.SystemTesting,
            TaskType = "WriteSystemTestCases",
            ExperienceLevel = ExperienceLevel.NewToArea,
            SimpleCount = 10
        };
        // Rate = 0.5 × 10 = 5.0, multiplier = 1.25 → 6.25
        Assert.Equal(5m, row.Total);
        Assert.Equal(6.25m, row.AdjustedExpLevel);
        Assert.Equal(6.25m, row.GrandTotal);
    }

    [Fact]
    public void BaTestCaseRow_WithManualAdjHours()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaTestCaseRow
        {
            TaskName = "Understanding Requirements",
            Category = BaCategory.SystemTesting,
            TaskType = "UnderstandingRequirements",
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 1,
            ManualAdjHours = 3m
        };
        // Rate = 1.0 × 1 = 1.0, multiplier = 1.0, + 3 adj = 4.0
        Assert.Equal(1m, row.Total);
        Assert.Equal(1m, row.AdjustedExpLevel);
        Assert.Equal(4m, row.GrandTotal);
    }

    [Fact]
    public void BaTestCaseRow_InfoRow_ReturnsZeroTotal()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaTestCaseRow
        {
            TaskName = "Iteration",
            Category = BaCategory.SystemTesting,
            TaskType = "Iteration",
            IsInfoRow = true,
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 5
        };
        Assert.Equal(0m, row.Total);
        Assert.Equal(0m, row.AdjustedExpLevel);
    }

    [Fact]
    public void BaTestCaseRow_MixedComplexity()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaTestCaseRow
        {
            TaskName = "Write System Test Cases (# cases)",
            Category = BaCategory.SystemTesting,
            TaskType = "WriteSystemTestCases",
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 5,
            ModerateCount = 3,
            ComplexCount = 2,
            VeryComplexCount = 1
        };
        // (5×0.5) + (3×1) + (2×2.5) + (1×4) = 2.5+3+5+4 = 14.5, multiplier 1.0
        Assert.Equal(14.5m, row.Total);
        Assert.Equal(14.5m, row.GrandTotal);
    }

    #endregion

    #region BaValidationRow Calculations

    [Fact]
    public void BaValidationRow_GeneralValidation_Simple_Proficient()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaValidationRow
        {
            TaskName = "General validation",
            TaskType = "GeneralValidation",
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 1
        };
        // Rate = 5.0 × 1 = 5.0, multiplier = 1.0
        Assert.Equal(5m, row.ComplexityTotal);
        Assert.Equal(5m, row.AdjustedExpLevel);
        Assert.Equal(5m, row.GrandTotal);
    }

    [Fact]
    public void BaValidationRow_MixedComplexity_NewToArea()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaValidationRow
        {
            TaskName = "General validation",
            TaskType = "GeneralValidation",
            ExperienceLevel = ExperienceLevel.NewToArea,
            SimpleCount = 1,
            ModerateCount = 1,
            ComplexCount = 1
        };
        // (1×5) + (1×10) + (1×20) = 35, multiplier = 1.25 → 43.75
        Assert.Equal(35m, row.ComplexityTotal);
        Assert.Equal(43.75m, row.AdjustedExpLevel);
        Assert.Equal(43.75m, row.GrandTotal);
    }

    [Fact]
    public void BaValidationRow_WithManualAdjHours()
    {
        DetailedWeightedValues.ResetToDefaults();
        var row = new BaValidationRow
        {
            TaskName = "Pricing changes",
            TaskType = "PricingChanges",
            ExperienceLevel = ExperienceLevel.Proficient,
            SimpleCount = 1,
            ManualAdjHours = 5m
        };
        // Rate = 5.0 × 1 = 5.0, multiplier = 1.0, + 5 adj = 10.0
        Assert.Equal(5m, row.ComplexityTotal);
        Assert.Equal(5m, row.AdjustedExpLevel);
        Assert.Equal(10m, row.GrandTotal);
    }

    #endregion

    #region ConsultantRow

    [Fact]
    public void ConsultantRow_DefaultValues()
    {
        var row = new ConsultantRow();
        Assert.Equal(string.Empty, row.Name);
        Assert.Equal(0m, row.Hours);
    }

    [Fact]
    public void ConsultantRow_SetValues()
    {
        var row = new ConsultantRow { Name = "Jane Doe", Hours = 24.5m };
        Assert.Equal("Jane Doe", row.Name);
        Assert.Equal(24.5m, row.Hours);
    }

    [Fact]
    public void ConsultantRow_PropertyChanged_Fires()
    {
        var row = new ConsultantRow();
        string? changedProp = null;
        row.PropertyChanged += (_, e) => changedProp = e.PropertyName;

        row.Name = "Test";
        Assert.Equal("Name", changedProp);

        row.Hours = 10m;
        Assert.Equal("Hours", changedProp);
    }

    #endregion
}
