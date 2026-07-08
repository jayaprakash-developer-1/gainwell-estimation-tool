# Gainwell Estimation Tool

[![Build](https://img.shields.io/badge/build-passing-brightgreen)](.) [![Tests](https://img.shields.io/badge/tests-930%20passing-brightgreen)](.) [![.NET](https://img.shields.io/badge/.NET-10.0-blue)](.) [![License](https://img.shields.io/badge/license-proprietary-red)](.)

A WPF desktop application for component-based project effort estimation. Replicates and extends the Excel **"PROMISe Estimating Tool"** with **Initial Estimate** and **Detailed Estimate** workflows, producing results verified to match the Excel reference file exactly.

---

## Features

### Initial Estimate
- **Component-Based Estimation** — Add components (PowerBuilder Windows, Reports, Stored Procs, Webpages, K2 Workflows, etc.) with size/change-type to auto-calculate base hours from 66 weighted values
- **Automatic Task Derivation** — System Testing (30%), Analysis (5%), Business Design (15%), Promotion (5%), BA/System Doc (5%), Production Validation (20%) — all using Excel `ROUNDUP(x, 2)` semantics
- **Test Case-Based Estimation** — Alternative system testing formula using Simple/Medium/Complex/Very Complex test case counts with configurable iteration multiplier (supports decimal iterations)
- **Project Management Effort** — Configurable PM% (1–20%) applied to cascading effective task totals
- **Collaboration Tracking** — WPRs, Client Meetings, Internal Meetings, Automation Test Collaboration with per-type adjusted hours
- **Adjusted Hours** — Mid-project re-estimation with per-task-type adjustments, notes, and cascading recalculation
- **Role Breakout** — BA, SE, Tester, PM, Collaboration hour distribution matching Excel rows 47–51
- **T-Shirt Sizing** — Automatic classification (Small → XL8) based on grand total hours

### Detailed Estimate
- **SE Component Matrix** — Per-phase hours (Analysis, Technical Design, Code Construction, etc.) by component type, status (New/Existing), and complexity (Simple/Moderate/Complex)
- **BA Estimation** — BDD Creation, System Testing, Production Validation task breakdowns
- **Experience Level Multipliers** — New to Area, Proficient, Expert adjustments
- **Collaboration/Quality Sheet** — Separate meeting hours and prep hours with consultant tracking

### General
- **Project Persistence** — Save/Load via SQLite with full round-trip fidelity
- **Project History** — Browse, filter, and reload past estimates
- **Settings UI** — Edit all 66 weighted values directly in-app
- **Keyboard Shortcuts** — Ctrl+S (Save), Ctrl+N (New Component), Ctrl+Z (Undo), Delete
- **Gainwell Brand Compliance** — Full adherence to Gainwell Style Guide 2024

---

## Calculation Verification

All formulas are verified against the Excel reference file `CO 23327 002 Final Estimate V1.0.xlsm`:

| Metric | Excel Value | Tool Output | Status |
|--------|------------|-------------|--------|
| Development | 596.5625 | 596.5625 | ✅ |
| System Testing (test-case, 2.5 iter) | 2,517.46 | 2,517.46 | ✅ |
| Analysis | 155.71 | 155.71 | ✅ |
| Business Design | 467.11 | 467.11 | ✅ |
| Promotion | 29.83 | 29.83 | ✅ |
| BA System Doc (+1.17 adj) | 31.00 | 31.00 | ✅ |
| Production Validation | 503.50 | 503.50 | ✅ |
| PM Effort (15%) | 645.18 | 645.18 | ✅ |
| Collaboration | 185.75 | 185.75 | ✅ |
| Subtotal | 5,261.11 | 5,261.11 | ✅ |
| **Grand Total** | **5,262** | **5,262** | ✅ |
| T-Shirt Size | XL5 | XL5 | ✅ |

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| UI Framework | WPF (.NET 10) |
| Architecture | MVVM (CommunityToolkit.Mvvm 8.4) |
| Data Access | Entity Framework Core 10 + SQLite |
| Testing | xUnit 2.9 + Coverlet |
| Build | MSBuild / `dotnet` CLI |

---

## Project Structure

```
gainwell-estimation-tool/
├── Gainwell.EstimationTool.sln
├── src/
│   └── Gainwell.EstimationTool/          — Main WPF application
│       ├── Models/                       — Domain models and enums
│       ├── ViewModels/                   — MVVM ViewModels (calculation engine)
│       ├── Views/                        — WPF Windows and Dialogs
│       ├── Data/                         — EF Core DbContext, seeders, weighted values
│       ├── Converters/                   — WPF value converters
│       └── Assets/                       — App icon, Gainwell logo
├── tests/
│   └── Gainwell.EstimationTool.Tests/    — 930 xUnit tests
├── tools/
│   └── ExcelReader/                      — Excel data extraction utility
├── docs/
│   ├── reference/                        — Excel reference files for verification
│   ├── style-guide/                      — Gainwell Brand Style Guide 2024
│   └── oracle_schema.sql                 — Oracle DB schema (future migration)
└── llm-bridge/                           — AI integration utilities
```

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Windows 10/11 (WPF requirement)
- Visual Studio 2022 17.12+ or VS Code with C# Dev Kit

---

## Quick Start

```powershell
# Clone
git clone https://github.com/Shoaib-Gainwell/gainwell-estimation-tool.git
cd gainwell-estimation-tool

# Build
dotnet build

# Run
dotnet run --project src/Gainwell.EstimationTool

# Test (930 tests)
dotnet test
```

---

## Test Coverage

| Test Suite | Tests | Coverage Area |
|-----------|-------|---------------|
| ExcelVerificationTests | 27 | Exact Excel data match (CO 23327 002) |
| FullCalculationPipelineTests | 31 | End-to-end formula pipeline |
| WeightedValuesTests | 131 | All 66 base hour lookups |
| RoundUpAndTShirtTests | 71 | ROUNDUP semantics + T-shirt boundaries |
| DetailedCollaborationQualityTests | 79 | Detailed estimate collab formulas |
| InitialEstimateAndDtlCollabCoverageTests | 70 | Role breakout, PM%, subtotals |
| AdjustedHoursTests | — | Per-task adjustment cascading |
| CollaborationTests | — | Collaboration formula edge cases |
| PersistenceRoundTripTests | — | Save/Load fidelity |
| *+ 15 more test classes* | — | Converters, DB, boundaries, negatives |
| **Total** | **930** | **0 failures** |

---

## Component Types Supported

| Component | Sizes | Change Types |
|-----------|-------|-------------|
| PowerBuilder Windows | S / M / L | New / Change |
| Reports | S / M / L | New / Change |
| Programs/DB Stored Procs | S / M / L | New / Change |
| Support Modules / JOB / JIL | S / M / L | New / Change |
| DB Manipulation (SQL, PL/SQL) | S / M / L | New / Change |
| Database Review | S / M / L | New / Change |
| Webpage (UI, Portal, Intranet) | S / M / L | New / Change |
| K2 Workflow | S / M / L | New / Change |
| K2 Smart Form | S / M / L | New / Change |
| Test Automation Suites (UFT) | S / M / L | New / Change |
| MISC (Server/Software Setup) | S / M / L | New / Change |

---

## Key Formulas

```
System Testing   = ROUNDUP(Development × 30%, 2)
Analysis         = ROUNDUP((Development + System Testing) × 5%, 2)
Business Design  = ROUNDUP((Development + System Testing) × 15%, 2)
Promotion        = ROUNDUP(Development × 5%, 2)
BA System Doc    = ROUNDUP(Development × 5%, 2)
Prod Validation  = ROUNDUP(System Testing × 20%, 2)
PM Effort        = ROUNDUP(AllEffectiveTasks × PM%, 2)
Collaboration    = Meetings × (Duration/60 + PrepTime/60) × Participants
Grand Total      = ⌈Subtotal⌉  (ceiling to whole number)
```

---

## Contributors

- Shoaib Mohammed — Lead Developer & Architect
- Gainwell Technologies Estimation Team

---

## License

**Proprietary** — Internal use only. © Gainwell Technologies LLC.
