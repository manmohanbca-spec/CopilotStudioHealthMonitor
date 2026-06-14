# Copilot Studio Agent Health Monitor — User Guide

**Version 1.0.0 | XrmToolBox Plugin | Power Platform**

---

## Table of Contents

1. [Overview](#1-overview)
2. [System Requirements](#2-system-requirements)
3. [Installation](#3-installation)
4. [Connecting to Your Environment](#4-connecting-to-your-environment)
5. [Dashboard](#5-dashboard)
6. [Agent Inventory](#6-agent-inventory)
7. [Security Scanner](#7-security-scanner)
8. [Deployment Readiness](#8-deployment-readiness)
9. [ALM Diff](#9-alm-diff)
10. [Exporting Data](#10-exporting-data)
11. [Troubleshooting](#11-troubleshooting)
12. [Known Limitations](#12-known-limitations)

---

## 1. Overview

**Copilot Studio Agent Health Monitor** is an XrmToolBox plugin that gives Power Platform administrators and developers a single-pane view of all Microsoft Copilot Studio agents in a Dataverse environment.

### What it does

| Capability | Description |
|---|---|
| **Inventory** | Lists every Copilot Studio agent with owner, auth mode, solution membership, and creation dates |
| **Security Scan** | Scores each agent 0–100 against four security checks and provides remediation steps |
| **Deployment Readiness** | Runs four pre-flight checks before promoting an agent to a target environment |
| **ALM Diff** | Side-by-side component comparison of the same agent across two Dataverse environments |
| **Dashboard** | KPI cards and a risk-ranked table summarising the health of all agents at once |

---

## 2. System Requirements

| Requirement | Minimum |
|---|---|
| XrmToolBox | 1.2024.9.69 or later |
| .NET Framework | 4.7.2 |
| Windows | Windows 10 / Windows Server 2016 or later |
| Dataverse / Dynamics 365 | Any online or on-premise environment with Copilot Studio |
| Permissions | System Administrator or System Customizer role in the target environment |

---

## 3. Installation

### Option A — XrmToolBox Plugin Store (recommended)

1. Open XrmToolBox.
2. Click **Tool Library** in the top ribbon.
3. Search for **Copilot Studio Agent Health Monitor**.
4. Click **Install** and restart XrmToolBox when prompted.

### Option B — Manual (DLL drop)

1. Download `CopilotStudioHealthMonitor.dll` from the [Releases page](https://github.com/manmohanbca-spec/CopilotStudioHealthMonitor/releases).
2. Copy the DLL to:
   ```
   %APPDATA%\MscrmTools\XrmToolBox\Plugins\
   ```
3. Restart XrmToolBox — the plugin appears automatically in the tool list.

---

## 4. Connecting to Your Environment

1. Open XrmToolBox and connect to your Dataverse environment via the **Connection** button in the top toolbar.
2. Open **Copilot Studio Agent Health Monitor** from the tool list.
3. The plugin loads all agents automatically on first connect.
4. To reload agents at any time, use the **🔄 Refresh Agents** button on the **Agent Inventory** tab.

> **Tip:** The plugin requires a connection with at least read access to the `bot`, `botcomponent`, `solutioncomponent`, `environmentvariabledefinition`, and `connectionreference` tables.

---

## 5. Dashboard

The Dashboard gives an at-a-glance health overview of all agents in the connected environment.

### Summary Cards

| Card | What it counts |
|---|---|
| **Total Agents** | Total number of Copilot Studio agents found |
| **Critical (score < 60)** | Agents with a security score below 60 — highest risk |
| **No Authentication** | Agents running with No Authentication configured |
| **Orphaned Owners** | Agents whose owner account is disabled in Azure AD |

### Risk-Ranked Table

Agents are listed with the worst security score first (rank 1 = most at-risk).

| Column | Description |
|---|---|
| **#** | Risk rank (1 = worst) |
| **Agent Name** | Display name of the Copilot Studio agent |
| **Score** | Security score 0–100 |
| **Health** | 🟢 Healthy (≥85) / 🟡 Needs Attention (60–84) / 🔴 Critical (<60) |
| **Auth Mode** | ✅ Azure AD / ⚠️ External / ❌ No Auth |
| **Owner** | Owner name; shows ⚠️ (Disabled) if owner account is inactive |
| **In Solution** | ✅ Yes / ❌ No |

### Row colour coding

| Colour | Score range | Meaning |
|---|---|---|
| Green | ≥ 85 | Healthy — no significant risks |
| Yellow | 60–84 | Needs Attention — at least one security issue |
| Red | < 60 | Critical — multiple or high-severity issues |

### Action Buttons

Select any agent row to enable:

- **View Security** — jumps to the Security Scanner tab pre-filtered for the selected agent
- **View Deployment** — jumps to the Deployment Readiness tab with the agent pre-selected
- **View ALM Diff** — jumps to the ALM Diff tab

---

## 6. Agent Inventory

The Agent Inventory tab shows detailed metadata for every Copilot Studio agent.

### Filter Bar

Type in the search box to filter agents by **Name**, **Owner**, or **Auth Mode** in real time. The filter is case-insensitive.

### Columns

| Column | Description |
|---|---|
| **Name** | Agent display name |
| **Authentication** | Auth mode: No Auth / Azure AD / External |
| **Owner** | Owner full name (⚠️ if owner is disabled) |
| **Owner Disabled** | True/False |
| **Status** | Active or Inactive |
| **In Solution** | Whether the agent belongs to a Dataverse solution |
| **Created On** | Date the agent was created |
| **Last Modified** | Date of last modification |

### Component Panel (lower half)

Click any agent row to load its bot components in the lower grid:

| Column | Description |
|---|---|
| **Type** | Topic / Action / Knowledge Source / Variable / Entity / Entity List |
| **Name** | Component display name |
| **Active** | Whether the component is enabled |
| **Content** | Truncated JSON content of the component |

> **Note:** Loading components makes a second API call to Dataverse. For large agents (hundreds of topics), this may take a few seconds.

### Refresh / Export

- **🔄 Refresh Agents** — Reloads the full agent list from Dataverse.
- **📥 Export CSV** — Exports the currently visible (filtered) list to a UTF-8 CSV file.

---

## 7. Security Scanner

The Security Scanner runs four automated checks against every agent and produces a 0–100 score with actionable remediation steps.

### Running a Scan

Click **Run Security Scan**. The scanner queries Dataverse for all agents and their components; results appear within a few seconds.

### Security Checks

#### SEC-01 — No Authentication (-30 points)

| | |
|---|---|
| **What it checks** | Whether the agent's authentication mode is set to "No Authentication" |
| **Why it matters** | An unauthenticated agent is accessible by anyone; it cannot access user-specific data or enforce security policies |
| **Remediation** | In Copilot Studio → Settings → Security → Authentication, configure **Azure AD** or **External** authentication |

#### SEC-03 — HTTP Request Actions (-10 points)

| | |
|---|---|
| **What it checks** | Whether any **Action** component (componentType = 1) in the agent calls `OpenApiConnection` or `httpRequest` connector types |
| **Why it matters** | HTTP connectors can exfiltrate data to arbitrary endpoints and may bypass Data Loss Prevention (DLP) connector policies |
| **Remediation** | Review each HTTP action for data exfiltration risk; add the connector to your DLP policy in the Power Platform Admin Center |

#### SEC-05 — Orphaned Owner (-20 points)

| | |
|---|---|
| **What it checks** | Whether the agent's owner account is disabled in Azure AD / Dataverse |
| **Why it matters** | A disabled-owner agent has no active accountable party; alerts and ownership transfers may fail |
| **Remediation** | In Copilot Studio → Settings → Advanced, reassign the agent to an active user or service account |

#### SEC-07 — Not in Solution (-5 points)

| | |
|---|---|
| **What it checks** | Whether the agent is a member of at least one Dataverse solution |
| **Why it matters** | Agents outside a solution cannot be transported via ALM pipelines (export/import) and have no version control |
| **Remediation** | In make.powerapps.com, open your managed solution and add the agent as a component |

### Score Bands

| Score | Label | Meaning |
|---|---|---|
| 85–100 | 🟢 Healthy | All or nearly all checks pass |
| 60–84 | 🟡 Needs Attention | One or two issues to address |
| 0–59 | 🔴 Critical | Multiple high-severity issues; immediate action required |

### Detail Panel

Click any agent row to see:
- The full score and health label
- Each failed check with its description
- The corresponding remediation step for each issue

### Exporting Results

Click **📥 Export CSV** to save the scan results. The CSV includes: Agent Name, Score, Health, Issue Count, Failed Checks (pipe-separated), Remediation Steps (pipe-separated).

---

## 8. Deployment Readiness

The Deployment Readiness tab runs four pre-deployment checks on a selected agent before it is promoted to a target environment.

### Connecting a Target Organisation

> If you only want to check the **current** environment, skip this step — checks will run against the current connection.

1. Click **🔗 Connect Target Org**.
2. XrmToolBox opens the Connection Manager — select or create a connection to the **target** environment (e.g., UAT or Production).
3. The status bar shows **Connected: \<OrgName\>** when successful.
4. Once connected, DEP-03 and DEP-04 checks run against the **target** environment.

### Running Checks

1. Select an agent from the **Agent** dropdown.
2. Click **Run Checks**.
3. Results appear as a list with a pass/fail banner.

### Deployment Checks

#### DEP-01 — In Solution

| | |
|---|---|
| **What it checks** | Whether the agent belongs to a Dataverse solution |
| **Passed means** | The agent can be exported/imported as part of a solution package |
| **Remediation** | Add the agent to a solution in make.powerapps.com before transport |

#### DEP-02 — Authentication Configured

| | |
|---|---|
| **What it checks** | Whether the agent uses Azure AD or External authentication (not No Auth) |
| **Passed means** | The agent will enforce user identity in the target environment |
| **Remediation** | Configure authentication in Copilot Studio → Settings → Security |

#### DEP-03 — Environment Variables (checked in target org if connected)

| | |
|---|---|
| **What it checks** | Whether all `environmentvariabledefinition` records in the target environment have corresponding value records |
| **Passed means** | All environment variable values are populated — no missing configuration |
| **Remediation** | Set values for missing variables in the target org via Settings → Solutions → Environment Variables |

#### DEP-04 — Connection References (checked in target org if connected)

| | |
|---|---|
| **What it checks** | Whether all `connectionreference` records in the target environment have a mapped `connectionid` |
| **Passed means** | All connectors are mapped — flows and agents will have working connections |
| **Remediation** | In the target solution, map each connection reference to a valid connection |

### Result Banner

| Banner | Meaning |
|---|---|
| ✅ Green — "Ready to Deploy" | All 4 checks passed |
| ❌ Red — "Not Ready" | One or more checks failed; count shown |

---

## 9. ALM Diff

The ALM Diff tab performs a component-level comparison of the **same agent** across two Dataverse environments (e.g., Dev vs UAT).

### Setup

1. **Connect a Target Org** — click **🔗 Connect Target Org** and select your target environment.
2. Once connected, the **Target Agent** dropdown populates with agents from the target.
3. Select a **Source Agent** (from the current/source environment).
4. Select a **Target Agent** (from the connected target environment).
5. Click **Run Diff**.

### Diff Results

| Status | Colour | Meaning |
|---|---|---|
| ✅ Match | Green | Component exists in both environments with identical content |
| ⚠️ Content Differs | Yellow | Component exists in both but the JSON content is different |
| ❌ Missing in Target | Red | Component exists in source but is absent in target |
| ➕ Only in Target | Blue | Component exists in target but is absent in source |

### Result Banner

- **Green** — All components match perfectly across both orgs
- **Amber** — Summary of matches, differences, missing, and extra components

### Notes on Comparison

- Component matching is **case-insensitive** by name
- Matching is on **(component type, name)** — the same name in different types (e.g., a Topic named "Greeting" vs an Action named "Greeting") are treated as separate components
- If Dataverse returns duplicate `(type, name)` rows (can happen after failed imports), the first record is used for matching and all duplicates appear individually in the diff results

---

## 10. Exporting Data

### Agent Inventory CSV

Fields: `Name, Authentication, Owner, Owner Disabled, Status, In Solution, Created On, Last Modified`

### Security Scan CSV

Fields: `Agent Name, Score, Health, Issue Count, Failed Checks, Remediation Steps`

- Failed Checks and Remediation Steps are pipe-separated (`|`) within a single CSV cell
- Files are UTF-8 encoded and timestamped in the filename

---

## 11. Troubleshooting

### No agents appear after connecting

- Confirm the connection is to a Dataverse environment (not a legacy Dynamics CRM endpoint)
- Ensure your user has at least **System Customizer** or **System Administrator** role
- Check that Copilot Studio is enabled in the environment

### "Error loading agents" message

- The FetchXML query requires read access to the `bot` table — verify permissions
- On-premise environments may not have the `bot` entity; Copilot Studio is online-only

### Security scan takes a long time

- Each agent triggers a separate component query. Environments with many agents (50+) will take longer
- Check network latency to the Dataverse endpoint

### ALM Diff "Run Diff" button stays disabled

- Ensure a target org is connected (green status bar)
- Wait for the target agent list to finish loading — the button remains disabled while loading is in progress

### Target org agents show "Loading agents..." indefinitely

- The target org connection may have failed silently — try clicking **Change Target Org** to reconnect

### Deployment checks run against source instead of target

- Connect the target org **before** running checks — if no target is connected, checks run against the current (source) org automatically

---

## 12. Known Limitations

| Limitation | Details |
|---|---|
| **Environment Variables with Defaults** | DEP-03 flags env vars that have no `environmentvariablevalue` record, even if a `defaultvalue` is set on the definition. A default value is still technically a valid value; this may produce false positives for variables that rely on defaults. |
| **Carriage Return in CSV** | Values containing `\r` (carriage return without newline) are not quoted in CSV exports. This is rare in Dataverse field values but may cause formatting issues in some CSV readers. |
| **Component Content Truncation** | The Agent Inventory tab truncates component content to 200 characters for display. The full content is used for ALM Diff comparison. |
| **Copilot Studio Online Only** | The `bot` and `botcomponent` Dataverse tables only exist in Power Platform online environments. On-premise Dynamics 365 installations are not supported. |
| **Read-Only** | This plugin is read-only — it never writes to or modifies data in your Dataverse environment. |

---

*Copilot Studio Agent Health Monitor is an open-source XrmToolBox community plugin.*
*For issues or feature requests, visit the GitHub repository.*
