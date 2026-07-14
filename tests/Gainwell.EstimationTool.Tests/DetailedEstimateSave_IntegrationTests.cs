using System.IO;
using Gainwell.EstimationTool.Data;
using Gainwell.EstimationTool.Models;
using Gainwell.EstimationTool.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Gainwell.EstimationTool.Tests;

/// <summary>
/// Integration tests for Detailed Estimate save fix (navigation → save → reload),
/// CO format validation edge cases, and MigrateSchema logic.
/// </summary>
public class DetailedEstimateSave_IntegrationTests
{
    #region Helpers

    private static string CreateTempDb() =>
        Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid():N}.db");

    private static InitialEstimateViewModel CreateValidViewModel()
    {
        var vm = new InitialEstimateViewModel();
        vm.ProjectName = "Test Project";
        vm.ChangeOrderId = "23327 002";
        vm.ProjectDescription = "Integration test project";
        vm.EstimatedBy = "Tester";
        vm.ReviewedBy = "Reviewer";
        // Add at least one component so save passes validation
        vm.AddComponentCommand.Execute(null);
        var row = vm.Components[^1];
        row.ComponentType = ComponentType.SupportModules;
        row.ChangeType = ChangeType.New;
        row.Size = ComponentSize.Medium;
        row.Count = 1;
        return vm;
    }

    private static void CleanupDb(string dbPath)
    {
        EstimateDbContext.DatabasePathOverride = null;
        try { if (File.Exists(dbPath)) File.Delete(dbPath); } catch { /* SQLite file lock */ }
    }

    #endregion

    #region Category 1: Detailed Estimate Save — Project Lookup Tests

    [Fact]
    public void SaveProject_ThenLookupByProjectId_FindsProject()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            var result = vm.SaveProject();
            Assert.Null(result);

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var project = db.Projects.FirstOrDefault();
            Assert.NotNull(project);
            Assert.False(string.IsNullOrEmpty(project.ProjectId));

            var found = db.Projects.Find(project.ProjectId);
            Assert.NotNull(found);
            Assert.Equal("Test Project", found.ProjectName);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_ThenLookupByProjectName_FindsProject()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var found = db.Projects.FirstOrDefault(p => p.ProjectName == "Test Project");
            Assert.NotNull(found);
            Assert.Equal("23327 002", found.ChangeOrderId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_LookupReturnsCorrectFields()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectDescription = "My detailed description";
            vm.EstimatedBy = "Alice";
            vm.ReviewedBy = "Bob";
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var found = db.Projects.First(p => p.ProjectName == "Test Project");
            Assert.Equal("My detailed description", found.ProjectDescription);
            Assert.Equal("Alice", found.EstimatedBy);
            Assert.Equal("Bob", found.ReviewedBy);
            Assert.Equal("23327 002", found.ChangeOrderId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_SimulateNavigation_LookupByNameFindsOriginal()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.SaveProject();

            // Get the original project ID
            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var original = db.Projects.First(p => p.ProjectName == "Test Project");
            var originalId = original.ProjectId;

            // Simulate navigation: lookup by name still finds original
            var found = db.Projects.FirstOrDefault(p => p.ProjectName == "Test Project");
            Assert.NotNull(found);
            Assert.Equal(originalId, found.ProjectId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_DetailedConsultantEntity_RoundTrips()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var project = new ProjectEntity
            {
                ProjectName = "Consultant Test",
                ChangeOrderId = "11111",
                ProjectDescription = "Test",
                EstimatedBy = "Tester",
                ReviewedBy = "Rev"
            };
            db.Projects.Add(project);

            var consultant = new DetailedConsultantEntity
            {
                ProjectId = project.ProjectId,
                LineNumber = 1,
                Name = "John Expert",
                Hours = 40.5m
            };
            db.DetailedConsultants.Add(consultant);
            db.SaveChanges();

            // Reload
            using var db2 = new EstimateDbContext();
            var loaded = db2.DetailedConsultants.First(c => c.ProjectId == project.ProjectId);
            Assert.Equal("John Expert", loaded.Name);
            Assert.Equal(40.5m, loaded.Hours);
            Assert.Equal(1, loaded.LineNumber);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_DetailedCollabMeetingEntity_RoundTrips()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var project = new ProjectEntity
            {
                ProjectName = "Meeting Test",
                ChangeOrderId = "22222",
                ProjectDescription = "Test",
                EstimatedBy = "Tester",
                ReviewedBy = "Rev"
            };
            db.Projects.Add(project);

            var meeting = new DetailedCollabMeetingEntity
            {
                ProjectId = project.ProjectId,
                MeetingType = "WPR",
                MeetingCount = 10,
                MeetingHours = 1.5m,
                Attendees = 5,
                PrepHours = 0.5m,
                AdjustedMeeting = 2.0m,
                AdjustedPrep = 0.75m
            };
            db.DetailedCollabMeetings.Add(meeting);
            db.SaveChanges();

            using var db2 = new EstimateDbContext();
            var loaded = db2.DetailedCollabMeetings.First(m => m.ProjectId == project.ProjectId);
            Assert.Equal("WPR", loaded.MeetingType);
            Assert.Equal(10, loaded.MeetingCount);
            Assert.Equal(1.5m, loaded.MeetingHours);
            Assert.Equal(5, loaded.Attendees);
            Assert.Equal(0.5m, loaded.PrepHours);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_DetailedMiscFieldsEntity_RoundTrips()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var project = new ProjectEntity
            {
                ProjectName = "Misc Fields Test",
                ChangeOrderId = "33333",
                ProjectDescription = "Test",
                EstimatedBy = "Tester",
                ReviewedBy = "Rev"
            };
            db.Projects.Add(project);

            var misc = new DetailedMiscFieldsEntity
            {
                ProjectId = project.ProjectId,
                PromotionHours = 12.5m,
                SystemDocHours = 8.0m,
                PmReservePercentage = 10m,
                CreateDetailEstHours = 4.0m,
                CreateFinalEstHours = 2.0m,
                PmEffortHours = 20.0m,
                SeAdjustedComment = "SE comment",
                BaAdjustedComment = "BA comment",
                CollabAdjustedComment = "Collab comment"
            };
            db.DetailedMiscFields.Add(misc);
            db.SaveChanges();

            using var db2 = new EstimateDbContext();
            var loaded = db2.DetailedMiscFields.First(m => m.ProjectId == project.ProjectId);
            Assert.Equal(12.5m, loaded.PromotionHours);
            Assert.Equal(8.0m, loaded.SystemDocHours);
            Assert.Equal(10m, loaded.PmReservePercentage);
            Assert.Equal("SE comment", loaded.SeAdjustedComment);
            Assert.Equal("BA comment", loaded.BaAdjustedComment);
            Assert.Equal("Collab comment", loaded.CollabAdjustedComment);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_DirectInsert_Persists()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var project = new ProjectEntity
            {
                ProjectName = "Direct Insert Test",
                ChangeOrderId = "44444",
                ProjectDescription = "Directly inserted",
                EstimatedBy = "Admin",
                ReviewedBy = "Admin2"
            };
            db.Projects.Add(project);
            db.SaveChanges();

            using var db2 = new EstimateDbContext();
            var loaded = db2.Projects.Find(project.ProjectId);
            Assert.NotNull(loaded);
            Assert.Equal("Direct Insert Test", loaded.ProjectName);
            Assert.Equal("44444", loaded.ChangeOrderId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_MultipleProjects_DontConflict()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            // Save first project
            var vm1 = CreateValidViewModel();
            vm1.ProjectName = "Project Alpha";
            vm1.ChangeOrderId = "11111";
            vm1.SaveProject();

            // Save second project
            var vm2 = CreateValidViewModel();
            vm2.ProjectName = "Project Beta";
            vm2.ChangeOrderId = "22222";
            vm2.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var projects = db.Projects.ToList();
            Assert.Equal(2, projects.Count);

            var alpha = projects.First(p => p.ProjectName == "Project Alpha");
            var beta = projects.First(p => p.ProjectName == "Project Beta");
            Assert.Equal("11111", alpha.ChangeOrderId);
            Assert.Equal("22222", beta.ChangeOrderId);
            Assert.NotEqual(alpha.ProjectId, beta.ProjectId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void SaveProject_TwiceWithSameName_Updates_DoesNotDuplicate()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectDescription = "First save";
            vm.SaveProject();

            vm.ProjectDescription = "Updated description";
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var projects = db.Projects.Where(p => p.ProjectName == "Test Project").ToList();
            Assert.Single(projects);
            Assert.Equal("Updated description", projects[0].ProjectDescription);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    #endregion

    #region Category 2: CO Format Validation — Comprehensive Edge Cases

    [Theory]
    [InlineData("00000")]
    [InlineData("99999")]
    [InlineData("12345")]
    [InlineData("23327")]
    [InlineData("00001")]
    [InlineData("10000")]
    public void COFormat_Valid_FiveDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.Null(result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("23327 002")]
    [InlineData("00000 000")]
    [InlineData("99999 999")]
    [InlineData("12345 001")]
    [InlineData("55555 500")]
    public void COFormat_Valid_FiveDigitsSpaceThreeDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.Null(result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData(" 23327 ")]
    [InlineData(" 23327 002 ")]
    [InlineData("  99999  ")]
    public void COFormat_Valid_WithLeadingTrailingSpaces_TrimmedBeforeValidation(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            // Trim() is applied so these should pass validation
            Assert.Null(result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void COFormat_Invalid_EmptyOrWhitespace(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("CO / Defect #", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("CO-001")]
    [InlineData("ABCDE")]
    [InlineData("abcde")]
    [InlineData("AB123")]
    public void COFormat_Invalid_NonNumeric(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("1234")]
    [InlineData("123")]
    [InlineData("1")]
    public void COFormat_Invalid_TooFewDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("1234567")]
    public void COFormat_Invalid_TooManyDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("12345 02")]
    [InlineData("12345 2")]
    public void COFormat_Invalid_SuffixTooFewDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("12345 0234")]
    [InlineData("12345 12345")]
    public void COFormat_Invalid_SuffixTooManyDigits(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("12345-002")]
    [InlineData("12345_002")]
    [InlineData("12345/002")]
    public void COFormat_Invalid_WrongSeparator(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Theory]
    [InlineData("12345  002")]
    [InlineData("12 345")]
    public void COFormat_Invalid_MultipleSpacesOrMisplacedSpace(string co)
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = co;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("99999 or 99999 999 format", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void COFormat_Null_ChangeOrderId_ReturnsError()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ChangeOrderId = null!;
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("CO / Defect #", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    #endregion

    #region Category 3: Schema Migration — MigrateSchema

    [Fact]
    public void EnsureSchema_FreshDb_Succeeds()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            var ex = Record.Exception(() => db.EnsureSchema());
            Assert.Null(ex);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void EnsureSchema_CalledTwice_DoesNotThrow()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var ex = Record.Exception(() => db.EnsureSchema());
            Assert.Null(ex);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void EnsureSchema_DropNotesColumn_ReAddsIt()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            // Create initial schema
            using (var db = new EstimateDbContext())
            {
                db.EnsureSchema();
            }

            // Manually drop the Notes column by recreating the table without it
            // SQLite doesn't support DROP COLUMN directly prior to 3.35, so we
            // simulate a DB that was created before Notes existed by verifying
            // that EnsureSchema's MigrateSchema path handles the ALTER TABLE gracefully
            using (var db = new EstimateDbContext())
            {
                var conn = db.Database.GetDbConnection();
                conn.Open();
                using var cmd = conn.CreateCommand();
                // SQLite 3.35+ supports DROP COLUMN
                cmd.CommandText = "ALTER TABLE [DETAILED_BA_TEST_CASES] DROP COLUMN [Notes]";
                try
                {
                    cmd.ExecuteNonQuery();
                }
                catch
                {
                    // If SQLite version doesn't support DROP COLUMN, recreate table without Notes
                    cmd.CommandText = @"
                        CREATE TABLE DETAILED_BA_TEST_CASES_BACKUP AS SELECT Id, ProjectId, LineNumber, TaskName, TaskType, Category, ExperienceLevel, IsInfoRow, SimpleCount, ModerateCount, ComplexCount, VeryComplexCount, ManualAdjHours, GridType FROM DETAILED_BA_TEST_CASES;
                        DROP TABLE DETAILED_BA_TEST_CASES;
                        ALTER TABLE DETAILED_BA_TEST_CASES_BACKUP RENAME TO DETAILED_BA_TEST_CASES;";
                    cmd.ExecuteNonQuery();
                }
            }

            // Now call EnsureSchema — MigrateSchema should re-add Notes
            using (var db = new EstimateDbContext())
            {
                var ex = Record.Exception(() => db.EnsureSchema());
                Assert.Null(ex);

                // Verify Notes column exists by inserting a row with Notes
                var conn = db.Database.GetDbConnection();
                if (conn.State != System.Data.ConnectionState.Open) conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='DETAILED_BA_TEST_CASES'";
                var schema = cmd.ExecuteScalar()?.ToString() ?? "";
                Assert.Contains("Notes", schema, StringComparison.OrdinalIgnoreCase);
            }
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void EnsureSchema_AllExpectedTablesExist()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
            var tables = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) tables.Add(reader.GetString(0));

            Assert.Contains("PROJECT_ESTIMATES", tables);
            Assert.Contains("COMPONENT_ENTRIES", tables);
            Assert.Contains("COLLABORATION_ITEMS", tables);
            Assert.Contains("DETAILED_SE_COMPONENTS", tables);
            Assert.Contains("DETAILED_SE_MODULES", tables);
            Assert.Contains("DETAILED_BA_TEST_CASES", tables);
            Assert.Contains("DETAILED_BA_VALIDATIONS", tables);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void MigrateSchema_PreservesExistingData()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            // Create schema and insert a row
            using (var db = new EstimateDbContext())
            {
                db.EnsureSchema();
                var project = new ProjectEntity
                {
                    ProjectName = "Migration Test",
                    ChangeOrderId = "55555",
                    ProjectDescription = "Data preservation test",
                    EstimatedBy = "Tester",
                    ReviewedBy = "Rev"
                };
                db.Projects.Add(project);

                var testCase = new DetailedBaTestCaseEntity
                {
                    ProjectId = project.ProjectId,
                    LineNumber = 1,
                    TaskName = "Test Login",
                    TaskType = "Functional",
                    Category = "SystemTest",
                    ExperienceLevel = "Mid",
                    SimpleCount = 5,
                    ModerateCount = 3,
                    GridType = "TestCases"
                };
                db.DetailedBaTestCases.Add(testCase);
                db.SaveChanges();
            }

            // Call EnsureSchema again (triggers MigrateSchema)
            using (var db = new EstimateDbContext())
            {
                db.EnsureSchema();

                // Verify data preserved
                var testCase = db.DetailedBaTestCases.First();
                Assert.Equal("Test Login", testCase.TaskName);
                Assert.Equal("Functional", testCase.TaskType);
                Assert.Equal(5, testCase.SimpleCount);
                Assert.Equal(3, testCase.ModerateCount);
            }
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void MigrateSchema_BaValidations_NotesColumnExists()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table' AND name='DETAILED_BA_VALIDATIONS'";
            var schema = cmd.ExecuteScalar()?.ToString() ?? "";
            Assert.Contains("Notes", schema, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void EnsureSchema_ConsultantsAndMeetingTablesExist()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            using var db = new EstimateDbContext();
            db.EnsureSchema();

            var conn = db.Database.GetDbConnection();
            if (conn.State != System.Data.ConnectionState.Open) conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
            var tables = new List<string>();
            using var reader = cmd.ExecuteReader();
            while (reader.Read()) tables.Add(reader.GetString(0));

            // Detailed Estimate collaboration tables
            Assert.Contains(tables, t => t.Contains("Consultant", StringComparison.OrdinalIgnoreCase)
                                     || t.Contains("CONSULTANT", StringComparison.OrdinalIgnoreCase));
            Assert.Contains(tables, t => t.Contains("Meeting", StringComparison.OrdinalIgnoreCase)
                                     || t.Contains("MEETING", StringComparison.OrdinalIgnoreCase)
                                     || t.Contains("COLLAB", StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    #endregion

    #region Category 4: Navigation Project Handoff

    [Fact]
    public void NavigationHandoff_SaveProject_LookupByName_ProjectIdMatches()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectName = "Navigation Test";
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var project = db.Projects.First(p => p.ProjectName == "Navigation Test");
            Assert.NotNull(project);
            Assert.False(string.IsNullOrEmpty(project.ProjectId));
            Assert.Equal(32, project.ProjectId.Length); // GUID without hyphens
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void NavigationHandoff_TwoProjects_UniqueProjectIds()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm1 = CreateValidViewModel();
            vm1.ProjectName = "Project One";
            vm1.SaveProject();

            var vm2 = CreateValidViewModel();
            vm2.ProjectName = "Project Two";
            vm2.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var p1 = db.Projects.First(p => p.ProjectName == "Project One");
            var p2 = db.Projects.First(p => p.ProjectName == "Project Two");
            Assert.NotEqual(p1.ProjectId, p2.ProjectId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void NavigationHandoff_ProjectEntity_DefaultProjectId_IsValidGuidFormat()
    {
        var entity = new ProjectEntity();
        Assert.Equal(32, entity.ProjectId.Length);
        // Should be parseable as a GUID
        Assert.True(Guid.TryParse(entity.ProjectId, out _));
    }

    [Fact]
    public void NavigationHandoff_SaveTwice_SameName_Updates()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectName = "Updatable Project";
            vm.ChangeOrderId = "11111";
            vm.SaveProject();

            vm.ChangeOrderId = "22222";
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var projects = db.Projects.Where(p => p.ProjectName == "Updatable Project").ToList();
            Assert.Single(projects);
            Assert.Equal("22222", projects[0].ChangeOrderId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void NavigationHandoff_ProjectFields_RoundTrip()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectName = "Roundtrip Fields";
            vm.ProjectDescription = "Full description text";
            vm.EstimatedBy = "Developer A";
            vm.ReviewedBy = "Manager B";
            vm.ChangeOrderId = "77777 003";
            vm.SaveProject();

            using var db = new EstimateDbContext();
            db.EnsureSchema();
            var project = db.Projects.First(p => p.ProjectName == "Roundtrip Fields");
            Assert.Equal("Full description text", project.ProjectDescription);
            Assert.Equal("Developer A", project.EstimatedBy);
            Assert.Equal("Manager B", project.ReviewedBy);
            Assert.Equal("77777 003", project.ChangeOrderId);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void NavigationHandoff_SaveProject_ReturnsNull_OnSuccess()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            var result = vm.SaveProject();
            Assert.Null(result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    [Fact]
    public void NavigationHandoff_SaveProject_MissingProjectName_ReturnsError()
    {
        var dbPath = CreateTempDb();
        EstimateDbContext.DatabasePathOverride = dbPath;
        try
        {
            var vm = CreateValidViewModel();
            vm.ProjectName = "";
            var result = vm.SaveProject();
            Assert.NotNull(result);
            Assert.Contains("Project Name", result);
        }
        finally
        {
            CleanupDb(dbPath);
        }
    }

    #endregion
}
