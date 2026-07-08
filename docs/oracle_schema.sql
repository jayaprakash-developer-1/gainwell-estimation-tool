-- ============================================================================
-- GAINWELL ESTIMATION TOOL - Oracle Database Schema
-- ============================================================================
-- Generated from SQLite EF Core model for Oracle DB replication
-- Application: PROMISe Estimation Tool (WPF Desktop → Oracle Backend)
-- Version: 1.0
-- Date: 2026-07-08
--
-- NOTES:
--   - SQLite REAL → Oracle NUMBER(10,2) or NUMBER(10,4) depending on precision
--   - SQLite INTEGER autoincrement → Oracle NUMBER + IDENTITY or SEQUENCE
--   - SQLite TEXT → Oracle VARCHAR2(n) or CLOB
--   - All ROUNDUP calculations are performed in the application layer
--   - Enum values are stored as INTEGER ordinals (0-based)
-- ============================================================================

-- ============================================================================
-- TABLE: WEIGHTED_VALUES
-- Purpose: Lookup table for Initial Estimate base hours per component type,
--          size, and change type. Editable by managers via Settings page.
-- Source:  PROMISe Estimating Tool Excel - "Weighted Values" sheet
-- ============================================================================
CREATE TABLE WEIGHTED_VALUES (
    ID                  NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    COMPONENT_TYPE      NUMBER(3)       NOT NULL,
    SIZE                NUMBER(3)       NOT NULL,
    CHANGE_TYPE         NUMBER(3)       NOT NULL,
    BASE_HOURS          NUMBER(10,4)    NOT NULL,
    LAST_MODIFIED       TIMESTAMP       DEFAULT SYSTIMESTAMP NOT NULL,
    MODIFIED_BY         VARCHAR2(100)   DEFAULT 'System' NOT NULL,
    --
    CONSTRAINT UQ_WEIGHTED_VALUES UNIQUE (COMPONENT_TYPE, SIZE, CHANGE_TYPE)
);

COMMENT ON TABLE WEIGHTED_VALUES IS 'Initial Estimate weighted values lookup table. Each row maps a (ComponentType, Size, ChangeType) combination to base development hours per unit. Managers can edit via Settings.';

COMMENT ON COLUMN WEIGHTED_VALUES.ID IS 'Auto-generated primary key';
COMMENT ON COLUMN WEIGHTED_VALUES.COMPONENT_TYPE IS 'Enum ordinal: 0=PowerBuilderWindows, 1=Reports, 2=ProgramsDBStoredProcs, 3=SupportModules, 4=DBManipulation, 5=DatabaseReview, 6=Webpage, 7=K2Workflow, 8=K2SmartForm, 9=TestAutomationUFT, 10=MISC';
COMMENT ON COLUMN WEIGHTED_VALUES.SIZE IS 'Enum ordinal: 0=Small, 1=Medium, 2=Large';
COMMENT ON COLUMN WEIGHTED_VALUES.CHANGE_TYPE IS 'Enum ordinal: 0=New, 1=Change';
COMMENT ON COLUMN WEIGHTED_VALUES.BASE_HOURS IS 'Base development hours per unit for this combination. Precision to 4 decimal places (e.g., 4.0625)';
COMMENT ON COLUMN WEIGHTED_VALUES.LAST_MODIFIED IS 'Timestamp of last modification';
COMMENT ON COLUMN WEIGHTED_VALUES.MODIFIED_BY IS 'Username of person who last modified this value';


-- ============================================================================
-- TABLE: DETAILED_SE_WEIGHTED_VALUES
-- Purpose: Detailed Estimate SE (Software Engineering) hours lookup.
--          Each row maps (ComponentType, TaskPhase, ComponentStatus, Complexity)
--          to base hours for that task phase.
-- Source:  Detailed Estimate Excel - SE task phase breakdown
-- ============================================================================
CREATE TABLE DETAILED_SE_WEIGHTED_VALUES (
    ID                  NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    COMPONENT_TYPE      NUMBER(3)       NOT NULL,
    TASK_PHASE          NUMBER(3)       NOT NULL,
    COMPONENT_STATUS    NUMBER(3)       NOT NULL,
    COMPLEXITY          NUMBER(3)       NOT NULL,
    HOURS               NUMBER(10,2)    NOT NULL,
    LAST_MODIFIED       TIMESTAMP       DEFAULT SYSTIMESTAMP NOT NULL,
    MODIFIED_BY         VARCHAR2(100)   DEFAULT 'System' NOT NULL,
    --
    CONSTRAINT UQ_DETAILED_SE_WV UNIQUE (COMPONENT_TYPE, TASK_PHASE, COMPONENT_STATUS, COMPLEXITY)
);

COMMENT ON TABLE DETAILED_SE_WEIGHTED_VALUES IS 'Detailed Estimate SE weighted values. Maps each SE task phase to hours based on component type, status (New/Existing), and complexity level.';

COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.ID IS 'Auto-generated primary key';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.COMPONENT_TYPE IS 'Enum ordinal: 0=PowerBuilderWindows, 1=Reports, 2=ProgramsDBStoredProcs, 3=SupportModules, 4=DBManipulation, 5=DatabaseReview, 6=Webpage, 7=K2Workflow, 8=K2SmartForm, 9=TestAutomationUFT, 10=MISC';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.TASK_PHASE IS 'SE Task Phase enum ordinal: 0=Analysis, 1=GenerateTechnicalDesign, 2=DesignReviewAndAcceptance, 3=UnitTestCasesScenarios, 4=CodeConstructionAndUnitTest, 5=CodeAndUnitTestReviewPreWPR, 6=UpdateDocumentation, 7=ProductionImplementation, 8=SqlDesign, 9=SqlConstruction, 10=SqlTesting, 11=SqlReview, 12=DesignModule, 13=BuildModule, 14=TestModule, 15=ReviewModule, 16=Forms, 17=PreReview, 18=ReviewMeeting, 19=PostReview, 20=DbChange, 21=Erwin, 22=TestExecution, 23=ModelOffice, 24=UAT, 25=E2E, 26=Production, 27=Total';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.COMPONENT_STATUS IS 'Enum ordinal: 0=New, 1=Existing';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.COMPLEXITY IS 'SE Complexity enum ordinal: 0=Simple, 1=Moderate, 2=Complex';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.HOURS IS 'Base hours for this specific task phase combination';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.LAST_MODIFIED IS 'Timestamp of last modification';
COMMENT ON COLUMN DETAILED_SE_WEIGHTED_VALUES.MODIFIED_BY IS 'Username of person who last modified this value';


-- ============================================================================
-- TABLE: DETAILED_BA_WEIGHTED_VALUES
-- Purpose: Detailed Estimate BA (Business Analyst) hours lookup.
--          Each row maps (Category, TaskType, Complexity) to base hours.
-- Source:  Detailed Estimate Excel - BA task breakdown
-- ============================================================================
CREATE TABLE DETAILED_BA_WEIGHTED_VALUES (
    ID                  NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CATEGORY            NUMBER(3)       NOT NULL,
    TASK_TYPE           VARCHAR2(100)   NOT NULL,
    COMPLEXITY          NUMBER(3)       NOT NULL,
    HOURS               NUMBER(10,2)   NOT NULL,
    LAST_MODIFIED       TIMESTAMP       DEFAULT SYSTIMESTAMP NOT NULL,
    MODIFIED_BY         VARCHAR2(100)   DEFAULT 'System' NOT NULL,
    --
    CONSTRAINT UQ_DETAILED_BA_WV UNIQUE (CATEGORY, TASK_TYPE, COMPLEXITY)
);

COMMENT ON TABLE DETAILED_BA_WEIGHTED_VALUES IS 'Detailed Estimate BA weighted values. Maps BA task types to hours based on category and complexity.';

COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.ID IS 'Auto-generated primary key';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.CATEGORY IS 'BA Category enum ordinal: 0=BddCreation, 1=SystemTesting, 2=ProductionValidation';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.TASK_TYPE IS 'Task type name string. For BddCreation: PowerBuilderWindows, Reports, ProgramsDBStoredProcs, Webpage, K2WorkflowSmartForm, ClaimsEdits, ClaimsAudits, ApplicationFunctions, EvsRec, Extracts, ExternalInterfaces, ReferenceUpdates, Tables. For SystemTesting: UnderstandingRequirements, WriteSystemTestCases, Iteration, DataPreparation, AlmTasks, TestExecution, PreReleaseDefects, RegressionTesting. For ProductionValidation: GeneralValidation, PricingChanges, ReferenceChanges';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.COMPLEXITY IS 'BA Complexity enum ordinal: 0=Simple, 1=Moderate, 2=Complex, 3=VeryComplex';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.HOURS IS 'Base hours for this BA task combination';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.LAST_MODIFIED IS 'Timestamp of last modification';
COMMENT ON COLUMN DETAILED_BA_WEIGHTED_VALUES.MODIFIED_BY IS 'Username of person who last modified this value';


-- ============================================================================
-- TABLE: EXPERIENCE_LEVELS
-- Purpose: Multiplier table for experience levels per role.
--          Hours are multiplied by this factor based on engineer/analyst experience.
-- ============================================================================
CREATE TABLE EXPERIENCE_LEVELS (
    ID                  NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ROLE                NUMBER(3)       NOT NULL,
    LEVEL               NUMBER(3)       NOT NULL,
    MULTIPLIER          NUMBER(5,2)     NOT NULL,
    LAST_MODIFIED       TIMESTAMP       DEFAULT SYSTIMESTAMP NOT NULL,
    MODIFIED_BY         VARCHAR2(100)   DEFAULT 'System' NOT NULL,
    --
    CONSTRAINT UQ_EXPERIENCE_LEVELS UNIQUE (ROLE, LEVEL)
);

COMMENT ON TABLE EXPERIENCE_LEVELS IS 'Experience level multipliers per role. Base hours are multiplied by this factor depending on the resource experience level.';

COMMENT ON COLUMN EXPERIENCE_LEVELS.ID IS 'Auto-generated primary key';
COMMENT ON COLUMN EXPERIENCE_LEVELS.ROLE IS 'Role enum ordinal: 0=SE (Software Engineer), 1=BA (Business Analyst)';
COMMENT ON COLUMN EXPERIENCE_LEVELS.LEVEL IS 'Experience Level enum ordinal: 0=SelectALevel (placeholder), 1=NewToArea, 2=Proficient, 3=Expert';
COMMENT ON COLUMN EXPERIENCE_LEVELS.MULTIPLIER IS 'Hours multiplier. E.g., 1.25 for NewToArea (25% more time), 1.00 for Proficient, 0.85 for Expert';
COMMENT ON COLUMN EXPERIENCE_LEVELS.LAST_MODIFIED IS 'Timestamp of last modification';
COMMENT ON COLUMN EXPERIENCE_LEVELS.MODIFIED_BY IS 'Username of person who last modified this value';


-- ============================================================================
-- TABLE: PROJECT_ESTIMATES
-- Purpose: Master project estimate table. One row per saved estimate.
--          Contains project metadata, summary totals, adjusted hours,
--          adjustment notes, assumptions, and audit fields.
-- Source:  PROMISe Estimating Tool Excel - Summary/Header data
-- ============================================================================
CREATE TABLE PROJECT_ESTIMATES (
    -- Primary Key
    PROJECT_ID                  VARCHAR2(36)    DEFAULT SYS_GUID() PRIMARY KEY,

    -- Project Metadata
    PROJECT_NAME                VARCHAR2(200)   NOT NULL,
    CHANGE_ORDER_ID             VARCHAR2(100),
    PROJECT_DESCRIPTION         VARCHAR2(500),
    ESTIMATED_BY                VARCHAR2(100),
    REVIEWED_BY                 VARCHAR2(100),

    -- PM Configuration
    PM_EFFORT_PERCENTAGE        NUMBER(5,2)     DEFAULT 15.00,
    PM_RESERVE_PERCENTAGE       NUMBER(5,2)     DEFAULT 0,

    -- Summary Totals (calculated in application, persisted for reporting)
    TOTAL_DEVELOPMENT_HOURS     NUMBER(10,2)    DEFAULT 0,
    GRAND_TOTAL_HOURS           NUMBER(10,2)    DEFAULT 0,
    TSHIRT_SIZE                 VARCHAR2(20),
    COLLABORATION_HOURS         NUMBER(10,2)    DEFAULT 0,

    -- Adjusted Hours: SE/Development breakdown (Mid-Project Re-estimation)
    SE_ADJUSTED_HOURS           NUMBER(10,2)    DEFAULT 0,
    BA_ADJUSTED_HOURS           NUMBER(10,2)    DEFAULT 0,
    COLLAB_ADJUSTED_HOURS       NUMBER(10,2)    DEFAULT 0,
    WPRS_ADJUSTED_HOURS         NUMBER(10,2)    DEFAULT 0,
    CLIENT_MTG_ADJUSTED_HOURS   NUMBER(10,2)    DEFAULT 0,
    INTERNAL_MTG_ADJUSTED_HOURS NUMBER(10,2)    DEFAULT 0,
    AUTO_TEST_ADJUSTED_HOURS    NUMBER(10,2)    DEFAULT 0,
    CONSULTANT_ADJUSTED_HOURS   NUMBER(10,2)    DEFAULT 0,

    -- Adjusted Hours: Task-type breakdown
    DEV_ADJUSTED_HOURS          NUMBER(10,2)    DEFAULT 0,
    ANALYSIS_ADJUSTED_HOURS     NUMBER(10,2)    DEFAULT 0,
    BIZ_DESIGN_ADJUSTED_HOURS   NUMBER(10,2)    DEFAULT 0,
    SYS_TEST_ADJUSTED_HOURS     NUMBER(10,2)    DEFAULT 0,
    PROMOTION_ADJUSTED_HOURS    NUMBER(10,2)    DEFAULT 0,
    BA_DOC_ADJUSTED_HOURS       NUMBER(10,2)    DEFAULT 0,
    PROD_VAL_ADJUSTED_HOURS     NUMBER(10,2)    DEFAULT 0,
    PM_ADJUSTED_HOURS           NUMBER(10,2)    DEFAULT 0,

    -- Adjustment Notes (per task type, max 1000 chars each)
    DEV_NOTES                   VARCHAR2(1000),
    ANALYSIS_NOTES              VARCHAR2(1000),
    BIZ_DESIGN_NOTES            VARCHAR2(1000),
    SYS_TEST_NOTES              VARCHAR2(1000),
    PROMOTION_NOTES             VARCHAR2(1000),
    BA_DOC_NOTES                VARCHAR2(1000),
    PROD_VAL_NOTES              VARCHAR2(1000),
    PM_NOTES                    VARCHAR2(1000),
    WPRS_NOTES                  VARCHAR2(1000),
    CLIENT_MTG_NOTES            VARCHAR2(1000),
    INTERNAL_MTG_NOTES          VARCHAR2(1000),
    AUTO_TEST_NOTES             VARCHAR2(1000),
    CONSULTANT_NOTES            VARCHAR2(1000),

    -- Assumptions (free text per section)
    SE_ASSUMPTIONS              VARCHAR2(2000),
    BA_ASSUMPTIONS              VARCHAR2(2000),
    COLLAB_ASSUMPTIONS          VARCHAR2(2000),
    GENERAL_ASSUMPTIONS         VARCHAR2(2000),

    -- Adjusted Hours Comments (aggregated)
    ADJUSTED_HOURS_COMMENTS     VARCHAR2(4000),

    -- Actual Hours Tracking
    TOTAL_ACTUAL_HOURS          NUMBER(10,2)    DEFAULT 0,
    ACTUAL_HOURS_AS_OF_DATE     DATE,

    -- Time for Estimates
    TIME_FOR_ESTIMATES          NUMBER(10,2)    DEFAULT 0,

    -- Test Cases for System Testing (alternative to percentage-based calculation)
    USE_TEST_CASES              NUMBER(1)       DEFAULT 0,
    TEST_CASES_SIMPLE           NUMBER(10,2)    DEFAULT 0,
    TEST_CASES_MEDIUM           NUMBER(10,2)    DEFAULT 0,
    TEST_CASES_COMPLEX          NUMBER(10,2)    DEFAULT 0,
    TEST_CASES_VERY_COMPLEX     NUMBER(10,2)    DEFAULT 0,
    TEST_CASE_ITERATIONS        NUMBER(10,2)    DEFAULT 1,

    -- Audit Fields
    CREATED_DATE                DATE            DEFAULT SYSDATE NOT NULL,
    LAST_MODIFIED_DATE          DATE            DEFAULT SYSDATE NOT NULL,
    CREATED_BY                  VARCHAR2(100),
    VERSION_NUMBER              NUMBER(5)       DEFAULT 1,

    -- Constraints
    CONSTRAINT UQ_PROJECT_NAME UNIQUE (PROJECT_NAME)
);

COMMENT ON TABLE PROJECT_ESTIMATES IS 'Master project estimate table. Each row represents one saved estimate with all summary totals, adjusted hours, notes, assumptions, and audit metadata. Parent table for COMPONENT_ENTRIES and COLLABORATION_ITEMS.';

-- Project Metadata
COMMENT ON COLUMN PROJECT_ESTIMATES.PROJECT_ID IS 'Primary key - GUID string (32 hex chars without hyphens). Auto-generated on insert.';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROJECT_NAME IS 'Unique project name. Required. Used for search/display in History window.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CHANGE_ORDER_ID IS 'Change Order number (e.g., "CO 23327 002"). Used for History search.';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROJECT_DESCRIPTION IS 'Free-text project description, max 500 chars.';
COMMENT ON COLUMN PROJECT_ESTIMATES.ESTIMATED_BY IS 'Name of the person who created/owns this estimate.';
COMMENT ON COLUMN PROJECT_ESTIMATES.REVIEWED_BY IS 'Name of the person who reviewed this estimate.';

-- PM Configuration
COMMENT ON COLUMN PROJECT_ESTIMATES.PM_EFFORT_PERCENTAGE IS 'Project Management effort as percentage of subtotal. Default 15%. Formula: PM_Hours = ROUNDUP(Subtotal × PM%, 2)';
COMMENT ON COLUMN PROJECT_ESTIMATES.PM_RESERVE_PERCENTAGE IS 'Legacy column. Always 0. Kept for backward compatibility with existing databases.';

-- Summary Totals
COMMENT ON COLUMN PROJECT_ESTIMATES.TOTAL_DEVELOPMENT_HOURS IS 'Sum of all component TotalHours from COMPONENT_ENTRIES. Drives all derived calculations.';
COMMENT ON COLUMN PROJECT_ESTIMATES.GRAND_TOTAL_HOURS IS 'Final total hours ROUNDUP to whole number. Includes: Development + SystemTesting + Analysis + BusinessDesign + Promotion + BASystemDoc + ProductionValidation + PM + Collaboration + all Adjusted Hours.';
COMMENT ON COLUMN PROJECT_ESTIMATES.TSHIRT_SIZE IS 'Auto-calculated T-Shirt size based on GrandTotal: XS (≤40), S (≤200), M (≤600), L (≤1000), XL (≤2000), XXL (>2000).';
COMMENT ON COLUMN PROJECT_ESTIMATES.COLLABORATION_HOURS IS 'Total collaboration hours from COLLABORATION_ITEMS. Formula per item: NumMeetings × (Duration/60 + PrepTime/60) × Participants.';

-- Adjusted Hours: SE/Development
COMMENT ON COLUMN PROJECT_ESTIMATES.SE_ADJUSTED_HOURS IS 'Legacy SE adjusted hours. Mapped to DEV_ADJUSTED_HOURS for backward compatibility.';
COMMENT ON COLUMN PROJECT_ESTIMATES.BA_ADJUSTED_HOURS IS 'Legacy BA adjusted hours. Mapped to ANALYSIS_ADJUSTED_HOURS for backward compatibility.';
COMMENT ON COLUMN PROJECT_ESTIMATES.COLLAB_ADJUSTED_HOURS IS 'Overall collaboration section adjusted hours override.';
COMMENT ON COLUMN PROJECT_ESTIMATES.WPRS_ADJUSTED_HOURS IS 'WPR (Walkthroughs/Peer Reviews) collaboration adjusted hours.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CLIENT_MTG_ADJUSTED_HOURS IS 'Client Meetings collaboration adjusted hours.';
COMMENT ON COLUMN PROJECT_ESTIMATES.INTERNAL_MTG_ADJUSTED_HOURS IS 'Internal Meetings collaboration adjusted hours.';
COMMENT ON COLUMN PROJECT_ESTIMATES.AUTO_TEST_ADJUSTED_HOURS IS 'Automation Test Collaboration adjusted hours.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CONSULTANT_ADJUSTED_HOURS IS 'Consultant/Mentor collaboration adjusted hours.';

-- Adjusted Hours: Task-type breakdown
COMMENT ON COLUMN PROJECT_ESTIMATES.DEV_ADJUSTED_HOURS IS 'Development task adjusted hours override for mid-project re-estimation.';
COMMENT ON COLUMN PROJECT_ESTIMATES.ANALYSIS_ADJUSTED_HOURS IS 'Analysis task adjusted hours override. Base = ROUNDUP((Dev + SysTest) × 5%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.BIZ_DESIGN_ADJUSTED_HOURS IS 'Business Design task adjusted hours override. Base = ROUNDUP((Dev + SysTest) × 15%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.SYS_TEST_ADJUSTED_HOURS IS 'System Testing task adjusted hours override. Base = ROUNDUP(Dev × 30%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROMOTION_ADJUSTED_HOURS IS 'Promotion task adjusted hours override. Base = ROUNDUP(Dev × 5%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.BA_DOC_ADJUSTED_HOURS IS 'BA System Documentation adjusted hours override. Base = ROUNDUP(Dev × 5%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROD_VAL_ADJUSTED_HOURS IS 'Production Validation adjusted hours override. Base = ROUNDUP(SysTest × 20%, 2).';
COMMENT ON COLUMN PROJECT_ESTIMATES.PM_ADJUSTED_HOURS IS 'Project Management adjusted hours override. Base = ROUNDUP(Subtotal × PM%, 2).';

-- Adjustment Notes
COMMENT ON COLUMN PROJECT_ESTIMATES.DEV_NOTES IS 'Justification note for Development hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.ANALYSIS_NOTES IS 'Justification note for Analysis hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.BIZ_DESIGN_NOTES IS 'Justification note for Business Design hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.SYS_TEST_NOTES IS 'Justification note for System Testing hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROMOTION_NOTES IS 'Justification note for Promotion hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.BA_DOC_NOTES IS 'Justification note for BA System Doc hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.PROD_VAL_NOTES IS 'Justification note for Production Validation hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.PM_NOTES IS 'Justification note for Project Management hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.WPRS_NOTES IS 'Justification note for WPR collaboration hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CLIENT_MTG_NOTES IS 'Justification note for Client Meetings hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.INTERNAL_MTG_NOTES IS 'Justification note for Internal Meetings hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.AUTO_TEST_NOTES IS 'Justification note for Automation Test collaboration hours adjustment.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CONSULTANT_NOTES IS 'Justification note for Consultant/Mentor hours adjustment.';

-- Assumptions
COMMENT ON COLUMN PROJECT_ESTIMATES.SE_ASSUMPTIONS IS 'SE (Software Engineering) section assumptions text.';
COMMENT ON COLUMN PROJECT_ESTIMATES.BA_ASSUMPTIONS IS 'BA (Business Analyst) section assumptions text.';
COMMENT ON COLUMN PROJECT_ESTIMATES.COLLAB_ASSUMPTIONS IS 'Collaboration section assumptions text.';
COMMENT ON COLUMN PROJECT_ESTIMATES.GENERAL_ASSUMPTIONS IS 'General project assumptions text.';
COMMENT ON COLUMN PROJECT_ESTIMATES.ADJUSTED_HOURS_COMMENTS IS 'Aggregated comments for all adjusted hours (supports up to 4000 chars).';

-- Actual Hours
COMMENT ON COLUMN PROJECT_ESTIMATES.TOTAL_ACTUAL_HOURS IS 'Total actual hours logged (for variance tracking).';
COMMENT ON COLUMN PROJECT_ESTIMATES.ACTUAL_HOURS_AS_OF_DATE IS 'Date through which actual hours have been reported.';

-- Time for Estimates
COMMENT ON COLUMN PROJECT_ESTIMATES.TIME_FOR_ESTIMATES IS 'Hours spent on creating this estimate (meta-tracking).';

-- Test Cases
COMMENT ON COLUMN PROJECT_ESTIMATES.USE_TEST_CASES IS 'Boolean flag (0/1). When 1, system testing uses test case counts instead of percentage formula.';
COMMENT ON COLUMN PROJECT_ESTIMATES.TEST_CASES_SIMPLE IS 'Number of simple test cases (hours multiplier per case).';
COMMENT ON COLUMN PROJECT_ESTIMATES.TEST_CASES_MEDIUM IS 'Number of medium test cases.';
COMMENT ON COLUMN PROJECT_ESTIMATES.TEST_CASES_COMPLEX IS 'Number of complex test cases.';
COMMENT ON COLUMN PROJECT_ESTIMATES.TEST_CASES_VERY_COMPLEX IS 'Number of very complex test cases.';
COMMENT ON COLUMN PROJECT_ESTIMATES.TEST_CASE_ITERATIONS IS 'Number of test iterations/cycles. Default 1.';

-- Audit
COMMENT ON COLUMN PROJECT_ESTIMATES.CREATED_DATE IS 'Date when the estimate was first created.';
COMMENT ON COLUMN PROJECT_ESTIMATES.LAST_MODIFIED_DATE IS 'Date when the estimate was last saved/modified.';
COMMENT ON COLUMN PROJECT_ESTIMATES.CREATED_BY IS 'Windows username of the person who created this estimate.';
COMMENT ON COLUMN PROJECT_ESTIMATES.VERSION_NUMBER IS 'Estimate version number. Incremented on major revisions.';


-- ============================================================================
-- TABLE: COMPONENT_ENTRIES
-- Purpose: Individual component rows for an estimate. Each row represents
--          one development component (e.g., a PB Window, a Report, a Stored Proc).
--          TotalHours = BaseHoursPerUnit × Count
-- Source:  PROMISe Estimating Tool Excel - Component grid rows
-- ============================================================================
CREATE TABLE COMPONENT_ENTRIES (
    COMPONENT_ID            NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PROJECT_ID              VARCHAR2(36)    NOT NULL,
    LINE_NUMBER             NUMBER(5),
    REQUIREMENT_ID          VARCHAR2(50),
    COMPONENT_TYPE          VARCHAR2(50)    NOT NULL,
    DESCRIPTION             VARCHAR2(500),
    CHANGE_TYPE             VARCHAR2(10)    NOT NULL,
    COMPONENT_SIZE          VARCHAR2(10)    NOT NULL,
    COMPONENT_COUNT         NUMBER(5)       DEFAULT 1,
    BASE_HOURS_PER_UNIT     NUMBER(10,4),
    TOTAL_HOURS             NUMBER(10,2),
    NOTES                   VARCHAR2(1000),
    --
    CONSTRAINT FK_COMP_PROJECT FOREIGN KEY (PROJECT_ID)
        REFERENCES PROJECT_ESTIMATES (PROJECT_ID) ON DELETE CASCADE,
    CONSTRAINT CK_COMP_CHANGE_TYPE CHECK (CHANGE_TYPE IN ('New', 'Change')),
    CONSTRAINT CK_COMP_SIZE CHECK (COMPONENT_SIZE IN ('Small', 'Medium', 'Large'))
);

COMMENT ON TABLE COMPONENT_ENTRIES IS 'Individual development component rows for a project estimate. Child of PROJECT_ESTIMATES. TotalHours = BaseHoursPerUnit × ComponentCount. Base hours looked up from WEIGHTED_VALUES table.';

COMMENT ON COLUMN COMPONENT_ENTRIES.COMPONENT_ID IS 'Auto-generated surrogate primary key.';
COMMENT ON COLUMN COMPONENT_ENTRIES.PROJECT_ID IS 'Foreign key to PROJECT_ESTIMATES.PROJECT_ID. Cascade delete.';
COMMENT ON COLUMN COMPONENT_ENTRIES.LINE_NUMBER IS 'Display order/row number in the component grid (1-based).';
COMMENT ON COLUMN COMPONENT_ENTRIES.REQUIREMENT_ID IS 'Optional requirement/story ID reference (e.g., "REQ-001").';
COMMENT ON COLUMN COMPONENT_ENTRIES.COMPONENT_TYPE IS 'Component type name: PowerBuilderWindows, Reports, ProgramsDBStoredProcs, SupportModules, DBManipulation, DatabaseReview, Webpage, K2Workflow, K2SmartForm, TestAutomationUFT, MISC';
COMMENT ON COLUMN COMPONENT_ENTRIES.DESCRIPTION IS 'Free-text description of what this component does.';
COMMENT ON COLUMN COMPONENT_ENTRIES.CHANGE_TYPE IS 'Whether this is a New component or a Change to existing. Values: New, Change.';
COMMENT ON COLUMN COMPONENT_ENTRIES.COMPONENT_SIZE IS 'Size classification. Values: Small, Medium, Large.';
COMMENT ON COLUMN COMPONENT_ENTRIES.COMPONENT_COUNT IS 'Number of units of this component type. Default 1. TotalHours = BaseHours × Count.';
COMMENT ON COLUMN COMPONENT_ENTRIES.BASE_HOURS_PER_UNIT IS 'Hours per single unit, looked up from WEIGHTED_VALUES. Precision to 4 decimals.';
COMMENT ON COLUMN COMPONENT_ENTRIES.TOTAL_HOURS IS 'Calculated: BASE_HOURS_PER_UNIT × COMPONENT_COUNT. Stored for reporting convenience.';
COMMENT ON COLUMN COMPONENT_ENTRIES.NOTES IS 'Optional notes/justification for this component row.';


-- ============================================================================
-- TABLE: COLLABORATION_ITEMS
-- Purpose: Collaboration/meeting items that contribute to total hours.
--          Formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × Participants
-- Source:  PROMISe Estimating Tool Excel - Collaboration Quality sheet
-- ============================================================================
CREATE TABLE COLLABORATION_ITEMS (
    ITEM_ID                     NUMBER(10)      GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PROJECT_ID                  VARCHAR2(36)    NOT NULL,
    LINE_NUMBER                 NUMBER(5),
    TASK_NAME                   VARCHAR2(200),
    COLLABORATION_TYPE          VARCHAR2(50),
    NUMBER_OF_MEETINGS          NUMBER(5)       DEFAULT 1,
    MEETING_DURATION_MINUTES    NUMBER(5)       DEFAULT 60,
    NUMBER_OF_PARTICIPANTS      NUMBER(5)       DEFAULT 3,
    PARTICIPANT_PREP_TIME_MINUTES NUMBER(5)     DEFAULT 15,
    TOTAL_HOURS                 NUMBER(10,2),
    NOTES                       VARCHAR2(1000),
    --
    CONSTRAINT FK_COLLAB_PROJECT FOREIGN KEY (PROJECT_ID)
        REFERENCES PROJECT_ESTIMATES (PROJECT_ID) ON DELETE CASCADE,
    CONSTRAINT CK_COLLAB_TYPE CHECK (COLLABORATION_TYPE IN ('WPRs', 'ClientMeetings', 'InternalMeetings', 'AutomationTestCollaboration'))
);

COMMENT ON TABLE COLLABORATION_ITEMS IS 'Collaboration/meeting items for a project estimate. Child of PROJECT_ESTIMATES. Formula: NumMeetings × (MeetingDuration/60 + PrepTime/60) × NumParticipants = TotalHours.';

COMMENT ON COLUMN COLLABORATION_ITEMS.ITEM_ID IS 'Auto-generated surrogate primary key.';
COMMENT ON COLUMN COLLABORATION_ITEMS.PROJECT_ID IS 'Foreign key to PROJECT_ESTIMATES.PROJECT_ID. Cascade delete.';
COMMENT ON COLUMN COLLABORATION_ITEMS.LINE_NUMBER IS 'Display order/row number in the collaboration grid (1-based).';
COMMENT ON COLUMN COLLABORATION_ITEMS.TASK_NAME IS 'Name/description of the meeting or collaboration activity.';
COMMENT ON COLUMN COLLABORATION_ITEMS.COLLABORATION_TYPE IS 'Category: WPRs, ClientMeetings, InternalMeetings, AutomationTestCollaboration.';
COMMENT ON COLUMN COLLABORATION_ITEMS.NUMBER_OF_MEETINGS IS 'How many meetings/sessions of this type. Default 1.';
COMMENT ON COLUMN COLLABORATION_ITEMS.MEETING_DURATION_MINUTES IS 'Duration of each meeting in minutes. Default 60.';
COMMENT ON COLUMN COLLABORATION_ITEMS.NUMBER_OF_PARTICIPANTS IS 'Number of participants per meeting. Default 3.';
COMMENT ON COLUMN COLLABORATION_ITEMS.PARTICIPANT_PREP_TIME_MINUTES IS 'Preparation time per participant in minutes. Default 15.';
COMMENT ON COLUMN COLLABORATION_ITEMS.TOTAL_HOURS IS 'Calculated: NUM_MEETINGS × (DURATION/60 + PREP_TIME/60) × NUM_PARTICIPANTS. Stored for reporting.';
COMMENT ON COLUMN COLLABORATION_ITEMS.NOTES IS 'Optional notes for this collaboration item.';


-- ============================================================================
-- INDEXES (Performance optimization for common queries)
-- ============================================================================

-- Project lookup by Change Order ID (History search)
CREATE INDEX IDX_PROJECT_CO_ID ON PROJECT_ESTIMATES (CHANGE_ORDER_ID);

-- Project lookup by created date (History sorting)
CREATE INDEX IDX_PROJECT_CREATED ON PROJECT_ESTIMATES (CREATED_DATE DESC);

-- Component entries by project (join performance)
CREATE INDEX IDX_COMP_PROJECT_ID ON COMPONENT_ENTRIES (PROJECT_ID);

-- Collaboration items by project (join performance)
CREATE INDEX IDX_COLLAB_PROJECT_ID ON COLLABORATION_ITEMS (PROJECT_ID);

-- Weighted values lookup (already enforced by unique constraint, but explicit for clarity)
CREATE INDEX IDX_WV_LOOKUP ON WEIGHTED_VALUES (COMPONENT_TYPE, SIZE, CHANGE_TYPE);


-- ============================================================================
-- CALCULATION REFERENCE (Application Layer Logic)
-- ============================================================================
-- These formulas are NOT stored as computed columns — they are calculated in the
-- application and persisted to GRAND_TOTAL_HOURS for reporting/querying.
--
-- Development Hours     = SUM(COMPONENT_ENTRIES.TOTAL_HOURS) for the project
-- System Testing        = CEIL(Development × 0.30 * 100) / 100     -- ROUNDUP to 2 decimals
-- Analysis              = CEIL((Development + SystemTesting) × 0.05 * 100) / 100
-- Business Design       = CEIL((Development + SystemTesting) × 0.15 * 100) / 100
-- Promotion             = CEIL(Development × 0.05 * 100) / 100
-- BA System Doc         = CEIL(Development × 0.05 * 100) / 100
-- Production Validation = CEIL(SystemTesting × 0.20 * 100) / 100
-- Collaboration         = SUM(COLLABORATION_ITEMS.TOTAL_HOURS) for the project
-- Subtotal              = All above + all Adjusted Hours
-- PM Effort             = CEIL(Subtotal × PM_EFFORT_PERCENTAGE/100 * 100) / 100
-- Grand Total           = CEIL(Subtotal + PM Effort)  -- rounded UP to whole number
--
-- T-Shirt Size Mapping:
--   XS: Grand Total ≤ 40
--   S:  Grand Total ≤ 200
--   M:  Grand Total ≤ 600
--   L:  Grand Total ≤ 1000
--   XL: Grand Total ≤ 2000
--   XXL: Grand Total > 2000
-- ============================================================================


-- ============================================================================
-- GRANTS (adjust schema/role names as needed for your Oracle environment)
-- ============================================================================
-- GRANT SELECT, INSERT, UPDATE, DELETE ON WEIGHTED_VALUES TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON DETAILED_SE_WEIGHTED_VALUES TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON DETAILED_BA_WEIGHTED_VALUES TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON EXPERIENCE_LEVELS TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON PROJECT_ESTIMATES TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON COMPONENT_ENTRIES TO estimation_app_role;
-- GRANT SELECT, INSERT, UPDATE, DELETE ON COLLABORATION_ITEMS TO estimation_app_role;
--
-- Read-only reporting role:
-- GRANT SELECT ON PROJECT_ESTIMATES TO estimation_report_role;
-- GRANT SELECT ON COMPONENT_ENTRIES TO estimation_report_role;
-- GRANT SELECT ON COLLABORATION_ITEMS TO estimation_report_role;


-- ============================================================================
-- END OF SCHEMA SCRIPT
-- ============================================================================
