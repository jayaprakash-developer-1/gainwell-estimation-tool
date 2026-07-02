namespace InitialEstimatePOC.Models;

/// <summary>
/// Whether a component is New or Existing (used in Detailed Estimate).
/// </summary>
public enum ComponentStatus
{
    Select = -1,
    New = 0,
    Existing = 1
}

/// <summary>
/// Complexity level for SE work in the Detailed Estimate.
/// </summary>
public enum Complexity
{
    Simple,
    Moderate,
    Complex
}

/// <summary>
/// Complexity level for BA work in the Detailed Estimate (includes Very Complex).
/// </summary>
public enum BaComplexity
{
    Simple,
    Moderate,
    Complex,
    VeryComplex
}

/// <summary>
/// Experience level of the engineer/analyst.
/// </summary>
public enum ExperienceLevel
{
    SelectALevel,
    NewToArea,
    Proficient,
    Expert
}

/// <summary>
/// Role type for experience level multipliers.
/// </summary>
public enum EstimateRole
{
    SE,
    BA
}

/// <summary>
/// Task phases for SE work in the Detailed Estimate.
/// </summary>
public enum SeTaskPhase
{
    Analysis,
    GenerateTechnicalDesign,
    DesignReviewAndAcceptance,
    UnitTestCasesScenarios,
    CodeConstructionAndUnitTest,
    CodeAndUnitTestReviewPreWPR,
    UpdateDocumentation,
    ProductionImplementation,
    // Database Manipulation specific
    SqlDesign,
    SqlConstruction,
    SqlTesting,
    SqlReview,
    // Support Modules specific
    DesignModule,
    BuildModule,
    TestModule,
    ReviewModule,
    // Database Review specific (flat per table)
    Forms,
    PreReview,
    ReviewMeeting,
    PostReview,
    DbChange,
    Erwin,
    // Test Automation (UFT) specific
    TestExecution,
    // MISC specific
    ModelOffice,
    UAT,
    E2E,
    Production,
    // Total (sum of all task phases for a component)
    Total
}

/// <summary>
/// BA task categories in the Detailed Estimate.
/// </summary>
public enum BaCategory
{
    BddCreation,
    SystemTesting,
    ProductionValidation
}

/// <summary>
/// BA task types for BDD Creation.
/// </summary>
public enum BaBddComponentType
{
    PowerBuilderWindows,
    Reports,
    ProgramsDBStoredProcs,
    Webpage,
    K2WorkflowSmartForm,
    ClaimsEdits,
    ClaimsAudits,
    ApplicationFunctions,
    EvsRec,
    Extracts,
    ExternalInterfaces,
    ReferenceUpdates,
    Tables
}

/// <summary>
/// BA task types for System Testing.
/// </summary>
public enum BaSystemTestTask
{
    UnderstandingRequirements,
    WriteSystemTestCases,
    DataPreparation,
    AlmTasks,
    TestExecution,
    RegressionTesting
}

/// <summary>
/// BA task types for Production Validation.
/// </summary>
public enum BaProductionValidationType
{
    GeneralValidation,
    PricingChanges,
    ReferenceChanges
}
