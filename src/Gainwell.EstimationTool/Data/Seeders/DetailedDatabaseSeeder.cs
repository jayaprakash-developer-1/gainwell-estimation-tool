using Microsoft.EntityFrameworkCore;
using Gainwell.EstimationTool.Models;

namespace Gainwell.EstimationTool.Data;

public static class DetailedDatabaseSeeder
{
    public static void Initialize(EstimateDbContext db)
    {
        db.Database.EnsureCreated();

        // Create tables if they don't exist (handles case where DB existed before these tables were added)
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_SE_WEIGHTED_VALUES (
                Id INTEGER PRIMARY KEY,
                ComponentType INTEGER NOT NULL,
                TaskPhase INTEGER NOT NULL,
                ComponentStatus INTEGER NOT NULL,
                Complexity INTEGER NOT NULL,
                Hours REAL NOT NULL DEFAULT 0,
                LastModified TEXT,
                ModifiedBy TEXT
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_DETAILED_SE_WEIGHTED_VALUES_ComponentType_TaskPhase_ComponentStatus_Complexity
            ON DETAILED_SE_WEIGHTED_VALUES (ComponentType, TaskPhase, ComponentStatus, Complexity)");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_BA_WEIGHTED_VALUES (
                Id INTEGER PRIMARY KEY,
                Category INTEGER NOT NULL,
                TaskType TEXT NOT NULL,
                Complexity INTEGER NOT NULL,
                Hours REAL NOT NULL DEFAULT 0,
                LastModified TEXT,
                ModifiedBy TEXT
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_DETAILED_BA_WEIGHTED_VALUES_Category_TaskType_Complexity
            ON DETAILED_BA_WEIGHTED_VALUES (Category, TaskType, Complexity)");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS EXPERIENCE_LEVELS (
                Id INTEGER PRIMARY KEY,
                Role INTEGER NOT NULL,
                Level INTEGER NOT NULL,
                Multiplier REAL NOT NULL DEFAULT 0,
                LastModified TEXT,
                ModifiedBy TEXT
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE UNIQUE INDEX IF NOT EXISTS IX_EXPERIENCE_LEVELS_Role_Level
            ON EXPERIENCE_LEVELS (Role, Level)");

        // Detailed estimate persistence tables (added later – need CREATE IF NOT EXISTS for existing DBs)
        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_SE_COMPONENTS (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                ComponentType TEXT NOT NULL,
                SimpleTotal INTEGER NOT NULL DEFAULT 0,
                ModerateTotal INTEGER NOT NULL DEFAULT 0,
                ComplexTotal INTEGER NOT NULL DEFAULT 0,
                HoursTotal REAL NOT NULL DEFAULT 0,
                AdjustedExpLevel REAL NOT NULL DEFAULT 0,
                AdjustedHrs REAL NOT NULL DEFAULT 0,
                GrandTotal REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_SE_MODULES (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                ComponentType TEXT NOT NULL,
                ExperienceLevel TEXT,
                AssociatedRequirement TEXT,
                ModuleName TEXT,
                ComponentStatus TEXT,
                SimpleCount INTEGER NOT NULL DEFAULT 0,
                ModerateCount INTEGER NOT NULL DEFAULT 0,
                ComplexCount INTEGER NOT NULL DEFAULT 0,
                ComplexityTotal REAL NOT NULL DEFAULT 0,
                AdjustedExpLevel REAL NOT NULL DEFAULT 0,
                AdjustedHrs REAL NOT NULL DEFAULT 0,
                GrandTotal REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_BA_TEST_CASES (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                TaskName TEXT,
                TaskType TEXT,
                Category TEXT,
                ExperienceLevel TEXT,
                GridType TEXT DEFAULT 'TestCases',
                IsInfoRow INTEGER NOT NULL DEFAULT 0,
                SimpleCount REAL NOT NULL DEFAULT 0,
                ModerateCount REAL NOT NULL DEFAULT 0,
                ComplexCount REAL NOT NULL DEFAULT 0,
                VeryComplexCount REAL NOT NULL DEFAULT 0,
                ManualAdjHours REAL NOT NULL DEFAULT 0,
                Notes TEXT DEFAULT '',
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        // Add IsInfoRow column to existing DETAILED_BA_TEST_CASES tables
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_BA_TEST_CASES ADD COLUMN IsInfoRow INTEGER NOT NULL DEFAULT 0"); }
        catch { /* column already exists */ }

        // Add VeryComplexCount to existing DETAILED_BA_TEST_CASES tables
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_BA_TEST_CASES ADD COLUMN VeryComplexCount REAL NOT NULL DEFAULT 0"); }
        catch { /* column already exists */ }

        // Add Notes column to existing tables (safe to call even if column already exists)
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_BA_TEST_CASES ADD COLUMN Notes TEXT DEFAULT ''"); } catch { /* column already exists */ }

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_BA_VALIDATIONS (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                TaskName TEXT,
                TaskType TEXT,
                ExperienceLevel TEXT,
                SimpleCount INTEGER NOT NULL DEFAULT 0,
                ModerateCount INTEGER NOT NULL DEFAULT 0,
                ComplexCount INTEGER NOT NULL DEFAULT 0,
                VeryComplexCount INTEGER NOT NULL DEFAULT 0,
                ManualAdjHours REAL NOT NULL DEFAULT 0,
                Notes TEXT DEFAULT '',
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_BA_VALIDATIONS ADD COLUMN VeryComplexCount INTEGER NOT NULL DEFAULT 0"); }
        catch { /* column already exists */ };
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_BA_VALIDATIONS ADD COLUMN Notes TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN SeEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN BaEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN CollabEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }

        // Add new columns to DETAILED_MISC_FIELDS for EstimateBy, Assumptions, ActualHours
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN SeEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN BaEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN CollabEstimateBy TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN SeAssumptions TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN BaAssumptions TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN CollabAssumptions TEXT DEFAULT ''"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN ActualHours REAL NOT NULL DEFAULT 0"); } catch { /* column already exists */ }
        try { db.Database.ExecuteSqlRaw("ALTER TABLE DETAILED_MISC_FIELDS ADD COLUMN ActualHoursDate TEXT DEFAULT ''"); } catch { /* column already exists */ }


        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_CONSULTANTS (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                Name TEXT,
                Hours REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_COLLAB_MEETINGS (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                LineNumber INTEGER NOT NULL DEFAULT 0,
                MeetingType TEXT NOT NULL,
                MeetingCount REAL NOT NULL DEFAULT 0,
                MeetingHours REAL NOT NULL DEFAULT 0,
                Attendees REAL NOT NULL DEFAULT 0,
                PrepHours REAL NOT NULL DEFAULT 0,
                AdjustedMeeting REAL NOT NULL DEFAULT 0,
                AdjustedPrep REAL NOT NULL DEFAULT 0,
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        db.Database.ExecuteSqlRaw(@"
            CREATE TABLE IF NOT EXISTS DETAILED_MISC_FIELDS (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                ProjectId TEXT NOT NULL,
                PromotionHours REAL NOT NULL DEFAULT 0,
                SystemDocHours REAL NOT NULL DEFAULT 0,
                PmReservePercentage REAL NOT NULL DEFAULT 0,
                CreateDetailEstHours REAL NOT NULL DEFAULT 0,
                CreateFinalEstHours REAL NOT NULL DEFAULT 0,
                PmEffortHours REAL NOT NULL DEFAULT 0,
                RemainingBddHours REAL NOT NULL DEFAULT 0,
                SysDocProdValHours REAL NOT NULL DEFAULT 0,
                BaSysDocHours REAL NOT NULL DEFAULT 0,
                CommPlanHours REAL NOT NULL DEFAULT 0,
                SeAdjustedComment TEXT,
                BaAdjustedComment TEXT,
                CollabAdjustedComment TEXT,
                SeEstimateBy TEXT,
                BaEstimateBy TEXT,
                CollabEstimateBy TEXT,
                SeAssumptions TEXT,
                BaAssumptions TEXT,
                CollabAssumptions TEXT,
                ActualHours REAL NOT NULL DEFAULT 0,
                ActualHoursDate TEXT,
                FOREIGN KEY (ProjectId) REFERENCES PROJECT_ESTIMATES(PROJECT_ID) ON DELETE CASCADE
            )");

        SeedSeWeightedValues(db);
        SeedBaWeightedValues(db);
        SeedExperienceLevels(db);
    }

    private static void SeedSeWeightedValues(EstimateDbContext db)
    {
        if (db.DetailedSeWeightedValues.Any()) return;

        var values = new List<DetailedSeWeightedValueEntity>();
        int id = 1;

        void A(ComponentType ct, SeTaskPhase tp, ComponentStatus cs, Complexity cx, decimal hours) =>
            values.Add(new DetailedSeWeightedValueEntity { Id = id++, ComponentType = ct, TaskPhase = tp, ComponentStatus = cs, Complexity = cx, Hours = hours });

        // PowerBuilder Windows - New
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 4m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 12m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 20m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 6m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 18m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 30m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 1m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 3m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 2.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 7.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 12.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 2.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 7.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 12.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 2.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 7.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 12.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 1m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 3m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 0.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 1.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 2.5m);
        // PowerBuilder Windows - Existing
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 3.25m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 9.75m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 16m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 14.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 24m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 1m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 2.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 4m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 2m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 6m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 10m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 2m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 6m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 10m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 2m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 6m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 10m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 1m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 2.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 4m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 0.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 1.25m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 2m);

        // Reports - New
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 4.8m);
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 14.4m);
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 24m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 3.6m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 10.8m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 18m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 0.4m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 1.2m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 2m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 2.4m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 4m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 2m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 6m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 10m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 0.4m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 1.2m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 2m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 2.4m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 4m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 2.4m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 4m);
        // Reports - Existing
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 3.84m);
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 11.52m);
        A(ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 19.2m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 2.88m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 8.64m);
        A(ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 14.4m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 0.32m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 0.96m);
        A(ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 1.6m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 1.92m);
        A(ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 3.2m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 1.6m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 4.8m);
        A(ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 8m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 0.32m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 0.96m);
        A(ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 1.6m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 1.92m);
        A(ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 3.2m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 1.92m);
        A(ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 3.2m);

        // Programs/DB Stored Procedures - New
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 20m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 51.2m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 12m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 30m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 76.8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 10.24m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 2.4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 15.36m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 6.4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 16m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 40.96m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 2m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 5.12m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 10m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 25.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 10.24m);
        // Programs/DB Stored Procedures - Existing
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 6.4m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 16m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 40.96m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 9.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 24m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 61.44m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 1.28m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 3.2m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 8.192m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 1.92m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 4.8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 12.288m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 5.12m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 12.8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 32.768m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 1.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 4.096m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 3.2m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 20.48m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 1.28m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 3.2m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 8.192m);

        // Database Manipulation - New
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Moderate, 4m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Complex, 9.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Moderate, 2.4m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Complex, 5.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Moderate, 4m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Complex, 8m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Moderate, 1.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Complex, 2.4m);
        // Database Manipulation - Existing
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Simple, 1.28m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Moderate, 3.2m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Complex, 7.68m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Moderate, 1.92m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Complex, 4.48m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Simple, 1.28m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Moderate, 3.2m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Complex, 6.4m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Moderate, 1.28m);
        A(ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Complex, 1.92m);

        // Support Modules - New
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Moderate, 4m);
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Complex, 9.6m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Moderate, 2m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Complex, 4.8m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Moderate, 2m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Complex, 4.8m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Moderate, 1.6m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Complex, 2.4m);
        // Support Modules - Existing
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Simple, 1.28m);
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Moderate, 3.2m);
        A(ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Complex, 7.68m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Moderate, 1.6m);
        A(ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Complex, 3.84m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Moderate, 1.6m);
        A(ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Complex, 3.84m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Simple, 0.64m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Moderate, 1.28m);
        A(ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Complex, 1.92m);

        // Database Review - flat per table (no complexity/status breakdown)
        A(ComponentType.DatabaseReview, SeTaskPhase.Forms, ComponentStatus.New, Complexity.Simple, 0.5m);
        A(ComponentType.DatabaseReview, SeTaskPhase.PreReview, ComponentStatus.New, Complexity.Simple, 1m);
        A(ComponentType.DatabaseReview, SeTaskPhase.ReviewMeeting, ComponentStatus.New, Complexity.Simple, 3.5m);
        A(ComponentType.DatabaseReview, SeTaskPhase.PostReview, ComponentStatus.New, Complexity.Simple, 0.5m);
        A(ComponentType.DatabaseReview, SeTaskPhase.DbChange, ComponentStatus.New, Complexity.Simple, 0.5m);
        A(ComponentType.DatabaseReview, SeTaskPhase.Erwin, ComponentStatus.New, Complexity.Simple, 0.5m);

        // Webpage - New
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 3.52m);
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 10.56m);
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 15.84m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 5.28m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 15.84m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 23.76m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 0.64m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 1.92m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 2.88m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 1.12m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 3.36m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 5.04m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 2.72m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 8.16m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 12.24m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 0.32m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 0.96m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 1.44m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 1.76m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 5.28m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 7.92m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 0.64m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 1.92m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 2.88m);
        // Webpage - Existing
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 2.816m);
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 8.448m);
        A(ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 13.1472m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 4.224m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 12.672m);
        A(ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 19.7208m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 0.512m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 1.536m);
        A(ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 2.3904m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 0.896m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 2.688m);
        A(ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 4.1832m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 2.176m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 6.528m);
        A(ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 10.1592m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 0.256m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 0.768m);
        A(ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 1.1952m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 1.408m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 4.224m);
        A(ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 6.5736m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 0.512m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 1.536m);
        A(ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 2.3904m);

        // K2 Workflow - New
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 8.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 17.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 35.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 13.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 26.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 52.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 3.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 6.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 2.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 5.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 11.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 6.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 13.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 27.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 1.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 3.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 4.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 8.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 17.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 1.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 3.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 6.4m);
        // K2 Workflow - Existing
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 6.16m);
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 14.08m);
        A(ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 26.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 9.24m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 21.12m);
        A(ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 39.6m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 1.12m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 2.56m);
        A(ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 4.8m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 1.96m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 4.48m);
        A(ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 8.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 4.76m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 10.88m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 20.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 0.56m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 1.28m);
        A(ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 2.4m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 3.08m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 7.04m);
        A(ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 13.2m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 1.12m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 2.56m);
        A(ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 4.8m);

        // K2 Smart Form - New
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 2.64m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 8.712m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 15.84m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple, 3.96m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate, 13.068m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex, 23.76m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple, 0.48m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate, 1.584m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex, 2.88m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple, 0.84m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate, 2.772m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex, 5.04m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 2.04m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 6.732m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 12.24m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple, 0.24m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate, 0.792m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex, 1.44m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 1.32m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 4.356m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 7.92m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple, 0.48m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate, 1.584m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex, 2.88m);
        // K2 Smart Form - Existing
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 1.7688m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 6.0984m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 13.1472m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple, 2.6532m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate, 9.1476m);
        A(ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex, 19.7208m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple, 0.3216m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate, 1.1088m);
        A(ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex, 2.3904m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple, 0.5628m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate, 1.9404m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex, 4.1832m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 1.3668m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 4.7124m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 10.1592m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple, 0.1608m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate, 0.5544m);
        A(ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex, 1.1952m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 0.8844m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 3.0492m);
        A(ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 6.5736m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple, 0.3216m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate, 1.1088m);
        A(ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex, 2.3904m);

        // Test Automation (UFT) - New
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple, 0.6m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate, 1.002m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex, 1.602m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple, 1.5m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate, 2.505m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex, 4.005m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Simple, 0.9m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Moderate, 1.503m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Complex, 2.403m);
        // Test Automation (UFT) - Existing
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple, 0.204m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate, 0.501m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex, 1.00926m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple, 0.51m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate, 1.2525m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex, 2.52315m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Simple, 0.306m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Moderate, 0.7515m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Complex, 1.51389m);

        // MISC - New
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Simple, 5.6m);
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Moderate, 14m);
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Complex, 28m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Simple, 3.2m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Moderate, 8m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Complex, 16m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Simple, 3.2m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Moderate, 8m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Complex, 16m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Simple, 3.2m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Moderate, 8m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Complex, 16m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple, 0.8m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate, 2m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex, 4m);
        // MISC - Existing
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Simple, 2.8m);
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Moderate, 7m);
        A(ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Complex, 14m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Simple, 1.6m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Moderate, 4m);
        A(ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Complex, 8m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Simple, 1.6m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Moderate, 4m);
        A(ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Complex, 8m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Simple, 1.6m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Moderate, 4m);
        A(ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Complex, 8m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple, 0.4m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate, 1m);
        A(ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex, 2m);

        // ===== TOTAL ROWS (sum of all task phases per component) from Excel =====
        // PowerBuilder Windows Totals
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 20m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 60m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 100m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 16.75m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 48.5m);
        A(ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 80m);
        // Reports Totals
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 13.6m);
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 40.8m);
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 68m);
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 10.88m);
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 32.64m);
        A(ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 54.4m);
        // Programs/DB Stored Procedures Totals
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 36.8m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 92m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 235.52m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 29.44m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 73.6m);
        A(ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 188.416m);
        // Database Manipulation Totals
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 4.8m);
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 12m);
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 25.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 3.84m);
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 9.6m);
        A(ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 20.48m);
        // Support Modules Totals
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 4m);
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 9.6m);
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 21.6m);
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 3.2m);
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 7.68m);
        A(ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 17.28m);
        // Database Review Total (per table, single value)
        A(ComponentType.DatabaseReview, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 6.5m);
        // Webpage Totals
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 16m);
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 48m);
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 72m);
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 12.8m);
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 38.4m);
        A(ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 59.76m);
        // K2 Workflow Totals
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 40m);
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 80m);
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 160m);
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 28m);
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 64m);
        A(ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 120m);
        // K2 Smart Form Totals
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 12m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 39.6m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 72m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 8.04m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 27.72m);
        A(ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 59.76m);
        // Test Automation (UFT) Totals
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 3m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 5.01m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 8.01m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 1.02m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 2.505m);
        A(ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 5.0463m);
        // MISC Totals
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple, 16m);
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate, 40m);
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex, 80m);
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple, 8m);
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate, 20m);
        A(ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex, 40m);

        db.DetailedSeWeightedValues.AddRange(values);
        db.SaveChanges();
    }

    private static void SeedBaWeightedValues(EstimateDbContext db)
    {
        if (db.DetailedBaWeightedValues.Any()) return;

        var values = new List<DetailedBaWeightedValueEntity>();
        int id = 1;

        void A(BaCategory cat, string task, BaComplexity cx, decimal hours) =>
            values.Add(new DetailedBaWeightedValueEntity { Id = id++, Category = cat, TaskType = task, Complexity = cx, Hours = hours });

        // BDD Creation
        A(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Simple, 4.5m);
        A(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Moderate, 9m);
        A(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Complex, 13.5m);
        A(BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.VeryComplex, 18m);
        A(BaCategory.BddCreation, "Reports", BaComplexity.Simple, 2.25m);
        A(BaCategory.BddCreation, "Reports", BaComplexity.Moderate, 4.5m);
        A(BaCategory.BddCreation, "Reports", BaComplexity.Complex, 6.75m);
        A(BaCategory.BddCreation, "Reports", BaComplexity.VeryComplex, 9m);
        A(BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Simple, 2m);
        A(BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Moderate, 4m);
        A(BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Complex, 6m);
        A(BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.VeryComplex, 8m);
        A(BaCategory.BddCreation, "Webpage", BaComplexity.Simple, 5m);
        A(BaCategory.BddCreation, "Webpage", BaComplexity.Moderate, 10m);
        A(BaCategory.BddCreation, "Webpage", BaComplexity.Complex, 15m);
        A(BaCategory.BddCreation, "Webpage", BaComplexity.VeryComplex, 20m);
        A(BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Simple, 4.5m);
        A(BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Moderate, 9m);
        A(BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Complex, 13.5m);
        A(BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.VeryComplex, 18m);
        A(BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Simple, 0.5m);
        A(BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Moderate, 0.75m);
        A(BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Complex, 1m);
        A(BaCategory.BddCreation, "ClaimsEdits", BaComplexity.VeryComplex, 2m);
        A(BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Simple, 0.75m);
        A(BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Moderate, 1.25m);
        A(BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Complex, 1.5m);
        A(BaCategory.BddCreation, "ClaimsAudits", BaComplexity.VeryComplex, 2.5m);
        A(BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Simple, 0.75m);
        A(BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Moderate, 1.5m);
        A(BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Complex, 2.5m);
        A(BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.VeryComplex, 3m);
        A(BaCategory.BddCreation, "EvsRec", BaComplexity.Simple, 0.75m);
        A(BaCategory.BddCreation, "EvsRec", BaComplexity.Moderate, 1.5m);
        A(BaCategory.BddCreation, "EvsRec", BaComplexity.Complex, 2.5m);
        A(BaCategory.BddCreation, "EvsRec", BaComplexity.VeryComplex, 3m);
        A(BaCategory.BddCreation, "Extracts", BaComplexity.Simple, 1m);
        A(BaCategory.BddCreation, "Extracts", BaComplexity.Moderate, 2m);
        A(BaCategory.BddCreation, "Extracts", BaComplexity.Complex, 3m);
        A(BaCategory.BddCreation, "Extracts", BaComplexity.VeryComplex, 4m);
        A(BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Simple, 0.75m);
        A(BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Moderate, 1.5m);
        A(BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Complex, 2.5m);
        A(BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.VeryComplex, 3m);
        A(BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Simple, 0.75m);
        A(BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Moderate, 1.5m);
        A(BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Complex, 2.5m);
        A(BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.VeryComplex, 3m);
        A(BaCategory.BddCreation, "Tables", BaComplexity.Simple, 1m);
        A(BaCategory.BddCreation, "Tables", BaComplexity.Moderate, 1.5m);
        A(BaCategory.BddCreation, "Tables", BaComplexity.Complex, 2m);
        A(BaCategory.BddCreation, "Tables", BaComplexity.VeryComplex, 3m);

        // System Testing
        A(BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Simple, 1m);
        A(BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Moderate, 1.5m);
        A(BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Complex, 2.5m);
        A(BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.VeryComplex, 4m);
        A(BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Simple, 0.5m);
        A(BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Moderate, 1m);
        A(BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Complex, 2.5m);
        A(BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.VeryComplex, 4m);
        A(BaCategory.SystemTesting, "DataPreparation", BaComplexity.Simple, 0.5m);
        A(BaCategory.SystemTesting, "DataPreparation", BaComplexity.Moderate, 0.5m);
        A(BaCategory.SystemTesting, "DataPreparation", BaComplexity.Complex, 1m);
        A(BaCategory.SystemTesting, "DataPreparation", BaComplexity.VeryComplex, 1m);
        A(BaCategory.SystemTesting, "AlmTasks", BaComplexity.Simple, 0.25m);
        A(BaCategory.SystemTesting, "AlmTasks", BaComplexity.Moderate, 0.25m);
        A(BaCategory.SystemTesting, "AlmTasks", BaComplexity.Complex, 0.5m);
        A(BaCategory.SystemTesting, "AlmTasks", BaComplexity.VeryComplex, 0.5m);
        A(BaCategory.SystemTesting, "TestExecution", BaComplexity.Simple, 0.5m);
        A(BaCategory.SystemTesting, "TestExecution", BaComplexity.Moderate, 1.5m);
        A(BaCategory.SystemTesting, "TestExecution", BaComplexity.Complex, 3m);
        A(BaCategory.SystemTesting, "TestExecution", BaComplexity.VeryComplex, 6m);
        A(BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Simple, 0.75m);
        A(BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Moderate, 1.25m);
        A(BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Complex, 2m);
        A(BaCategory.SystemTesting, "RegressionTesting", BaComplexity.VeryComplex, 2.5m);
        // Iteration — count-tracking sub-row (0 hours, informational only)
        A(BaCategory.SystemTesting, "Iteration", BaComplexity.Simple, 0m);
        A(BaCategory.SystemTesting, "Iteration", BaComplexity.Moderate, 0m);
        A(BaCategory.SystemTesting, "Iteration", BaComplexity.Complex, 0m);
        A(BaCategory.SystemTesting, "Iteration", BaComplexity.VeryComplex, 0m);
        // Pre Release Defects Creation and Retest
        A(BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Simple, 0.5m);
        A(BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Moderate, 0.275m);  // Excel-verified
        A(BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Complex, 0.600m);   // Excel-verified
        A(BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.VeryComplex, 4.0m);

        // Production Validation
        A(BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Simple, 5m);
        A(BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Moderate, 10m);
        A(BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Complex, 20m);
        A(BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.VeryComplex, 40m);
        A(BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Simple, 5m);
        A(BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Moderate, 10m);
        A(BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Complex, 20m);
        A(BaCategory.ProductionValidation, "PricingChanges", BaComplexity.VeryComplex, 40m);
        A(BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Simple, 5m);
        A(BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Moderate, 10m);
        A(BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Complex, 20m);
        A(BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.VeryComplex, 40m);

        db.DetailedBaWeightedValues.AddRange(values);
        db.SaveChanges();
    }

    private static void SeedExperienceLevels(EstimateDbContext db)
    {
        if (db.ExperienceLevels.Any()) return;

        var values = new List<ExperienceLevelEntity>
        {
            new() { Id = 1, Role = EstimateRole.SE, Level = ExperienceLevel.SelectALevel, Multiplier = 0m },
            new() { Id = 2, Role = EstimateRole.SE, Level = ExperienceLevel.NewToArea, Multiplier = 1.25m },
            new() { Id = 3, Role = EstimateRole.SE, Level = ExperienceLevel.Proficient, Multiplier = 1m },
            new() { Id = 4, Role = EstimateRole.SE, Level = ExperienceLevel.Expert, Multiplier = 0.85m },
            new() { Id = 5, Role = EstimateRole.BA, Level = ExperienceLevel.SelectALevel, Multiplier = 0m },
            new() { Id = 6, Role = EstimateRole.BA, Level = ExperienceLevel.NewToArea, Multiplier = 1.25m },
            new() { Id = 7, Role = EstimateRole.BA, Level = ExperienceLevel.Proficient, Multiplier = 1m },
            new() { Id = 8, Role = EstimateRole.BA, Level = ExperienceLevel.Expert, Multiplier = 0.95m },
        };

        db.ExperienceLevels.AddRange(values);
        db.SaveChanges();
    }
}
