# Copilot Studio Health Monitor — Test Plan

**Plugin version:** All 5 phases complete  
**Date:** 2026-06-14  
**XrmToolBox version:** v1.2025.10.74

---

## 1. Test Scope

| Layer | Coverage |
|---|---|
| Models (computed properties) | Automated unit tests |
| Services (business logic) | Automated unit tests with mocked IOrganizationService |
| Controls + PluginControl (UI, events) | Manual functional tests (requires XrmToolBox + live Dataverse) |
| Integration (real Dataverse) | Manual tests (see §6) |

---

## 2. Unit Test Project

**Location:** `CopilotStudioHealthMonitor.Tests\`  
**Framework:** NUnit 3.x + Moq 4.x targeting net472  
**Run:** Build the solution in Visual Studio 2022, then open Test Explorer → Run All.

### 2.1 Build & Run

```powershell
cd "C:\Project\XRMToolbox Tools"
dotnet restore CopilotStudioHealthMonitor.Tests\CopilotStudioHealthMonitor.Tests.csproj
dotnet test CopilotStudioHealthMonitor.Tests\CopilotStudioHealthMonitor.Tests.csproj
```

### 2.2 Test Matrix

#### AgentModelTests (~14 tests)

| Test | What is verified |
|---|---|
| `AuthenticationModeLabel_ReturnsCorrectLabel` (×4) | 0→"No Auth", 1→"Azure AD", 2+→"External" |
| `AuthenticationModeDisplay_*` (×3) | Correct emoji per auth mode |
| `InSolutionDisplay_*` (×2) | ✅/❌ based on InSolution |
| `StatusLabel_KnownCode_*` (×2) | 1→"Active", 2→"Inactive" |
| `StatusLabel_UnknownCode_*` (×3) | Falls back to "Status N" |
| `OwnerDisplay_ActiveOwner` | Returns name only |
| `OwnerDisplay_DisabledOwner` | Appends ⚠️ and "Disabled" |
| `NewInstance_AllComponentLists_AreEmpty` | Default empty collections |

#### BotComponentModelTests (~7 tests)

| Test | What is verified |
|---|---|
| `ComponentTypeLabel_*` (×6) | 0→Topic, 1→Action, 9→Knowledge Source, other→"Type N" |
| `IsActive_WhenStateCode0_ReturnsTrue` | Active when stateCode == 0 |
| `IsActive_WhenStateCodeNonZero_ReturnsFalse` (×3) | Inactive for any other stateCode |

#### AgentSecurityResultTests (~8 tests)

| Test | What is verified |
|---|---|
| `ScoreLabel_ReturnsBandForScore` (×6) | Boundary values: 85=🟢, 84=🟡, 60=🟡, 59=🔴 |
| `IssueCount_ReflectsFailedChecksCount` | Live counter on FailedChecks list |
| `NewInstance_HasEmptyCollections` | Safe defaults |

#### DeploymentCheckResultTests (2 tests)

| Test | What is verified |
|---|---|
| `Status_WhenPassed` | "✅ Pass" |
| `Status_WhenFailed` | "❌ Fail" |

#### AgentInventoryServiceTests (~17 tests)

| Test | What is verified |
|---|---|
| `GetAllAgents_MapsNameCorrectly` | Name attribute mapped |
| `GetAllAgents_NullName_FallsBackToUnnamed` | Null → "(Unnamed)" |
| `GetAllAgents_MapsAuthenticationMode` | OptionSetValue.Value extracted |
| `GetAllAgents_MissingAuthenticationMode_DefaultsToMinusOne` | Null OptionSetValue → -1 |
| `GetAllAgents_MapsOwnerNameFromAlias` | AliasedValue "owner.fullname" extracted |
| `GetAllAgents_MissingOwnerAlias_FallsBackToUnknown` | Absent alias → "Unknown" |
| `GetAllAgents_MapsOwnerDisabled` | AliasedValue bool extracted |
| `GetAllAgents_MissingOwnerDisabledAlias_DefaultsToFalse` | Absent alias → false |
| `GetAllAgents_WhenInSolution_SetsInSolutionTrue` | solutioncomponent row → true |
| `GetAllAgents_WhenNotInSolution_SetsInSolutionFalse` | Empty query → false |
| `GetAllAgents_EmptyOrg_ReturnsEmptyList` | Zero entities → empty list |
| `GetBotComponents_MapsComponentTypeAndName` | Type, name, active state mapped |
| `GetBotComponents_InactiveComponent_IsActiveFalse` | stateCode 1 → IsActive=false |
| `GetBotComponents_MapsContent` | Content string preserved |
| `GetBotComponents_NoComponents_ReturnsEmpty` | Zero entities → empty |
| `IsAgentInSolution_WhenSolutionComponentExists_ReturnsTrue` | Non-empty result → true |
| `IsAgentInSolution_WhenNoSolutionComponent_ReturnsFalse` | Empty result → false |

#### SecurityScannerServiceTests (~21 tests)

| Test | What is verified |
|---|---|
| `ScanAgent_PerfectAgent_ScoreIs100NoIssues` | Baseline: no deductions |
| `SEC01_NoAuth_DeductsThirtyPoints` | authMode=0 → score 70 |
| `SEC01_NoAuth_AddsFailedCheckAndRemediation` | "SEC-01" in FailedChecks |
| `SEC01_AzureAD_NoDeduction` | authMode=1 → no SEC-01 |
| `SEC01_External_NoDeduction` | authMode=2 → no SEC-01 |
| `SEC05_DisabledOwner_DeductsTwentyPoints` | ownerDisabled → score 80 |
| `SEC05_DisabledOwner_AddsFailedCheckWithOwnerName` | Owner name in check text |
| `SEC05_EnabledOwner_NoDeduction` | Active owner → no SEC-05 |
| `SEC07_NotInSolution_DeductsFivePoints` | Not in solution → score 95 |
| `SEC07_NotInSolution_AddsFailedCheck` | "SEC-07" in FailedChecks |
| `SEC07_InSolution_NoDeduction` | In solution → no SEC-07 |
| `SEC03_ActionWithOpenApiConnection_DeductsTenPoints` | HTTP action → score 90 |
| `SEC03_ActionWithHttpRequest_DeductsTenPoints` | httpRequest → score 90 |
| `SEC03_TopicWithHttpContent_NoDeduction_WrongComponentType` | Type 0 is immune to SEC-03 |
| `SEC03_ActionWithNullContent_NoDeduction` | Null content → no false positive |
| `SEC03_ActionWithSafeContent_NoDeduction` | PowerFx → no SEC-03 |
| `AllFourChecks_TotalDeductionIs65_ScoreIs35` | -30-20-5-10 = 35 |
| `Score_NeverGoesBelowZero` | Floor guard confirmed |
| `FailedChecksAndRemediationSteps_CountsAlwaysMatch` | Parity enforced |
| `ScanAllAgents_ResultsOrderedByScoreAscending` | Worst agent first |
| `ScanAllAgents_EmptyOrg_ReturnsEmptyList` | Zero bots → empty |

#### DeploymentReadinessServiceTests (~16 tests)

| Test | What is verified |
|---|---|
| `HasTargetOrg_*` (×2) | False before, true after SetTargetService |
| `DEP01_AgentInSolution_Passes` | DEP-01 pass, empty remediation |
| `DEP01_AgentNotInSolution_Fails` | DEP-01 fail, has remediation |
| `DEP02_AzureAD_Passes` | DEP-02 pass, detail includes "Azure AD" |
| `DEP02_NoAuth_Fails` | DEP-02 fail, has remediation |
| `DEP02_External_Passes` | DEP-02 pass |
| `DEP03_AllEnvVarsHaveValues_Passes` | DEP-03 pass |
| `DEP03_OneEnvVarMissingValue_Fails` | Var name in detail |
| `DEP03_FourMissingEnvVars_TruncatesListWithEllipsis` | "..." + count shown |
| `DEP03_ThreeMissingEnvVars_NoEllipsis` | Exactly 3 → no "..." |
| `DEP03_EnvVarMissingDisplayName_FallsBackToSchemaName` | schemaname fallback |
| `DEP04_AllConnectionRefsConfigured_Passes` | DEP-04 pass |
| `DEP04_UnconfiguredConnectionRef_Fails` | Ref name in detail |
| `DEP04_FourUnconfiguredRefs_TruncatesListWithEllipsis` | "..." + count |
| `DEP03_WithTargetOrg_UsesTargetServiceAndSaysTargetOrg` | Target service queried |
| `DEP04_WithTargetOrg_UsesTargetServiceAndSaysTargetOrg` | Target service queried |
| `DEP03_WithoutTargetOrg_DetailMentionsCurrentOrg` | "current org" in detail |
| `RunChecks_AlwaysReturnsFourResults` | Exactly 4 checks returned |
| `RunChecks_CheckNamesIncludeAllFourDEPCodes` | DEP-01 through DEP-04 present |

#### AlmDiffServiceTests (~16 tests)

| Test | What is verified |
|---|---|
| `HasTargetOrg_*` (×2) | False before, true after SetTargetService |
| `GetTargetAgents_WithoutTargetService_ReturnsEmpty` | No target → empty |
| `GetTargetAgents_WithTargetService_ReturnsTargetAgents` | Target bots fetched |
| `RunDiff_IdenticalComponents_ReturnsMatch` | Match status |
| `RunDiff_Match_DiffStatusContainsMatchText` | DiffStatus string correct |
| `RunDiff_SameNameDifferentContent_ReturnsContentDiffers` | Content differs status |
| `RunDiff_ComponentOnlyInSource_ReturnsMissingInTarget` | Source-only component |
| `RunDiff_ComponentOnlyInTarget_ReturnsOnlyInTarget` | Target-only component |
| `RunDiff_SameNameDifferentCase_TreatedAsMatch` | Case-insensitive matching |
| `RunDiff_SameNameDifferentType_TreatedAsMissingAndExtra` | Type is part of the key |
| `RunDiff_BothNullContent_ReturnsMatch` | Null treated as empty string |
| `RunDiff_MixedComponents_AllFourStatusCodes` | All four statuses in one diff |
| `RunDiff_ResultsSortedByComponentTypeThenName` | Sort: Type asc, then Name asc |
| `RunDiff_SameType_SortedByNameAlphabetically` | Name sort within same type |
| `RunDiff_BothEmpty_ReturnsEmpty` | Empty → empty |
| `RunDiff_EmptySource_AllTargetItemsAreOnlyInTarget` | All target items are extra |
| `RunDiff_EmptyTarget_AllSourceItemsAreMissingInTarget` | All source items missing |

---

## 3. Manual UI Tests — Prerequisites

Before running manual tests, set up these Dataverse environments:

### Source Environment must have
- [ ] 5+ Copilot Studio agents in various states
- [ ] At least 1 agent with **No Authentication** (authMode = 0)
- [ ] At least 1 agent with a **disabled owner** account
- [ ] At least 1 agent **not in any solution**
- [ ] At least 1 agent with an **HTTP connector action** (OpenApiConnection or httpRequest)
- [ ] At least 1 agent that is **in a solution**
- [ ] At least 1 **missing environment variable** value
- [ ] At least 1 **unconfigured connection reference** (connectionid is null)
- [ ] At least 1 agent that is **Inactive** (statusCode = 2)

### Target Environment must have
- [ ] At least 1 agent with the **same name** as a source agent (for diff testing)
- [ ] At least 1 agent that is **only in the target** (not in source)

---

## 4. Manual UI Tests — Per Tab

### 4.1 Plugin Installation & Loading

| # | Step | Expected |
|---|---|---|
| P-01 | Open XrmToolBox, search "Copilot" in tool list | "Copilot Studio Agent Health Monitor" appears with blue background |
| P-02 | Click the plugin | Plugin window opens with 5 tabs: Inventory, Security, Deployment, ALM Diff, Dashboard |
| P-03 | Observe tool list metadata | Description visible, correct colours |
| P-04 | Before connecting to any org, attempt to use each tab | Buttons are disabled; no crash |

### 4.2 Connection

| # | Step | Expected |
|---|---|---|
| C-01 | Click Connect / use XrmToolBox connection | UpdateConnection fires; "Loading Copilot Studio agents..." spinner shown |
| C-02 | Connect to org with no agents | MessageBox: "No Copilot Studio agents found" |
| C-03 | Connect to org with agents | All tabs receive agent lists; no error |
| C-04 | All service buttons enabled after connection | Refresh, Run Scan, Run Checks, Connect Target Org all become enabled |
| C-05 | Disconnect / switch org | Re-loading replaces all previous data with new org data |

### 4.3 Inventory Tab

| # | Step | Expected |
|---|---|---|
| I-01 | Load agents | Grid shows: Name, Authentication (with emoji), Owner, Status, In Solution, Last Modified |
| I-02 | No-auth agent | Row highlighted **red** (RGB 255,235,235) |
| I-03 | Disabled-owner agent | Row highlighted **yellow** (RGB 255,248,220) |
| I-04 | Agent count label | Shows "N of M agents" |
| I-05 | Type in search box | Grid filters by name, owner, or auth mode in real time |
| I-06 | Clear search box | All agents restored; count label updates |
| I-07 | Search is case-insensitive | Typing "azure" finds agents with "Azure AD" auth |
| I-08 | Click an agent row | Detail panel shows Agent ID, Authentication, Owner, Status, In Solution, Created On, Last Modified |
| I-09 | Click an agent with components | Components panel shows Type / Name / Active–Inactive below detail |
| I-10 | Inactive component | Shown in **grey** text |
| I-11 | Agent with no components | Shows "(No components found)" placeholder |
| I-12 | Click Export CSV | SaveFileDialog opens; file saved with correct headers and data |
| I-13 | Export CSV — agent name with comma | Value is quoted in CSV output |
| I-14 | Export CSV — active search filter | Only filtered agents exported, count in confirmation matches |
| I-15 | Export CSV — before any agent loaded | Export button is disabled |
| I-16 | Click Refresh | Reload triggered; grid repopulated |

### 4.4 Security Tab

| # | Step | Expected |
|---|---|---|
| S-01 | Click Run Scan | "Running security scan..." spinner; results populate on completion |
| S-02 | Score ≥ 85 | Row **green** (RGB 220,245,220); ScoreLabel "🟢 Healthy" |
| S-03 | Score 60–84 | Row **yellow** (RGB 255,248,220); ScoreLabel "🟡 Needs Attention" |
| S-04 | Score < 60 | Row **red** (RGB 255,220,220); ScoreLabel "🔴 Critical" |
| S-05 | Scan count label | Shows "N agents scanned" |
| S-06 | Click agent with perfect score | Detail shows "✅ No issues found" in green |
| S-07 | Click agent with failures | Detail header shows score; each failed check shown in red with remediation |
| S-08 | Agent with No Auth | SEC-01 listed with "Configure Azure AD..." remediation |
| S-09 | Agent with disabled owner | SEC-05 listed with owner name |
| S-10 | Agent not in solution | SEC-07 listed |
| S-11 | Agent with HTTP action | SEC-03 listed |
| S-12 | Export CSV | File includes Agent Name, Score, Health, Issue Count, pipe-separated checks & remediations |
| S-13 | Remediation steps count matches issue count | No orphaned or missing remediations |

### 4.5 Deployment Tab

| # | Step | Expected |
|---|---|---|
| D-01 | Agent dropdown | Auto-populated with all agents; first agent pre-selected |
| D-02 | Change agent selection | Results cleared; status banner resets |
| D-03 | Run Checks (source org only) | Results show DEP-01 through DEP-04; banner green or red |
| D-04 | All checks pass | Green banner: "✅ {agent} — Ready to Deploy (4/4 checks passed)" |
| D-05 | Some checks fail | Red banner: "❌ {agent} — Not Ready (N of 4 checks failed)" |
| D-06 | DEP-01 fail | Shows "not included in any solution" + remediation |
| D-07 | DEP-02 fail | Shows "No Authentication" + remediation |
| D-08 | DEP-03 fail | Lists missing env var names (up to 3 + "..."); remediation shown |
| D-09 | DEP-04 fail | Lists unconfigured connection ref names; remediation shown |
| D-10 | Click Connect Target Org | XrmToolBox connection dialog opens for second org |
| D-11 | After target org connected | Label turns green: "Connected: {orgName}"; button text → "🔗 Change Target Org" |
| D-12 | Run Checks with target org | DEP-03 and DEP-04 evaluate **target org** data; detail says "target org" |
| D-13 | DEP-01/DEP-02 always use source agent data | No change in DEP-01/02 result when switching target org |
| D-14 | Dashboard jump to Deployment | Correct agent pre-selected in dropdown |

### 4.6 ALM Diff Tab

| # | Step | Expected |
|---|---|---|
| A-01 | Before connecting target org | Source dropdown populated; target dropdown disabled; Run Diff disabled |
| A-02 | Click Connect Target Org | Connection dialog opens |
| A-03 | After target org connected | Target label turns green; target dropdown shows "Loading agents..." then populates |
| A-04 | Run Diff button state | Enabled only when both source and target agents selected AND target org is connected |
| A-05 | Run Diff | "Comparing '{source}' vs '{target}'..." spinner; grid populates |
| A-06 | Match row | Green background (RGB 220,245,220); "✅ Match" |
| A-07 | Content differs row | Yellow background (RGB 255,248,210); "⚠️ Content Differs" |
| A-08 | Missing in target row | Red background (RGB 255,220,220); "❌ Missing in Target" |
| A-09 | Only in target row | Blue background (RGB 220,235,255); "➕ Only in Target" |
| A-10 | All match — banner | Green banner: "✅ {source} vs {target} — N components match perfectly" |
| A-11 | Differences exist — banner | Amber banner with counts per status |
| A-12 | No components | "No components found to compare." |
| A-13 | Results sorted by Component Type then Name | Verify sort order in grid |
| A-14 | Change source or target selection | Grid cleared; banner resets |

### 4.7 Dashboard Tab

| # | Step | Expected |
|---|---|---|
| DB-01 | Click Refresh | "Loading dashboard data..." spinner; all 4 cards and grid populated |
| DB-02 | Total Agents card | Shows correct count of all agents |
| DB-03 | Critical card | Count of agents with score < 60 |
| DB-04 | No Authentication card | Count of agents with authMode = 0 |
| DB-05 | Orphaned Owners card | Count of agents with disabled owner |
| DB-06 | Agent grid | Shows Rank, Name, Score, Health, Auth Mode, Owner, In Solution |
| DB-07 | Grid sort | Rank 1 = lowest score; worst agents at top |
| DB-08 | Color coding | Green/yellow/red per score band (same thresholds as Security tab) |
| DB-09 | Last refreshed label | Updates timestamp on each refresh |
| DB-10 | Select agent in grid | Jump buttons become enabled |
| DB-11 | No agent selected | Jump buttons disabled |
| DB-12 | Jump to Security | Security tab selected; no pre-filter applied (by design) |
| DB-13 | Jump to Deployment | Deployment tab selected; correct agent pre-selected in dropdown |
| DB-14 | Jump to ALM Diff | ALM Diff tab selected |
| DB-15 | Dashboard with 0 agents | All cards show "0"; grid empty |

---

## 5. Edge Case & Negative Tests

| # | Scenario | Expected |
|---|---|---|
| E-01 | Org with agents but all have no components | Components panel shows "(No components found)"; Security scan runs without crash |
| E-02 | Agent name contains comma | CSV export wraps in quotes; file opens correctly in Excel |
| E-03 | Agent name contains double-quotes | CSV export escapes `""` per RFC 4180 |
| E-04 | Very long agent name (200+ chars) | Grid wraps; no truncation in detail panel |
| E-05 | Owner name is null / empty | Shows "Unknown" rather than blank |
| E-06 | createdOn / modifiedOn null | Shows "—" in detail panel; no crash |
| E-07 | Security scan on org with 50+ agents | Completes in < 30 seconds; no timeout dialog |
| E-08 | ALM diff with 200+ components per agent | Grid scrollable; no hang |
| E-09 | Disconnect target org mid-session | Source org functions unaffected |
| E-10 | Run scan before loading agents | Scan button disabled (service null guard) |
| E-11 | Cancel SaveFileDialog in CSV export | No file written; no error |
| E-12 | Disk full during CSV export | Exception handled gracefully (OS message acceptable) |

---

## 6. Integration Test Checklist (Live Dataverse)

Run these against a real environment after each major code change:

- [ ] **INT-01** Connect to DEV org → agents load within 10 s
- [ ] **INT-02** Security scan on DEV org → correct SEC codes for known agents
- [ ] **INT-03** DEP check (source only) → DEP-03/DEP-04 results match env var / conn ref state in Portal
- [ ] **INT-04** Connect target UAT org → target agents list populates in ALM Diff dropdown
- [ ] **INT-05** Run ALM Diff between same agent in DEV and UAT → expected match/differ/missing results
- [ ] **INT-06** DEP check with target org → DEP-03/04 evaluate UAT data; banner says "target org"
- [ ] **INT-07** Dashboard refresh → card values agree with Security tab result counts
- [ ] **INT-08** Export CSV from Inventory → open in Excel; verify all agents, no corrupt rows
- [ ] **INT-09** Export CSV from Security → pipe-separated issues readable; no encoding issues

---

## 7. Known Limitations / Potential Bugs

| ID | Location | Description |
|---|---|---|
| BUG-01 | `AlmDiffService.CompareComponents` | `ToDictionary` on source or target will throw `ArgumentException` if the same org contains two `botcomponent` rows with identical `(componentType, name)`. Recommend `GroupBy` + take-first or a composite key with ID as tiebreaker. |
| BUG-02 | `SecurityScannerService.ScanAgent` | SEC-03 only looks for `"OpenApiConnection"` and `"httpRequest"` string literals inside component content. A base64-encoded or whitespace-varied JSON payload would not be detected. |
| BUG-03 | `DeploymentReadinessService.RunChecks` | DEP-03 queries ALL environment variable definitions with no value, not just those relevant to the selected agent. This is intentional (org-level check) but may surprise users in orgs with many unrelated env vars. |

---

## 8. Pass Criteria

| Criterion | Target |
|---|---|
| All automated unit tests green | 100% |
| No crashes in any manual UI test | 0 crashes |
| CSV exports open correctly in Excel | Both exports |
| Security scores match expected deductions per agent | Verified against 3+ known agents |
| ALM Diff results agree with manual component comparison | Verified for 1 DEV→UAT pair |
| Dashboard card counts equal Security tab counts | Verified after each refresh |
