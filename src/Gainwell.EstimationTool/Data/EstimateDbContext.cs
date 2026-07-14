using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Gainwell.EstimationTool.Models;
using System.IO;

namespace Gainwell.EstimationTool.Data;

public class EstimateDbContext : DbContext
{
    public DbSet<WeightedValueEntity> WeightedValues { get; set; } = null!;
    public DbSet<DetailedSeWeightedValueEntity> DetailedSeWeightedValues { get; set; } = null!;
    public DbSet<DetailedBaWeightedValueEntity> DetailedBaWeightedValues { get; set; } = null!;
    public DbSet<ExperienceLevelEntity> ExperienceLevels { get; set; } = null!;
    public DbSet<ProjectEntity> Projects { get; set; } = null!;
    public DbSet<ComponentEntryEntity> ComponentEntries { get; set; } = null!;
    public DbSet<CollaborationItemEntity> CollaborationItems { get; set; } = null!;
    public DbSet<DetailedSeComponentEntity> DetailedSeComponents { get; set; } = null!;
    public DbSet<DetailedSeModuleEntity> DetailedSeModules { get; set; } = null!;
    public DbSet<DetailedBaTestCaseEntity> DetailedBaTestCases { get; set; } = null!;
    public DbSet<DetailedBaValidationEntity> DetailedBaValidations { get; set; } = null!;
    public DbSet<DetailedConsultantEntity> DetailedConsultants { get; set; } = null!;
    public DbSet<DetailedCollabMeetingEntity> DetailedCollabMeetings { get; set; } = null!;
    public DbSet<DetailedMiscFieldsEntity> DetailedMiscFields { get; set; } = null!;

    private static readonly string SecureDbFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Gainwell", "EstimationTool");

    private readonly string _dbPath = string.Empty;

    /// <summary>
    /// Encryption password for SQLite database at rest.
    /// Set before first DbContext use. Requires SQLCipher native library
    /// (install SQLitePCLRaw.bundle_e_sqlcipher NuGet package to enable).
    /// When null, no encryption is applied.
    /// </summary>
    public static string? EncryptionPassword { get; set; }

    /// <summary>
    /// Override database file path (for testing or custom deployments).
    /// When null, uses %LOCALAPPDATA%\Gainwell\EstimationTool\estimates.db.
    /// </summary>
    public static string? DatabasePathOverride { get; set; }

    public EstimateDbContext()
    {
        if (DatabasePathOverride != null)
        {
            _dbPath = DatabasePathOverride;
        }
        else
        {
            Directory.CreateDirectory(SecureDbFolder);
            _dbPath = Path.Combine(SecureDbFolder, "estimates.db");
        }
    }

    public EstimateDbContext(DbContextOptions<EstimateDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Ensures database schema is created. Silently handles "table already exists" errors.
    /// </summary>
    public void EnsureSchema()
    {
        try { Database.EnsureCreated(); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 1 && ex.Message.Contains("already exists")) { }

        // Ensure newer columns exist in existing databases (SQLite ALTER TABLE ADD COLUMN)
        MigrateSchema();
    }

    /// <summary>
    /// Adds columns that may be missing from older database versions.
    /// SQLite only supports ADD COLUMN, not DROP/ALTER, so this is safe to run repeatedly.
    /// </summary>
    private void MigrateSchema()
    {
        var conn = Database.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open) conn.Open();
        using var cmd = conn.CreateCommand();

        // Helper to safely add a column if it doesn't exist
        void AddColumnIfMissing(string table, string column, string type, string defaultVal = "''")
        {
            try
            {
                cmd.CommandText = $"ALTER TABLE [{table}] ADD COLUMN [{column}] {type} DEFAULT {defaultVal}";
                cmd.ExecuteNonQuery();
            }
            catch (SqliteException) { /* column already exists — ignore */ }
        }

        // Notes columns added after initial schema
        AddColumnIfMissing("DETAILED_BA_TEST_CASES", "Notes", "TEXT", "''");
        AddColumnIfMissing("DETAILED_BA_VALIDATIONS", "Notes", "TEXT", "''");

        // Estimated/Reviewed By columns — safety net for databases created before these were added
        AddColumnIfMissing("PROJECT_ESTIMATES", "ESTIMATED_BY", "TEXT", "''");
        AddColumnIfMissing("PROJECT_ESTIMATES", "REVIEWED_BY", "TEXT", "''");
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _dbPath
            };
            if (!string.IsNullOrEmpty(EncryptionPassword))
                builder.Password = EncryptionPassword;
            options.UseSqlite(builder.ConnectionString);
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeightedValueEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ComponentType, e.Size, e.ChangeType }).IsUnique();
            entity.Property(e => e.BaseHours).HasColumnType("REAL");
        });

        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("PROJECT_ESTIMATES");
            entity.HasKey(e => e.ProjectId);
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36);
            entity.Property(e => e.ProjectName).HasColumnName("PROJECT_NAME").HasMaxLength(200).IsRequired();
            entity.HasIndex(e => e.ProjectName).IsUnique();
            entity.Property(e => e.ChangeOrderId).HasColumnName("CHANGE_ORDER_ID").HasMaxLength(100);
            entity.Property(e => e.ProjectDescription).HasColumnName("PROJECT_DESCRIPTION").HasMaxLength(500);
            entity.Property(e => e.EstimatedBy).HasColumnName("ESTIMATED_BY").HasMaxLength(100);
            entity.Property(e => e.ReviewedBy).HasColumnName("REVIEWED_BY").HasMaxLength(100);
            entity.Property(e => e.PmEffortPercentage).HasColumnName("PM_EFFORT_PERCENTAGE").HasColumnType("REAL");
            entity.Property(e => e.PmReservePercentage).HasColumnName("PM_RESERVE_PERCENTAGE").HasColumnType("REAL");
            entity.Property(e => e.TotalDevelopmentHours).HasColumnName("TOTAL_DEVELOPMENT_HOURS").HasColumnType("REAL");
            entity.Property(e => e.GrandTotalHours).HasColumnName("GRAND_TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.TShirtSize).HasColumnName("TSHIRT_SIZE").HasMaxLength(20);
            entity.Property(e => e.CollaborationHours).HasColumnName("COLLABORATION_HOURS").HasColumnType("REAL");
            entity.Property(e => e.SeAdjustedHours).HasColumnName("SE_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BaAdjustedHours).HasColumnName("BA_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.CollaborationAdjustedHours).HasColumnName("COLLAB_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.WprsAdjustedHours).HasColumnName("WPRS_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ClientMeetingsAdjustedHours).HasColumnName("CLIENT_MTG_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.InternalMeetingsAdjustedHours).HasColumnName("INTERNAL_MTG_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.AutomationTestCollabAdjustedHours).HasColumnName("AUTO_TEST_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ConsultantMentorAdjustedHours).HasColumnName("CONSULTANT_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.DevelopmentAdjustedHours).HasColumnName("DEV_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.AnalysisAdjustedHours).HasColumnName("ANALYSIS_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BusinessDesignAdjustedHours).HasColumnName("BIZ_DESIGN_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.SystemTestingAdjustedHours).HasColumnName("SYS_TEST_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.PromotionAdjustedHours).HasColumnName("PROMOTION_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.BaSystemDocAdjustedHours).HasColumnName("BA_DOC_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ProductionValidationAdjustedHours).HasColumnName("PROD_VAL_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ProjectManagementAdjustedHours).HasColumnName("PM_ADJUSTED_HOURS").HasColumnType("REAL");
            entity.Property(e => e.DevelopmentNotes).HasColumnName("DEV_NOTES").HasMaxLength(1000);
            entity.Property(e => e.AnalysisNotes).HasColumnName("ANALYSIS_NOTES").HasMaxLength(1000);
            entity.Property(e => e.BusinessDesignNotes).HasColumnName("BIZ_DESIGN_NOTES").HasMaxLength(1000);
            entity.Property(e => e.SystemTestingNotes).HasColumnName("SYS_TEST_NOTES").HasMaxLength(1000);
            entity.Property(e => e.PromotionNotes).HasColumnName("PROMOTION_NOTES").HasMaxLength(1000);
            entity.Property(e => e.BaSystemDocNotes).HasColumnName("BA_DOC_NOTES").HasMaxLength(1000);
            entity.Property(e => e.ProductionValidationNotes).HasColumnName("PROD_VAL_NOTES").HasMaxLength(1000);
            entity.Property(e => e.ProjectManagementNotes).HasColumnName("PM_NOTES").HasMaxLength(1000);
            entity.Property(e => e.WprsNotes).HasColumnName("WPRS_NOTES").HasMaxLength(1000);
            entity.Property(e => e.ClientMeetingsNotes).HasColumnName("CLIENT_MTG_NOTES").HasMaxLength(1000);
            entity.Property(e => e.InternalMeetingsNotes).HasColumnName("INTERNAL_MTG_NOTES").HasMaxLength(1000);
            entity.Property(e => e.AutomationTestCollabNotes).HasColumnName("AUTO_TEST_NOTES").HasMaxLength(1000);
            entity.Property(e => e.ConsultantMentorNotes).HasColumnName("CONSULTANT_NOTES").HasMaxLength(1000);
            entity.Property(e => e.SeAssumptions).HasColumnName("SE_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.BaAssumptions).HasColumnName("BA_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.CollaborationAssumptions).HasColumnName("COLLAB_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.GeneralAssumptions).HasColumnName("GENERAL_ASSUMPTIONS").HasMaxLength(2000);
            entity.Property(e => e.AdjustedHoursComments).HasColumnName("ADJUSTED_HOURS_COMMENTS").HasMaxLength(4000);
            entity.Property(e => e.TotalActualHours).HasColumnName("TOTAL_ACTUAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.ActualHoursAsOfDate).HasColumnName("ACTUAL_HOURS_AS_OF_DATE");
            entity.Property(e => e.TimeForEstimates).HasColumnName("TIME_FOR_ESTIMATES").HasColumnType("REAL");
            entity.Property(e => e.UseTestCasesForEstimate).HasColumnName("USE_TEST_CASES");
            entity.Property(e => e.TestCasesSimple).HasColumnName("TEST_CASES_SIMPLE").HasColumnType("REAL");
            entity.Property(e => e.TestCasesMedium).HasColumnName("TEST_CASES_MEDIUM").HasColumnType("REAL");
            entity.Property(e => e.TestCasesComplex).HasColumnName("TEST_CASES_COMPLEX").HasColumnType("REAL");
            entity.Property(e => e.TestCasesVeryComplex).HasColumnName("TEST_CASES_VERY_COMPLEX").HasColumnType("REAL");
            entity.Property(e => e.TestCaseIterations).HasColumnName("TEST_CASE_ITERATIONS").HasColumnType("REAL");
            entity.Property(e => e.CreatedDate).HasColumnName("CREATED_DATE");
            entity.Property(e => e.LastModifiedDate).HasColumnName("LAST_MODIFIED_DATE");
            entity.Property(e => e.CreatedBy).HasColumnName("CREATED_BY").HasMaxLength(100);
            entity.Property(e => e.VersionNumber).HasColumnName("VERSION_NUMBER");
            entity.HasMany(e => e.Components).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasMany(e => e.CollaborationItems).WithOne(c => c.Project).HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ComponentEntryEntity>(entity =>
        {
            entity.ToTable("COMPONENT_ENTRIES");
            entity.HasKey(e => e.ComponentId);
            entity.Property(e => e.ComponentId).HasColumnName("COMPONENT_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36).IsRequired();
            entity.Property(e => e.LineNumber).HasColumnName("LINE_NUMBER");
            entity.Property(e => e.RequirementId).HasColumnName("REQUIREMENT_ID").HasMaxLength(50);
            entity.Property(e => e.ComponentType).HasColumnName("COMPONENT_TYPE").HasMaxLength(50).IsRequired();
            entity.Property(e => e.Description).HasColumnName("DESCRIPTION").HasMaxLength(500);
            entity.Property(e => e.ChangeType).HasColumnName("CHANGE_TYPE").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Size).HasColumnName("COMPONENT_SIZE").HasMaxLength(10).IsRequired();
            entity.Property(e => e.Count).HasColumnName("COMPONENT_COUNT");
            entity.Property(e => e.BaseHoursPerUnit).HasColumnName("BASE_HOURS_PER_UNIT").HasColumnType("REAL");
            entity.Property(e => e.TotalHours).HasColumnName("TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.Notes).HasColumnName("NOTES").HasMaxLength(1000);
        });

        modelBuilder.Entity<CollaborationItemEntity>(entity =>
        {
            entity.ToTable("COLLABORATION_ITEMS");
            entity.HasKey(e => e.ItemId);
            entity.Property(e => e.ItemId).HasColumnName("ITEM_ID").ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasColumnName("PROJECT_ID").HasMaxLength(36).IsRequired();
            entity.Property(e => e.LineNumber).HasColumnName("LINE_NUMBER");
            entity.Property(e => e.TaskName).HasColumnName("TASK_NAME").HasMaxLength(200);
            entity.Property(e => e.CollaborationType).HasColumnName("COLLABORATION_TYPE").HasMaxLength(50);
            entity.Property(e => e.NumberOfMeetings).HasColumnName("NUMBER_OF_MEETINGS");
            entity.Property(e => e.MeetingDurationMinutes).HasColumnName("MEETING_DURATION_MINUTES");
            entity.Property(e => e.NumberOfParticipants).HasColumnName("NUMBER_OF_PARTICIPANTS");
            entity.Property(e => e.ParticipantPrepTimeMinutes).HasColumnName("PARTICIPANT_PREP_TIME_MINUTES");
            entity.Property(e => e.TotalHours).HasColumnName("TOTAL_HOURS").HasColumnType("REAL");
            entity.Property(e => e.Notes).HasColumnName("NOTES").HasMaxLength(1000);
        });

        modelBuilder.Entity<DetailedSeWeightedValueEntity>(entity =>
        {
            entity.ToTable("DETAILED_SE_WEIGHTED_VALUES");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.ComponentType, e.TaskPhase, e.ComponentStatus, e.Complexity }).IsUnique();
            entity.Property(e => e.Hours).HasColumnType("REAL");
        });

        modelBuilder.Entity<DetailedBaWeightedValueEntity>(entity =>
        {
            entity.ToTable("DETAILED_BA_WEIGHTED_VALUES");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Category, e.TaskType, e.Complexity }).IsUnique();
            entity.Property(e => e.TaskType).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Hours).HasColumnType("REAL");
        });

        modelBuilder.Entity<ExperienceLevelEntity>(entity =>
        {
            entity.ToTable("EXPERIENCE_LEVELS");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Role, e.Level }).IsUnique();
            entity.Property(e => e.Multiplier).HasColumnType("REAL");
        });

        // === Detailed Estimate Persistence Tables ===

        modelBuilder.Entity<DetailedSeComponentEntity>(entity =>
        {
            entity.ToTable("DETAILED_SE_COMPONENTS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.ComponentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.HoursTotal).HasColumnType("REAL");
            entity.Property(e => e.AdjustedExpLevel).HasColumnType("REAL");
            entity.Property(e => e.AdjustedHrs).HasColumnType("REAL");
            entity.Property(e => e.GrandTotal).HasColumnType("REAL");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedSeModuleEntity>(entity =>
        {
            entity.ToTable("DETAILED_SE_MODULES");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.ComponentType).HasMaxLength(50).IsRequired();
            entity.Property(e => e.ExperienceLevel).HasMaxLength(20);
            entity.Property(e => e.AssociatedRequirement).HasMaxLength(200);
            entity.Property(e => e.ModuleName).HasMaxLength(200);
            entity.Property(e => e.ComponentStatus).HasMaxLength(20);
            entity.Property(e => e.ComplexityTotal).HasColumnType("REAL");
            entity.Property(e => e.AdjustedExpLevel).HasColumnType("REAL");
            entity.Property(e => e.AdjustedHrs).HasColumnType("REAL");
            entity.Property(e => e.GrandTotal).HasColumnType("REAL");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedBaTestCaseEntity>(entity =>
        {
            entity.ToTable("DETAILED_BA_TEST_CASES");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.TaskName).HasMaxLength(200);
            entity.Property(e => e.TaskType).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.ExperienceLevel).HasMaxLength(20);
            entity.Property(e => e.GridType).HasMaxLength(20);
            entity.Property(e => e.IsInfoRow);
            entity.Property(e => e.SimpleCount).HasColumnType("REAL");
            entity.Property(e => e.ModerateCount).HasColumnType("REAL");
            entity.Property(e => e.ComplexCount).HasColumnType("REAL");
            entity.Property(e => e.VeryComplexCount).HasColumnType("REAL");
            entity.Property(e => e.ManualAdjHours).HasColumnType("REAL");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedBaValidationEntity>(entity =>
        {
            entity.ToTable("DETAILED_BA_VALIDATIONS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.TaskName).HasMaxLength(200);
            entity.Property(e => e.TaskType).HasMaxLength(50);
            entity.Property(e => e.ExperienceLevel).HasMaxLength(20);
            entity.Property(e => e.SimpleCount);
            entity.Property(e => e.ModerateCount);
            entity.Property(e => e.ComplexCount);
            entity.Property(e => e.VeryComplexCount);
            entity.Property(e => e.ManualAdjHours).HasColumnType("REAL");
            entity.Property(e => e.Notes).HasMaxLength(1000);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedConsultantEntity>(entity =>
        {
            entity.ToTable("DETAILED_CONSULTANTS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Hours).HasColumnType("REAL");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedCollabMeetingEntity>(entity =>
        {
            entity.ToTable("DETAILED_COLLAB_MEETINGS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.MeetingType).HasMaxLength(20).IsRequired();
            entity.Property(e => e.MeetingCount).HasColumnType("REAL");
            entity.Property(e => e.MeetingHours).HasColumnType("REAL");
            entity.Property(e => e.Attendees).HasColumnType("REAL");
            entity.Property(e => e.PrepHours).HasColumnType("REAL");
            entity.Property(e => e.AdjustedMeeting).HasColumnType("REAL");
            entity.Property(e => e.AdjustedPrep).HasColumnType("REAL");
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DetailedMiscFieldsEntity>(entity =>
        {
            entity.ToTable("DETAILED_MISC_FIELDS");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.ProjectId).HasMaxLength(36).IsRequired();
            entity.Property(e => e.PromotionHours).HasColumnType("REAL");
            entity.Property(e => e.SystemDocHours).HasColumnType("REAL");
            entity.Property(e => e.PmReservePercentage).HasColumnType("REAL");
            entity.Property(e => e.CreateDetailEstHours).HasColumnType("REAL");
            entity.Property(e => e.CreateFinalEstHours).HasColumnType("REAL");
            entity.Property(e => e.PmEffortHours).HasColumnType("REAL");
            entity.Property(e => e.RemainingBddHours).HasColumnType("REAL");
            entity.Property(e => e.SysDocProdValHours).HasColumnType("REAL");
            entity.Property(e => e.BaSysDocHours).HasColumnType("REAL");
            entity.Property(e => e.CommPlanHours).HasColumnType("REAL");
            entity.Property(e => e.SeAdjustedComment).HasMaxLength(2000);
            entity.Property(e => e.BaAdjustedComment).HasMaxLength(2000);
            entity.Property(e => e.CollabAdjustedComment).HasMaxLength(2000);
            entity.Property(e => e.SeEstimateBy).HasMaxLength(200);
            entity.Property(e => e.BaEstimateBy).HasMaxLength(200);
            entity.Property(e => e.CollabEstimateBy).HasMaxLength(200);
            entity.Property(e => e.SeAssumptions).HasMaxLength(4000);
            entity.Property(e => e.BaAssumptions).HasMaxLength(4000);
            entity.Property(e => e.CollabAssumptions).HasMaxLength(4000);
            entity.Property(e => e.ActualHours).HasColumnType("REAL");
            entity.Property(e => e.ActualHoursDate).HasMaxLength(20);
            entity.HasOne(e => e.Project).WithMany().HasForeignKey(e => e.ProjectId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
