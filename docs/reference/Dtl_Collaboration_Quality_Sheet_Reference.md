# Dtl Collaboration_Quality Sheet — Complete Technical Reference

## Overview

The **"Dtl Collaboration_Quality"** sheet is part of the PROMISe Estimation Tool (Template Version 2.9). It calculates **total collaboration and quality hours** for a detailed estimate. Unlike the InitialEstimate sheet (which uses a combined formula per collaboration type), this sheet separates meeting hours and prep hours into **distinct rows** and uses **1-hour fixed meeting duration** hardcoded in formulas.

**Key difference from InitialEstimate:**
- InitialEstimate: `Meetings × (DurationMinutes/60 + PrepMinutes/60) × Participants` (combined per type)
- Dtl Collaboration_Quality: Separate rows for meeting time and prep time, with meeting duration **fixed at 1 hour**

---

## Sheet Layout (Row-by-Row)

### Header Section (Rows 1–9)

| Row | Col A | Col B | Col C | Notes |
|-----|-------|-------|-------|-------|
| 1 | Template Version: | 2.9 | | Metadata |
| 2 | Collaboration - Quality | | | Sheet title |
| 3 | *(empty)* | | | Spacer |
| 4 | Change Order | CO 23327 002 | | Links to project ID |
| 5 | Instructions: | *"All fields highlighted in yellow..."* | | Guidance text |
| 6 | *(empty)* | | | Spacer |
| 7 | Estimate By: | BA Virginia Innerst | | Who created estimate |
| 8–9 | *(empty)* | | | Spacers |

### Column Headers (Row 10)

| Column | Header | Purpose |
|--------|--------|---------|
| A (Col 1) | Task | Description of the collaboration task |
| B (Col 2) | Count | Number of meetings/WPRs |
| C (Col 3) | Hours | Prep hours per person (for prep rows) |
| D (Col 4) | *(used for labels/subtotals)* | "Avg # of Attendees" label, or consultant/estimate hours |
| E (Col 5) | # of Attendees | Number of participants per meeting |
| F (Col 6) | *(empty)* | |
| G (Col 7) | Hour Total | Calculated meeting/prep hours |
| H (Col 8) | Adjusted Hrs | Manual adjustments (+/-) |
| I (Col 9) | Grand Total | HourTotal + AdjustedHrs |

### Row 11 — Spacer (empty)

---

## Total Summary Row (Row 12)

| Cell | Formula | Value | Description |
|------|---------|-------|-------------|
| A12 | "Total Summary" | | Label |
| **G12** | `=SUM(G14:G21,D23,D31,D35)` | **2240.00** | Sum of ALL calculated hours (meetings + prep + consultants + estimates + PM) |
| **H12** | `=SUM(H14:H21)` | **0.00** | Sum of ONLY meeting/prep adjusted hours (does NOT include consultant/estimates/PM adjustments) |
| **I12** | `=SUM(I14:I21,D23,D31,D35)` | **2240.00** | Sum of line grand totals + consultant + estimates + PM |

### Critical Formula Insight:
- **G12** includes: Meeting hours (G14:G21) + Consultant (D23) + Estimates (D31) + PM (D35)
- **H12** includes: ONLY meeting adjusted hours (H14:H21) — consultants, estimates, and PM have NO adjusted column
- **I12** includes: Meeting grand totals (I14:I21) + Consultant (D23) + Estimates (D31) + PM (D35)

---

## Meeting Sections (Rows 13–21)

### Section 1: Walkthrough/Work Product Reviews (WPR)

#### Row 13 — Spacer (empty)

#### Row 14 — WPR Meetings

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A14 | Label | "Meeting Walkthrough/Work Product Reviews..." | Full task description with instructions |
| **B14** | 🟡 INPUT | 25 | Number of WPR meetings |
| D14 | Label | "Avg # of Attendees" | Column label |
| **E14** | 🟡 INPUT | 6 | Average number of attendees |
| **G14** | FORMULA: `=B14*1*E14` | 150.00 | **Count × 1hr × Attendees** |
| **H14** | 🟡 INPUT | 0 | Adjusted hours |
| **I14** | FORMULA: `=G14+H14` | 150.00 | HourTotal + Adjusted |

**Formula explanation for G14:** `=B14*1*E14` → 25 × **1** × 6 = 150
- The **`*1*`** is a hardcoded multiplier meaning **each meeting = 1 hour**
- This is NOT configurable — meeting duration is always assumed to be 1 hour

#### Row 15 — WPR Preparation

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A15 | Label | "Walkthrough/Work Product Review Preparation per person" | |
| **C15** | 🟡 INPUT | 0.50 | Prep hours per person per meeting |
| **G15** | FORMULA: `=C15*E14*B14` | 75.00 | **PrepHrs × Attendees × Count** |
| **H15** | 🟡 INPUT | 0 | Adjusted hours |
| **I15** | FORMULA: `=G15+H15` | 75.00 | HourTotal + Adjusted |

**Formula explanation for G15:** `=C15*E14*B14` → 0.50 × 6 × 25 = 75
- Uses **E14** (attendees from meeting row) and **B14** (count from meeting row)
- PrepHrs is per person per meeting

#### Row 16 — Spacer (empty)

---

### Section 2: Client Meetings

#### Row 17 — Client Meetings

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A17 | Label | "Client Meetings..." | |
| **B17** | 🟡 INPUT | 5 | Number of client meetings |
| D17 | Label | "Avg # of Attendees" | |
| **E17** | 🟡 INPUT | 4 | Average attendees |
| **G17** | FORMULA: `=B17*1*E17` | 20.00 | **Count × 1hr × Attendees** |
| **H17** | 🟡 INPUT | 0 | Adjusted hours |
| **I17** | FORMULA: `=G17+H17` | 20.00 | |

**Formula:** 5 × 1 × 4 = 20

#### Row 18 — Client Meeting Preparation

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A18 | Label | "Client Meeting Preparation per person" | |
| **C18** | 🟡 INPUT | 0.25 | Prep hours per person per meeting |
| **G18** | FORMULA: `=C18*E17*B17` | 5.00 | **PrepHrs × Attendees × Count** |
| **H18** | 🟡 INPUT | 0 | Adjusted hours |
| **I18** | FORMULA: `=G18+H18` | 5.00 | |

**Formula:** 0.25 × 4 × 5 = 5

#### Row 19 — Spacer (empty)

---

### Section 3: Internal Meetings

#### Row 20 — Internal Meetings

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A20 | Label | "Internal Meetings..." | |
| **B20** | 🟡 INPUT | 24 | Number of internal meetings |
| D20 | Label | "Avg # of Attendees" | |
| **E20** | 🟡 INPUT | 6 | Average attendees |
| **G20** | FORMULA: `=B20*1*E20` | 144.00 | **Count × 1hr × Attendees** |
| **H20** | 🟡 INPUT | 0 | Adjusted hours |
| **I20** | FORMULA: `=G20+H20` | 144.00 | |

**Formula:** 24 × 1 × 6 = 144

#### Row 21 — Internal Meeting Preparation

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A21 | Label | "Internal Meeting Preparation per person" | |
| **C21** | 🟡 INPUT | 0.25 | Prep hours per person per meeting |
| **G21** | FORMULA: `=C21*E20*B20` | 36.00 | **PrepHrs × Attendees × Count** |
| **H21** | 🟡 INPUT | 0 | Adjusted hours |
| **I21** | FORMULA: `=G21+H21` | 36.00 | |

**Formula:** 0.25 × 6 × 24 = 36

#### Row 22 — Spacer (empty)

---

## Consultant/Mentor Section (Rows 23–28)

| Row | Col A | Col D | Notes |
|-----|-------|-------|-------|
| 23 | "Consultant/Mentor Effort" | **375.00** | `=SUM(D24:D28)` — Total of all persons |
| 24 | Person 1 Mario Castillon | 🟡 **100.00** | Direct hour input |
| 25 | Person 2 Tracey Elias | 🟡 **100.00** | Direct hour input |
| 26 | Person 3 Virginia Innerst | 🟡 **100.00** | Direct hour input |
| 27 | Person 4 Madeline Woodward | 🟡 **75.00** | Direct hour input |
| 28 | Person 5 | 🟡 **0** (displays as "-") | No consultant assigned |

### Key Formula:
```
D23 = SUM(D24:D28) = 100 + 100 + 100 + 75 + 0 = 375
```

**Important notes:**
- Up to 5 consultant slots are available in the template
- Consultant names and hours are free-form input (yellow cells)
- Consultant total (D23) feeds directly into G12 and I12
- There is NO adjusted hours column for consultants — only direct hour input

---

## Rows 29–30 — Spacers (empty)

---

## Create Estimates Section (Rows 31–33)

| Row | Col A | Col D | Notes |
|-----|-------|-------|-------|
| 31 | "Create Detailed and Final Estimates" | **35.00** | `=SUM(D32:D33)` — Total of both estimates |
| 32 | "Create Detail Estimate (hr)" | 🟡 **25.00** | Hours to build detailed estimate |
| 33 | "Create Final Estimate (hr)" | 🟡 **10.00** | Hours to build final estimate |

### Key Formula:
```
D31 = SUM(D32:D33) = 25 + 10 = 35
```

### Cell Comments (from Excel):
- **A32:** *"Insert the amount of time it takes to develop the detailed estimate (reviewing the design and doing detailed analysis). The detailed estimate will be populated on the EstimateSummary worksheet based on the data entered by the SE and BA on the respective worksheets in this workbook. Team review of the estimate should be included on the Collaboration tab."*
- **A33:** *"Insert the amount of time it takes to develop the final estimate (reviewing the design and doing detailed analysis). The final estimate will be populated on the EstimateSummary worksheet based on the data entered by the SE and BA on the respective worksheets in this workbook. Team review of the estimate should be included on the Collaboration tab."*

**Important:** Estimates total (D31) feeds directly into G12 and I12.

---

## Row 34 — Spacer (empty)

---

## Project Manager Effort (Row 35)

| Cell | Type | Value | Description |
|------|------|-------|-------------|
| A35 | Label | "Project Manager Effort — Insert the number of PM hours required." | |
| **D35** | 🟡 INPUT | **1,400.00** | Direct PM hours input (has validation) |

### Key Points:
- PM hours in the Detailed sheet are a **DIRECT INPUT** (not a percentage like the InitialEstimate sheet)
- D35 has validation (restricts input range)
- The formula in D35 is just the literal value `1400` (no calculation)
- PM total feeds directly into G12 and I12

**Critical difference from InitialEstimate:** In InitialEstimate, PM = `ROUNDUP(allTasks × PM%/100)`. In Dtl Collaboration_Quality, PM is a flat number input by the user.

---

## Rows 36–38 — Spacers (empty)

---

## Assumptions Section (Row 39)

| Cell | Type | Content |
|------|------|---------|
| A39 | Label | "Assumptions: Add assumptions that led to the numbers in the estimate." |
| **C39** | 🟡 INPUT | Multi-line free text (620 characters in this example) |

### Example Assumptions Text:
```
Meeting Walkthrough/Work Product Reviews: WPR for Test Plans, Test Results, 
Detailed Estimate, TDDs, Codes, and Prod Vals for T-MSIS CIP, CLT, COT, CRX, FTX files. 
Client Meetings: Review Test Results for T-MSIS CIP, CLT, COT, CRX, FTX files. 
Internal Meetings: Testing Status Meetings and any issue resolution discussions.
Consultant/Mentor Effort: Testing support 

Increase in collaboration from the Initial Estimate to the Detailed Estimate 
is the addition of consultants (field not available in estimate template when 
Initial Estimate was done) and increased PM hours due to the increase in 
overall scope of work.
```

**Important:** This is narrative text only — does NOT affect calculations.

---

## Rows 40–46 — Spacers (empty)

---

## Additional Hours Notes Section (Row 47)

| Cell | Type | Content |
|------|------|---------|
| A47 | Label | "*Additional Hours Notes (i.e. BRD revisions and approvals): Add justification for adjusted hours." |
| **C47** | 🟡 INPUT | Free text — justification for any values in the "Adjusted Hrs" column |

**Important:** This is narrative text only — does NOT affect calculations. It serves as documentation for auditors/reviewers to understand WHY adjusted hours were applied.

---

## Rows 48–50 — Spacers (empty, end of sheet)

---

## Complete Formula Reference

### Meeting Hour Formulas (Column G)

| Cell | Formula | Meaning | Result |
|------|---------|---------|--------|
| G14 | `=B14*1*E14` | WPR_Count × 1hr × WPR_Attendees | 25×1×6 = **150** |
| G15 | `=C15*E14*B14` | WPR_PrepHrs × WPR_Attendees × WPR_Count | 0.5×6×25 = **75** |
| G17 | `=B17*1*E17` | Client_Count × 1hr × Client_Attendees | 5×1×4 = **20** |
| G18 | `=C18*E17*B17` | Client_PrepHrs × Client_Attendees × Client_Count | 0.25×4×5 = **5** |
| G20 | `=B20*1*E20` | Internal_Count × 1hr × Internal_Attendees | 24×1×6 = **144** |
| G21 | `=C21*E20*B20` | Internal_PrepHrs × Internal_Attendees × Internal_Count | 0.25×6×24 = **36** |

### Grand Total Per Line (Column I)

| Cell | Formula | Meaning |
|------|---------|---------|
| I14 | `=G14+H14` | WPR Meeting Hours + WPR Meeting Adjusted |
| I15 | `=G15+H15` | WPR Prep Hours + WPR Prep Adjusted |
| I17 | `=G17+H17` | Client Meeting Hours + Client Meeting Adjusted |
| I18 | `=G18+H18` | Client Prep Hours + Client Prep Adjusted |
| I20 | `=G20+H20` | Internal Meeting Hours + Internal Meeting Adjusted |
| I21 | `=G21+H21` | Internal Prep Hours + Internal Prep Adjusted |

### Subtotal Formulas (Column D)

| Cell | Formula | Meaning | Result |
|------|---------|---------|--------|
| D23 | `=SUM(D24:D28)` | Sum of all consultant hours | **375** |
| D31 | `=SUM(D32:D33)` | Detail Estimate + Final Estimate hours | **35** |
| D35 | `1400` (direct input) | PM hours | **1400** |

### Overall Totals (Row 12)

| Cell | Formula | Meaning | Result |
|------|---------|---------|--------|
| **G12** | `=SUM(G14:G21,D23,D31,D35)` | All calculated hours | **2240** |
| **H12** | `=SUM(H14:H21)` | Only meeting/prep adjusted hours | **0** |
| **I12** | `=SUM(I14:I21,D23,D31,D35)` | All grand totals | **2240** |

---

## Input Fields Summary (All Yellow Cells — BgColorIdx=36)

| Cell | Field | Sample Value | Validation |
|------|-------|------|------------|
| B14 | WPR Meeting Count | 25 | ✅ Has validation |
| E14 | WPR Attendees | 6 | ✅ Has validation |
| C15 | WPR Prep Hours/Person | 0.50 | ✅ Has validation |
| B17 | Client Meeting Count | 5 | ✅ Has validation (assumed) |
| E17 | Client Attendees | 4 | ✅ Has validation (assumed) |
| C18 | Client Prep Hours/Person | 0.25 | ✅ Has validation (assumed) |
| B20 | Internal Meeting Count | 24 | ✅ Has validation (assumed) |
| E20 | Internal Attendees | 6 | ✅ Has validation (assumed) |
| C21 | Internal Prep Hours/Person | 0.25 | ✅ Has validation (assumed) |
| D24 | Person 1 Hours | 100 | No validation |
| D25 | Person 2 Hours | 100 | No validation |
| D26 | Person 3 Hours | 100 | No validation |
| D27 | Person 4 Hours | 75 | No validation |
| D28 | Person 5 Hours | 0 | No validation |
| D32 | Create Detail Estimate Hours | 25 | No validation |
| D33 | Create Final Estimate Hours | 10 | No validation |
| D35 | PM Hours | 1400 | ✅ Has validation |
| H14 | WPR Meeting Adjusted | 0 | No validation |
| H15 | WPR Prep Adjusted | 0 | No validation |
| H17 | Client Meeting Adjusted | 0 | No validation |
| H18 | Client Prep Adjusted | 0 | No validation |
| H20 | Internal Meeting Adjusted | 0 | No validation |
| H21 | Internal Prep Adjusted | 0 | No validation |
| C39 | Assumptions Text | *(multi-line)* | Free text |
| C47 | Additional Hours Notes | *(empty)* | Free text |

**Total input fields: 24**

---

## Weighted Values / Lookup Tables

**NONE** — The Dtl Collaboration_Quality sheet does **NOT** use any weighted value lookup tables. All calculations are purely arithmetic based on user inputs. The "IntialEstWeightedValues" and "DetailedEstWeightedValues" sheets are used only by the component/SE/BA sheets.

---

## Calculation Flow Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                    INPUT CELLS (Yellow)                          │
├─────────────────────────────────────────────────────────────────┤
│  WPR:      Count=B14, Attendees=E14, PrepHrs=C15                │
│  Client:   Count=B17, Attendees=E17, PrepHrs=C18                │
│  Internal: Count=B20, Attendees=E20, PrepHrs=C21                │
│  Consultants: D24, D25, D26, D27, D28                           │
│  Estimates: D32, D33                                            │
│  PM: D35                                                        │
│  Adjusted: H14, H15, H17, H18, H20, H21                        │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              MEETING HOUR CALCULATIONS (Col G)                   │
├─────────────────────────────────────────────────────────────────┤
│  G14 = B14 × 1 × E14    (Count × 1hr × Attendees)    = 150     │
│  G15 = C15 × E14 × B14  (PrepHrs × Attendees × Count) = 75     │
│  G17 = B17 × 1 × E17                                  = 20     │
│  G18 = C18 × E17 × B17                                = 5      │
│  G20 = B20 × 1 × E20                                  = 144    │
│  G21 = C21 × E20 × B20                                = 36     │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│           LINE GRAND TOTALS (Col I = Col G + Col H)             │
├─────────────────────────────────────────────────────────────────┤
│  I14 = G14 + H14 = 150 + 0 = 150                               │
│  I15 = G15 + H15 = 75 + 0 = 75                                 │
│  I17 = G17 + H17 = 20 + 0 = 20                                 │
│  I18 = G18 + H18 = 5 + 0 = 5                                   │
│  I20 = G20 + H20 = 144 + 0 = 144                               │
│  I21 = G21 + H21 = 36 + 0 = 36                                 │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│              SUBTOTALS (Col D)                                   │
├─────────────────────────────────────────────────────────────────┤
│  D23 = SUM(D24:D28) = 100+100+100+75+0         = 375           │
│  D31 = SUM(D32:D33) = 25+10                    = 35            │
│  D35 = 1400 (direct input)                     = 1400           │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│           OVERALL TOTALS (Row 12)                               │
├─────────────────────────────────────────────────────────────────┤
│  G12 = SUM(G14:G21) + D23 + D31 + D35                          │
│      = (150+75+20+5+144+36) + 375 + 35 + 1400                  │
│      = 430 + 375 + 35 + 1400 = 2240                            │
│                                                                  │
│  H12 = SUM(H14:H21)     ← ONLY meeting adjusted hours          │
│      = 0+0+0+0+0+0 = 0                                          │
│                                                                  │
│  I12 = SUM(I14:I21) + D23 + D31 + D35                          │
│      = (150+75+20+5+144+36) + 375 + 35 + 1400                  │
│      = 430 + 375 + 35 + 1400 = 2240                            │
└─────────────────────────────────────────────────────────────────┘
```

---

## Key Design Decisions & Nuances

### 1. Meeting Duration is Hardcoded at 1 Hour
The formula `=B14*1*E14` has a literal `1` meaning every meeting = 1 hour. There is **no input cell** for meeting duration. If a meeting is 2 hours, you increase the Count instead.

### 2. Prep Row Reuses Meeting Row's Attendees and Count
- G15 uses `E14` (attendees from Row 14) and `B14` (count from Row 14)
- G18 uses `E17` and `B17`  
- G21 uses `E20` and `B20`

This means the prep calculation assumes the same number of attendees and same number of meetings need prep.

### 3. Adjusted Hours (Column H) Only Apply to Meeting/Prep Rows
- H12 = `SUM(H14:H21)` — only 6 cells
- Consultants, Estimates, and PM have **no adjusted hours column**
- If you need to adjust consultant hours, you change D24:D28 directly

### 4. PM is a Direct Input (Not a Percentage)
Unlike the InitialEstimate sheet where PM = `ROUNDUP(allTasks × PM%/100)`, here PM (D35) is simply the number of hours the PM enters. No calculation derives it.

### 5. No Weighted Values Used
This sheet is 100% user-input-driven. No lookups, no experience multipliers, no component-type matrices.

### 6. Consultant Slots are Fixed at 5
Rows 24–28 allow up to 5 consultant/mentor entries. The formula `D23=SUM(D24:D28)` is hardcoded to this range.

### 7. Number Format
- Calculated cells (G, H, I columns): Display as `0.00` or accounting format `_(* #,##0.00_)`
- This means values like 185.75 display correctly with 2 decimal places

### 8. Grand Total Formula Asymmetry
```
G12 = SUM(G14:G21, D23, D31, D35)  ← includes consultants, estimates, PM
H12 = SUM(H14:H21)                  ← does NOT include consultants, estimates, PM
I12 = SUM(I14:I21, D23, D31, D35)  ← includes consultants, estimates, PM
```
This means: `I12 ≠ G12 + H12` when there are adjusted hours. Instead: `I12 = (G14+H14 + G15+H15 + ... + G21+H21) + D23 + D31 + D35`

---

## Comparison: Dtl Collaboration_Quality vs InitialEstimate Collaboration Section

| Feature | InitialEstimate | Dtl Collaboration_Quality |
|---------|----------------|--------------------------|
| Meeting duration | User input (minutes) | Hardcoded 1 hour |
| Prep time | Combined in same formula | Separate row per meeting type |
| Formula per meeting type | `Meetings × (Dur/60 + Prep/60) × Participants` | Meeting: `Count × 1 × Attendees` |
| Formula for prep | *(included above)* | `PrepHrs × Attendees × Count` |
| Consultant/Mentor | Only adjusted hours (no slots) | 5 named person slots with hours |
| PM Effort | Percentage of all tasks | Direct hour input |
| Create Estimates | Single "Time for Estimates" field | Split: Detail + Final |
| Adjusted Hours | Per collaboration type | Per meeting/prep row (6 cells) |
| Total Actual Hours | Has field | Not present |
| Number of meeting types | 4 (WPR, Client, Internal, Automation) | 3 (WPR, Client, Internal) |
| Automation Test Collab | Present | NOT present |

---

## Validation Rules (from Excel)

The following cells have Excel Data Validation (restricts what users can enter):
- **B14, E14, C15** — WPR inputs (likely integer ranges)
- **D35** — PM hours (likely minimum/maximum range)

---

## Test Coverage Mapping

Every formula and input cell is verified in the test suite:

| Excel Element | Test File | Test Method(s) |
|---------------|-----------|----------------|
| G14 formula (Count×1×Att) | `DetailedCollaborationQualityTests` | `MeetingHours_CountTimesHoursTimesAttendees` |
| G15 formula (Prep×Att×Count) | `DetailedCollaborationQualityTests` | `PrepHours_PrepTimesAttendeesTimesCount` |
| I(row) = G + H | `DetailedCollaborationQualityTests` | `GrandTotalPerLine_HourTotalPlusAdjusted` |
| D23 = SUM(consultants) | `DetailedCollaborationQualityTests` | `ConsultantTotal_SumOfAllPersons_MatchesExcel` |
| D31 = Detail + Final | `DetailedCollaborationQualityTests` | `EstimatesTotal_DetailPlusFinal` |
| G12 overall total | `DetailedCollaborationQualityTests` | `OverallHourTotal_MatchesExcel_Exactly` |
| H12 adjusted total | `DetailedCollaborationQualityTests` | `OverallAdjusted_SumOfMeetingAdjustmentsOnly` |
| I12 grand total | `DetailedCollaborationQualityTests` | `OverallGrandTotal_MatchesExcel_NoAdjustments` |
| Full pipeline (2240) | `DetailedCollaborationQualityTests` | `FullPipeline_ExcelExactValues_GrandTotal2240` |
| PM as direct input | `InitialEstimateAndDtlCollabCoverageTests` | `DtlCollab_PMEffort_IsDirectHoursInput` |
| >5 consultants | `InitialEstimateAndDtlCollabCoverageTests` | `DtlCollab_SixConsultants_SumIsCorrect` |
| Meeting vs Prep separate rows | `InitialEstimateAndDtlCollabCoverageTests` | `DtlCollab_MeetingAndPrep_OnSeparateRows` |
| Negative adjusted | `DetailedCollaborationQualityTests` | `AdjustedHours_AllNegative_DecreasesGrandTotal` |
| Zero inputs | `DetailedCollaborationQualityTests` | `OverallHourTotal_AllZeros_ReturnsZero` |
| Large values | `DetailedCollaborationQualityTests` | `LargeValues_NoOverflow` |
| ConsultantRow model | `DetailedCollaborationQualityTests` | `ConsultantRow_*` tests |
| Assumptions field | `InitialEstimateAndDtlCollabCoverageTests` | `DtlCollab_Assumptions_LongMultiLineText` |
| Additional Notes field | `InitialEstimateAndDtlCollabCoverageTests` | `DtlCollab_AdditionalHoursNotes_IsJustification` |
