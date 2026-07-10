using Gainwell.EstimationTool.Models;

namespace Gainwell.EstimationTool.Data;

/// <summary>
/// Detailed weighted values lookup — reads from SQLite database (editable via Settings).
/// Falls back to hardcoded defaults if DB is not available.
/// </summary>
public static class DetailedWeightedValues
{
    // SE values: (ComponentType, TaskPhase, ComponentStatus, Complexity) -> Hours
    private static Dictionary<(ComponentType, SeTaskPhase, ComponentStatus, Complexity), decimal> _seMatrix = new()
    {
        // PowerBuilder Windows - New
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 4m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 12m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 20m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 6m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 18m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 30m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 1m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 3m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 2.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 7.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 12.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 2.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 7.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 12.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 2.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 7.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 12.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 1m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 3m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 0.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 1.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 2.5m },
        // PowerBuilder Windows - Existing
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 3.25m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 9.75m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 16m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 14.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 24m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 1m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 2.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 4m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 2m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 6m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 10m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 2m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 6m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 10m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 2m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 6m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 10m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 1m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 2.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 4m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 0.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 1.25m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 2m },

        // Reports - New
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 4.8m },
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 14.4m },
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 24m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 3.6m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 10.8m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 18m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 0.4m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 1.2m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 2m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 2.4m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 4m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 2m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 6m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 10m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 0.4m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 1.2m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 2m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 2.4m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 4m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 2.4m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 4m },
        // Reports - Existing
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 3.84m },
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 11.52m },
        { (ComponentType.Reports, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 19.2m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 2.88m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 8.64m },
        { (ComponentType.Reports, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 14.4m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 0.32m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 0.96m },
        { (ComponentType.Reports, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 1.6m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 1.92m },
        { (ComponentType.Reports, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 3.2m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 1.6m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 4.8m },
        { (ComponentType.Reports, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 8m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 0.32m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 0.96m },
        { (ComponentType.Reports, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 1.6m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 1.92m },
        { (ComponentType.Reports, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 3.2m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 1.92m },
        { (ComponentType.Reports, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 3.2m },

        // Programs/DB Stored Procedures - New
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 20m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 51.2m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 12m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 30m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 76.8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 10.24m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 2.4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 15.36m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 6.4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 16m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 40.96m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 2m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 5.12m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 10m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 25.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 10.24m },
        // Programs/DB Stored Procedures - Existing
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 6.4m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 16m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 40.96m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 9.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 24m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 61.44m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 1.28m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 3.2m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 8.192m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 1.92m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 4.8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 12.288m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 5.12m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 12.8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 32.768m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 1.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 4.096m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 3.2m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 20.48m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 1.28m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 3.2m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 8.192m },

        // Database Manipulation (SQL, PL/SQL, etc.) - New
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Moderate), 4m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.New, Complexity.Complex), 9.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Moderate), 2.4m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.New, Complexity.Complex), 5.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Moderate), 4m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.New, Complexity.Complex), 8m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Moderate), 1.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.New, Complexity.Complex), 2.4m },
        // Database Manipulation - Existing
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Simple), 1.28m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Moderate), 3.2m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlDesign, ComponentStatus.Existing, Complexity.Complex), 7.68m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Moderate), 1.92m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlConstruction, ComponentStatus.Existing, Complexity.Complex), 4.48m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Simple), 1.28m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Moderate), 3.2m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlTesting, ComponentStatus.Existing, Complexity.Complex), 6.4m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Moderate), 1.28m },
        { (ComponentType.DBManipulation, SeTaskPhase.SqlReview, ComponentStatus.Existing, Complexity.Complex), 1.92m },

        // Support Modules/JOB/JIL - New
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Moderate), 4m },
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.New, Complexity.Complex), 9.6m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Moderate), 2m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.New, Complexity.Complex), 4.8m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Moderate), 2m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.New, Complexity.Complex), 4.8m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Moderate), 1.6m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.New, Complexity.Complex), 2.4m },
        // Support Modules - Existing
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Simple), 1.28m },
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Moderate), 3.2m },
        { (ComponentType.SupportModules, SeTaskPhase.DesignModule, ComponentStatus.Existing, Complexity.Complex), 7.68m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Moderate), 1.6m },
        { (ComponentType.SupportModules, SeTaskPhase.BuildModule, ComponentStatus.Existing, Complexity.Complex), 3.84m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Moderate), 1.6m },
        { (ComponentType.SupportModules, SeTaskPhase.TestModule, ComponentStatus.Existing, Complexity.Complex), 3.84m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Simple), 0.64m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Moderate), 1.28m },
        { (ComponentType.SupportModules, SeTaskPhase.ReviewModule, ComponentStatus.Existing, Complexity.Complex), 1.92m },

        // Database Review - flat per table (stored as New/Simple, value is hours per table for each sub-task)
        { (ComponentType.DatabaseReview, SeTaskPhase.Forms, ComponentStatus.New, Complexity.Simple), 0.5m },
        { (ComponentType.DatabaseReview, SeTaskPhase.PreReview, ComponentStatus.New, Complexity.Simple), 1m },
        { (ComponentType.DatabaseReview, SeTaskPhase.ReviewMeeting, ComponentStatus.New, Complexity.Simple), 3.5m },
        { (ComponentType.DatabaseReview, SeTaskPhase.PostReview, ComponentStatus.New, Complexity.Simple), 0.5m },
        { (ComponentType.DatabaseReview, SeTaskPhase.DbChange, ComponentStatus.New, Complexity.Simple), 0.5m },
        { (ComponentType.DatabaseReview, SeTaskPhase.Erwin, ComponentStatus.New, Complexity.Simple), 0.5m },

        // Webpage (Includes UI, Portal & Intranet) - New
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 3.52m },
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 10.56m },
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 15.84m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 5.28m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 15.84m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 23.76m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 0.64m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 1.92m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 2.88m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 1.12m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 3.36m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 5.04m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 2.72m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 8.16m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 12.24m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 0.32m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 0.96m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 1.44m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 1.76m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 5.28m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 7.92m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 0.64m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 1.92m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 2.88m },
        // Webpage - Existing
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 2.816m },
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 8.448m },
        { (ComponentType.Webpage, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 13.1472m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 4.224m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 12.672m },
        { (ComponentType.Webpage, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 19.7208m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 0.512m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 1.536m },
        { (ComponentType.Webpage, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 2.3904m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 0.896m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 2.688m },
        { (ComponentType.Webpage, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 4.1832m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 2.176m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 6.528m },
        { (ComponentType.Webpage, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 10.1592m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 0.256m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 0.768m },
        { (ComponentType.Webpage, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 1.1952m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 1.408m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 4.224m },
        { (ComponentType.Webpage, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 6.5736m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 0.512m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 1.536m },
        { (ComponentType.Webpage, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 2.3904m },

        // K2 Workflow - New
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 8.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 17.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 35.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 13.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 26.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 52.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 3.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 6.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 2.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 5.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 11.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 6.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 13.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 27.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 1.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 3.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 4.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 8.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 17.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 1.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 3.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 6.4m },
        // K2 Workflow - Existing
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 6.16m },
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 14.08m },
        { (ComponentType.K2Workflow, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 26.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 9.24m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 21.12m },
        { (ComponentType.K2Workflow, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 39.6m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 1.12m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 2.56m },
        { (ComponentType.K2Workflow, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 4.8m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 1.96m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 4.48m },
        { (ComponentType.K2Workflow, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 8.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 4.76m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 10.88m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 20.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 0.56m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 1.28m },
        { (ComponentType.K2Workflow, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 2.4m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 3.08m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 7.04m },
        { (ComponentType.K2Workflow, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 13.2m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 1.12m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 2.56m },
        { (ComponentType.K2Workflow, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 4.8m },

        // K2 Smart Form - New
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 2.64m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 8.712m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 15.84m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Simple), 3.96m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Moderate), 13.068m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.New, Complexity.Complex), 23.76m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Simple), 0.48m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Moderate), 1.584m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.New, Complexity.Complex), 2.88m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Simple), 0.84m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Moderate), 2.772m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.New, Complexity.Complex), 5.04m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 2.04m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 6.732m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 12.24m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Simple), 0.24m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Moderate), 0.792m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.New, Complexity.Complex), 1.44m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 1.32m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 4.356m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 7.92m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Simple), 0.48m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Moderate), 1.584m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.New, Complexity.Complex), 2.88m },
        // K2 Smart Form - Existing
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 1.7688m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 6.0984m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 13.1472m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Simple), 2.6532m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Moderate), 9.1476m },
        { (ComponentType.K2SmartForm, SeTaskPhase.GenerateTechnicalDesign, ComponentStatus.Existing, Complexity.Complex), 19.7208m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Simple), 0.3216m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Moderate), 1.1088m },
        { (ComponentType.K2SmartForm, SeTaskPhase.DesignReviewAndAcceptance, ComponentStatus.Existing, Complexity.Complex), 2.3904m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Simple), 0.5628m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Moderate), 1.9404m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UnitTestCasesScenarios, ComponentStatus.Existing, Complexity.Complex), 4.1832m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 1.3668m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 4.7124m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 10.1592m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Simple), 0.1608m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Moderate), 0.5544m },
        { (ComponentType.K2SmartForm, SeTaskPhase.CodeAndUnitTestReviewPreWPR, ComponentStatus.Existing, Complexity.Complex), 1.1952m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 0.8844m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 3.0492m },
        { (ComponentType.K2SmartForm, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 6.5736m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Simple), 0.3216m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Moderate), 1.1088m },
        { (ComponentType.K2SmartForm, SeTaskPhase.ProductionImplementation, ComponentStatus.Existing, Complexity.Complex), 2.3904m },

        // Test Automation Suites (UFT) - New
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Simple), 0.6m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Moderate), 1.002m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.New, Complexity.Complex), 1.602m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Simple), 1.5m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Moderate), 2.505m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.New, Complexity.Complex), 4.005m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Simple), 0.9m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Moderate), 1.503m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.New, Complexity.Complex), 2.403m },
        // Test Automation Suites (UFT) - Existing
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Simple), 0.204m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Moderate), 0.501m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Analysis, ComponentStatus.Existing, Complexity.Complex), 1.00926m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Simple), 0.51m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Moderate), 1.2525m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.CodeConstructionAndUnitTest, ComponentStatus.Existing, Complexity.Complex), 2.52315m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Simple), 0.306m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Moderate), 0.7515m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.TestExecution, ComponentStatus.Existing, Complexity.Complex), 1.51389m },

        // MISC - New
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Simple), 5.6m },
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Moderate), 14m },
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.New, Complexity.Complex), 28m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Simple), 3.2m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Moderate), 8m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.New, Complexity.Complex), 16m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Simple), 3.2m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Moderate), 8m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.New, Complexity.Complex), 16m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Simple), 3.2m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Moderate), 8m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.New, Complexity.Complex), 16m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Simple), 0.8m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Moderate), 2m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.New, Complexity.Complex), 4m },
        // MISC - Existing
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Simple), 2.8m },
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Moderate), 7m },
        { (ComponentType.MISC, SeTaskPhase.ModelOffice, ComponentStatus.Existing, Complexity.Complex), 14m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Simple), 1.6m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Moderate), 4m },
        { (ComponentType.MISC, SeTaskPhase.UAT, ComponentStatus.Existing, Complexity.Complex), 8m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Simple), 1.6m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Moderate), 4m },
        { (ComponentType.MISC, SeTaskPhase.E2E, ComponentStatus.Existing, Complexity.Complex), 8m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Simple), 1.6m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Moderate), 4m },
        { (ComponentType.MISC, SeTaskPhase.Production, ComponentStatus.Existing, Complexity.Complex), 8m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Simple), 0.4m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Moderate), 1m },
        { (ComponentType.MISC, SeTaskPhase.UpdateDocumentation, ComponentStatus.Existing, Complexity.Complex), 2m },

        // ===== TOTAL ROWS (sum of all task phases per component) from Excel =====
        // PowerBuilder Windows Totals
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 20m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 60m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 100m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 16.75m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 48.5m },
        { (ComponentType.PowerBuilderWindows, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 80m },
        // Reports Totals
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 13.6m },
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 40.8m },
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 68m },
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 10.88m },
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 32.64m },
        { (ComponentType.Reports, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 54.4m },
        // Programs/DB Stored Procedures Totals
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 36.8m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 92m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 235.52m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 29.44m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 73.6m },
        { (ComponentType.ProgramsDBStoredProcs, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 188.416m },
        // Database Manipulation Totals
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 4.8m },
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 12m },
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 25.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 3.84m },
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 9.6m },
        { (ComponentType.DBManipulation, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 20.48m },
        // Support Modules Totals
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 4m },
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 9.6m },
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 21.6m },
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 3.2m },
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 7.68m },
        { (ComponentType.SupportModules, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 17.28m },
        // Database Review Total (per table)
        { (ComponentType.DatabaseReview, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 6.5m },
        // Webpage Totals
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 16m },
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 48m },
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 72m },
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 12.8m },
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 38.4m },
        { (ComponentType.Webpage, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 59.76m },
        // K2 Workflow Totals
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 40m },
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 80m },
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 160m },
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 28m },
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 64m },
        { (ComponentType.K2Workflow, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 120m },
        // K2 Smart Form Totals
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 12m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 39.6m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 72m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 8.04m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 27.72m },
        { (ComponentType.K2SmartForm, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 59.76m },
        // Test Automation (UFT) Totals
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 3m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 5.01m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 8.01m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 1.02m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 2.505m },
        { (ComponentType.TestAutomationUFT, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 5.0463m },
        // MISC Totals
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Simple), 16m },
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Moderate), 40m },
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.New, Complexity.Complex), 80m },
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Simple), 8m },
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Moderate), 20m },
        { (ComponentType.MISC, SeTaskPhase.Total, ComponentStatus.Existing, Complexity.Complex), 40m },
    };

    // BA weighted values: (Category, TaskType, Complexity) -> Hours
    private static Dictionary<(BaCategory, string, BaComplexity), decimal> _baMatrix = new()
    {
        // BDD Creation
        { (BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Simple), 4.5m },
        { (BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Moderate), 9m },
        { (BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.Complex), 13.5m },
        { (BaCategory.BddCreation, "PowerBuilderWindows", BaComplexity.VeryComplex), 18m },
        { (BaCategory.BddCreation, "Reports", BaComplexity.Simple), 2.25m },
        { (BaCategory.BddCreation, "Reports", BaComplexity.Moderate), 4.5m },
        { (BaCategory.BddCreation, "Reports", BaComplexity.Complex), 6.75m },
        { (BaCategory.BddCreation, "Reports", BaComplexity.VeryComplex), 9m },
        { (BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Simple), 2m },
        { (BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Moderate), 4m },
        { (BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.Complex), 6m },
        { (BaCategory.BddCreation, "ProgramsDBStoredProcs", BaComplexity.VeryComplex), 8m },
        { (BaCategory.BddCreation, "Webpage", BaComplexity.Simple), 5m },
        { (BaCategory.BddCreation, "Webpage", BaComplexity.Moderate), 10m },
        { (BaCategory.BddCreation, "Webpage", BaComplexity.Complex), 15m },
        { (BaCategory.BddCreation, "Webpage", BaComplexity.VeryComplex), 20m },
        { (BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Simple), 4.5m },
        { (BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Moderate), 9m },
        { (BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.Complex), 13.5m },
        { (BaCategory.BddCreation, "K2WorkflowSmartForm", BaComplexity.VeryComplex), 18m },
        { (BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Simple), 0.5m },
        { (BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Moderate), 0.75m },
        { (BaCategory.BddCreation, "ClaimsEdits", BaComplexity.Complex), 1m },
        { (BaCategory.BddCreation, "ClaimsEdits", BaComplexity.VeryComplex), 2m },
        { (BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Simple), 0.75m },
        { (BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Moderate), 1.25m },
        { (BaCategory.BddCreation, "ClaimsAudits", BaComplexity.Complex), 1.5m },
        { (BaCategory.BddCreation, "ClaimsAudits", BaComplexity.VeryComplex), 2.5m },
        { (BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Simple), 0.75m },
        { (BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Moderate), 1.5m },
        { (BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.Complex), 2.5m },
        { (BaCategory.BddCreation, "ApplicationFunctions", BaComplexity.VeryComplex), 3m },
        { (BaCategory.BddCreation, "EvsRec", BaComplexity.Simple), 0.75m },
        { (BaCategory.BddCreation, "EvsRec", BaComplexity.Moderate), 1.5m },
        { (BaCategory.BddCreation, "EvsRec", BaComplexity.Complex), 2.5m },
        { (BaCategory.BddCreation, "EvsRec", BaComplexity.VeryComplex), 3m },
        { (BaCategory.BddCreation, "Extracts", BaComplexity.Simple), 1m },
        { (BaCategory.BddCreation, "Extracts", BaComplexity.Moderate), 2m },
        { (BaCategory.BddCreation, "Extracts", BaComplexity.Complex), 3m },
        { (BaCategory.BddCreation, "Extracts", BaComplexity.VeryComplex), 4m },
        { (BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Simple), 0.75m },
        { (BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Moderate), 1.5m },
        { (BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.Complex), 2.5m },
        { (BaCategory.BddCreation, "ExternalInterfaces", BaComplexity.VeryComplex), 3m },
        { (BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Simple), 0.75m },
        { (BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Moderate), 1.5m },
        { (BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.Complex), 2.5m },
        { (BaCategory.BddCreation, "ReferenceUpdates", BaComplexity.VeryComplex), 3m },
        { (BaCategory.BddCreation, "Tables", BaComplexity.Simple), 1m },
        { (BaCategory.BddCreation, "Tables", BaComplexity.Moderate), 1.5m },
        { (BaCategory.BddCreation, "Tables", BaComplexity.Complex), 2m },
        { (BaCategory.BddCreation, "Tables", BaComplexity.VeryComplex), 3m },

        // System Testing
        { (BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Simple), 1m },
        { (BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Moderate), 1.5m },
        { (BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.Complex), 2.5m },
        { (BaCategory.SystemTesting, "UnderstandingRequirements", BaComplexity.VeryComplex), 4m },
        { (BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Simple), 0.5m },
        { (BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Moderate), 1m },
        { (BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.Complex), 2.5m },
        { (BaCategory.SystemTesting, "WriteSystemTestCases", BaComplexity.VeryComplex), 4m },
        { (BaCategory.SystemTesting, "DataPreparation", BaComplexity.Simple), 0.5m },
        { (BaCategory.SystemTesting, "DataPreparation", BaComplexity.Moderate), 0.5m },
        { (BaCategory.SystemTesting, "DataPreparation", BaComplexity.Complex), 1m },
        { (BaCategory.SystemTesting, "DataPreparation", BaComplexity.VeryComplex), 1m },
        { (BaCategory.SystemTesting, "AlmTasks", BaComplexity.Simple), 0.25m },
        { (BaCategory.SystemTesting, "AlmTasks", BaComplexity.Moderate), 0.25m },
        { (BaCategory.SystemTesting, "AlmTasks", BaComplexity.Complex), 0.5m },
        { (BaCategory.SystemTesting, "AlmTasks", BaComplexity.VeryComplex), 0.5m },
        { (BaCategory.SystemTesting, "TestExecution", BaComplexity.Simple), 0.5m },
        { (BaCategory.SystemTesting, "TestExecution", BaComplexity.Moderate), 1.5m },
        { (BaCategory.SystemTesting, "TestExecution", BaComplexity.Complex), 3m },
        { (BaCategory.SystemTesting, "TestExecution", BaComplexity.VeryComplex), 6m },
        { (BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Simple), 0.75m },
        { (BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Moderate), 1.25m },
        { (BaCategory.SystemTesting, "RegressionTesting", BaComplexity.Complex), 2m },
        { (BaCategory.SystemTesting, "RegressionTesting", BaComplexity.VeryComplex), 2.5m },
        // Iteration — count-tracking sub-row (0 hours, informational only)
        { (BaCategory.SystemTesting, "Iteration", BaComplexity.Simple), 0m },
        { (BaCategory.SystemTesting, "Iteration", BaComplexity.Moderate), 0m },
        { (BaCategory.SystemTesting, "Iteration", BaComplexity.Complex), 0m },
        { (BaCategory.SystemTesting, "Iteration", BaComplexity.VeryComplex), 0m },
        // Pre Release Defects Creation and Retest
        // Excel formula: H21 = SUM(H16, H19:H20) * 0.1 → rates = (WTC+ALM+SysTest) * 0.1 per complexity
        { (BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Simple), 0.125m },     // (0.5+0.25+0.5)*0.1
        { (BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Moderate), 0.275m },   // (1.0+0.25+1.5)*0.1
        { (BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.Complex), 0.600m },    // (2.5+0.5+3.0)*0.1
        { (BaCategory.SystemTesting, "PreReleaseDefects", BaComplexity.VeryComplex), 1.05m }, // (4.0+0.5+6.0)*0.1

        // Production Validation
        { (BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Simple), 5m },
        { (BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Moderate), 10m },
        { (BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.Complex), 20m },
        { (BaCategory.ProductionValidation, "GeneralValidation", BaComplexity.VeryComplex), 40m },
        { (BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Simple), 5m },
        { (BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Moderate), 10m },
        { (BaCategory.ProductionValidation, "PricingChanges", BaComplexity.Complex), 20m },
        { (BaCategory.ProductionValidation, "PricingChanges", BaComplexity.VeryComplex), 40m },
        { (BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Simple), 5m },
        { (BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Moderate), 10m },
        { (BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.Complex), 20m },
        { (BaCategory.ProductionValidation, "ReferenceChanges", BaComplexity.VeryComplex), 40m },
    };

    // Experience level multipliers: (Role, Level) -> Multiplier
    private static Dictionary<(EstimateRole, ExperienceLevel), decimal> _experienceMatrix = new()
    {
        { (EstimateRole.SE, ExperienceLevel.SelectALevel), 0m },
        { (EstimateRole.SE, ExperienceLevel.NewToArea), 1.25m },
        { (EstimateRole.SE, ExperienceLevel.Proficient), 1m },
        { (EstimateRole.SE, ExperienceLevel.Expert), 0.85m },
        { (EstimateRole.BA, ExperienceLevel.SelectALevel), 0m },
        { (EstimateRole.BA, ExperienceLevel.NewToArea), 1.25m },
        { (EstimateRole.BA, ExperienceLevel.Proficient), 1m },
        { (EstimateRole.BA, ExperienceLevel.Expert), 0.95m },
    };

    // SE Estimate Breakdown percentages
    private static Dictionary<string, decimal> _seBreakdownPercentages = new()
    {
        { "Development", 0.74m },
        { "Testing", 0.10m },
        { "Documentation", 0.13m },
        { "Miscellaneous", 0.03m },
    };

    private static readonly Dictionary<(ComponentType, SeTaskPhase, ComponentStatus, Complexity), decimal> _seDefaults = new(_seMatrix);
    private static readonly Dictionary<(BaCategory, string, BaComplexity), decimal> _baDefaults = new(_baMatrix);
    private static readonly Dictionary<(EstimateRole, ExperienceLevel), decimal> _experienceDefaults = new(_experienceMatrix);

    /// <summary>
    /// Get SE hours for a given component type, task phase, status, and complexity.
    /// </summary>
    public static decimal GetSeHours(ComponentType componentType, SeTaskPhase taskPhase, ComponentStatus status, Complexity complexity)
    {
        return _seMatrix.TryGetValue((componentType, taskPhase, status, complexity), out var value) ? value : 0m;
    }

    /// <summary>
    /// Get BA hours for a given category, task type, and complexity.
    /// </summary>
    public static decimal GetBaHours(BaCategory category, string taskType, BaComplexity complexity)
    {
        return _baMatrix.TryGetValue((category, taskType, complexity), out var value) ? value : 0m;
    }

    /// <summary>
    /// Get the experience level multiplier for a given role and level.
    /// </summary>
    public static decimal GetExperienceMultiplier(EstimateRole role, ExperienceLevel level)
    {
        return _experienceMatrix.TryGetValue((role, level), out var value) ? value : 1m;
    }

    /// <summary>
    /// Get SE estimate breakdown percentage.
    /// </summary>
    public static decimal GetSeBreakdownPercentage(string category)
    {
        return _seBreakdownPercentages.TryGetValue(category, out var value) ? value : 0m;
    }

    /// <summary>
    /// Get the total SE hours for a component (sum of all task phases),
    /// rounded to the nearest 0.25 to match Excel behavior.
    /// </summary>
    public static decimal GetSeTotalHours(ComponentType componentType, ComponentStatus status, Complexity complexity)
    {
        var rawTotal = _seMatrix
            .Where(kv => kv.Key.Item1 == componentType && kv.Key.Item2 != SeTaskPhase.Total && kv.Key.Item3 == status && kv.Key.Item4 == complexity)
            .Sum(kv => kv.Value);
        return RoundToQuarter(rawTotal);
    }

    /// <summary>
    /// Rounds a value to the nearest 0.25 increment (0.00, 0.25, 0.50, 0.75).
    /// Matches Excel MROUND(value, 0.25) behavior.
    /// </summary>
    public static decimal RoundToQuarter(decimal value)
    {
        return Math.Round(value * 4m, MidpointRounding.AwayFromZero) / 4m;
    }

    /// <summary>
    /// Reset all matrices back to compiled defaults. Used by tests.
    /// </summary>
    public static void ResetToDefaults()
    {
        _seMatrix = new Dictionary<(ComponentType, SeTaskPhase, ComponentStatus, Complexity), decimal>(_seDefaults);
        _baMatrix = new Dictionary<(BaCategory, string, BaComplexity), decimal>(_baDefaults);
        _experienceMatrix = new Dictionary<(EstimateRole, ExperienceLevel), decimal>(_experienceDefaults);
    }

    /// <summary>
    /// Load all detailed weighted values from the SQLite database into the in-memory cache.
    /// Called on app startup after database is seeded.
    /// </summary>
    public static void LoadFromDatabase(EstimateDbContext db)
    {
        // Load SE values
        var seValues = db.DetailedSeWeightedValues.ToList();
        if (seValues.Count > 0)
        {
            var newSeMatrix = new Dictionary<(ComponentType, SeTaskPhase, ComponentStatus, Complexity), decimal>();
            foreach (var v in seValues)
                newSeMatrix[(v.ComponentType, v.TaskPhase, v.ComponentStatus, v.Complexity)] = v.Hours;
            _seMatrix = newSeMatrix;
        }

        // Load BA values
        var baValues = db.DetailedBaWeightedValues.ToList();
        if (baValues.Count > 0)
        {
            var newBaMatrix = new Dictionary<(BaCategory, string, BaComplexity), decimal>();
            foreach (var v in baValues)
                newBaMatrix[(v.Category, v.TaskType, v.Complexity)] = v.Hours;
            // Merge hardcoded defaults for any task types not yet in the DB
            // (handles the case where a new task type was added to defaults after the DB was first seeded)
            foreach (var kvp in _baDefaults)
                if (!newBaMatrix.ContainsKey(kvp.Key))
                    newBaMatrix[kvp.Key] = kvp.Value;
            _baMatrix = newBaMatrix;
        }

        // Load experience levels
        var expValues = db.ExperienceLevels.ToList();
        if (expValues.Count > 0)
        {
            var newExpMatrix = new Dictionary<(EstimateRole, ExperienceLevel), decimal>();
            foreach (var v in expValues)
                newExpMatrix[(v.Role, v.Level)] = v.Multiplier;
            _experienceMatrix = newExpMatrix;
        }
    }

    /// <summary>
    /// Update a single SE weighted value in the database and refresh the cache.
    /// </summary>
    public static void UpdateSeValue(EstimateDbContext db, ComponentType type, SeTaskPhase phase, ComponentStatus status, Complexity complexity, decimal newHours, string modifiedBy = "Manager")
    {
        var entity = db.DetailedSeWeightedValues
            .FirstOrDefault(v => v.ComponentType == type && v.TaskPhase == phase && v.ComponentStatus == status && v.Complexity == complexity);

        if (entity != null)
        {
            entity.Hours = newHours;
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            db.SaveChanges();
            _seMatrix[(type, phase, status, complexity)] = newHours;
        }
    }

    /// <summary>
    /// Update a single BA weighted value in the database and refresh the cache.
    /// </summary>
    public static void UpdateBaValue(EstimateDbContext db, BaCategory category, string taskType, BaComplexity complexity, decimal newHours, string modifiedBy = "Manager")
    {
        var entity = db.DetailedBaWeightedValues
            .FirstOrDefault(v => v.Category == category && v.TaskType == taskType && v.Complexity == complexity);

        if (entity != null)
        {
            entity.Hours = newHours;
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            db.SaveChanges();
            _baMatrix[(category, taskType, complexity)] = newHours;
        }
    }

    /// <summary>
    /// Update an experience level multiplier in the database and refresh the cache.
    /// </summary>
    public static void UpdateExperienceLevel(EstimateDbContext db, EstimateRole role, ExperienceLevel level, decimal newMultiplier, string modifiedBy = "Manager")
    {
        var entity = db.ExperienceLevels
            .FirstOrDefault(v => v.Role == role && v.Level == level);

        if (entity != null)
        {
            entity.Multiplier = newMultiplier;
            entity.LastModified = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;
            db.SaveChanges();
            _experienceMatrix[(role, level)] = newMultiplier;
        }
    }

    /// <summary>
    /// Event raised when detailed weighted values are updated (so UI can refresh).
    /// </summary>
    public static event Action? ValuesChanged;

    public static void NotifyValuesChanged() => ValuesChanged?.Invoke();

    /// <summary>
    /// Get the task phases that have weighted values defined for a specific component type.
    /// </summary>
    public static List<SeTaskPhase> GetTaskPhasesForComponent(ComponentType componentType)
    {
        return _seMatrix.Keys
            .Where(k => k.Item1 == componentType)
            .Select(k => k.Item2)
            .Distinct()
            .OrderBy(p => (int)p)
            .ToList();
    }

    /// <summary>
    /// Display-friendly name for SE task phases.
    /// </summary>
    public static string GetTaskPhaseDisplayName(SeTaskPhase phase) => phase switch
    {
        SeTaskPhase.Analysis => "Analysis",
        SeTaskPhase.GenerateTechnicalDesign => "Generate Technical Design",
        SeTaskPhase.DesignReviewAndAcceptance => "Design Review and Acceptance",
        SeTaskPhase.UnitTestCasesScenarios => "Unit Test Cases / Scenarios",
        SeTaskPhase.CodeConstructionAndUnitTest => "Code Construction and Unit Test",
        SeTaskPhase.CodeAndUnitTestReviewPreWPR => "Code and Unit Test Review and Acceptance - Pre WPR",
        SeTaskPhase.UpdateDocumentation => "Update Documentation",
        SeTaskPhase.ProductionImplementation => "Production Implementation",
        SeTaskPhase.SqlDesign => "SQL Design",
        SeTaskPhase.SqlConstruction => "SQL Construction",
        SeTaskPhase.SqlTesting => "SQL Testing",
        SeTaskPhase.SqlReview => "SQL Review",
        SeTaskPhase.DesignModule => "Design Module",
        SeTaskPhase.BuildModule => "Build Module",
        SeTaskPhase.TestModule => "Test Module",
        SeTaskPhase.ReviewModule => "Review Module",
        SeTaskPhase.Forms => "Forms",
        SeTaskPhase.PreReview => "Pre-Review",
        SeTaskPhase.ReviewMeeting => "Review Meeting",
        SeTaskPhase.PostReview => "Post Review",
        SeTaskPhase.DbChange => "DB Change",
        SeTaskPhase.Erwin => "Erwin",
        SeTaskPhase.TestExecution => "Test Execution",
        SeTaskPhase.ModelOffice => "Model Office",
        SeTaskPhase.UAT => "UAT",
        SeTaskPhase.E2E => "E2E",
        SeTaskPhase.Production => "Production",
        _ => phase.ToString()
    };

    /// <summary>
    /// Display-friendly name for BA task types.
    /// </summary>
    public static string GetBaTaskTypeDisplayName(string taskType) => taskType switch
    {
        "PowerBuilderWindows" => "PowerBuilder Windows",
        "Reports" => "Reports",
        "ProgramsDBStoredProcs" => "Programs/DB Stored Procedures",
        "Webpage" => "Webpage (Includes UI, Portal & Intranet)",
        "K2WorkflowSmartForm" => "K2 Workflow-Smart Form",
        "ClaimsEdits" => "Claims-Edits",
        "ClaimsAudits" => "Claims-Audits",
        "ApplicationFunctions" => "Application Functions",
        "EvsRec" => "EVS/REC",
        "Extracts" => "Extracts",
        "ExternalInterfaces" => "External Interfaces",
        "ReferenceUpdates" => "Reference Updates",
        "Tables" => "Tables",
        "UnderstandingRequirements" => "Understanding Requirements",
        "WriteSystemTestCases" => "Write System Test Cases (# cases)",
        "DataPreparation" => "Data Preparation",
        "AlmTasks" => "ALM Upload, Linking and Generating Reports",
        "TestExecution" => "Sys Test Execution",
        "RegressionTesting" => "Regression testing/document (# cases)",
        "GeneralValidation" => "General Validation",
        "PricingChanges" => "Pricing Changes",
        "ReferenceChanges" => "Reference Changes",
        _ => taskType
    };
}
