# Copilot Studio Agent Health Monitor — User Guide

**Version 1.2.1 | XrmToolBox Plugin | Power Platform**

---

## Table of Contents

1. [Overview](#1-overview)
2. [System Requirements](#2-system-requirements)
3. [Installation](#3-installation)
4. [Connecting to Your Environment](#4-connecting-to-your-environment)
5. [Dashboard](#5-dashboard)
6. [Agent Inventory](#6-agent-inventory)
7. [Security Scanner](#7-security-scanner)
8. [Sharing & Access](#8-sharing--access)
9. [Knowledge Sources](#9-knowledge-sources)
10. [Adoption & Lifecycle](#10-adoption--lifecycle)
11. [Deployment Readiness](#11-deployment-readiness)
12. [ALM Diff](#12-alm-diff)
13. [ALM & Dependencies](#13-alm--dependencies)
14. [Governance Report](#14-governance-report)
15. [Exporting Data](#15-exporting-data)
16. [Troubleshooting](#16-troubleshooting)
17. [Known Limitations](#17-known-limitations)

---

## 1. Overview

**Copilot Studio Agent Health Monitor** is an XrmToolBox plugin that gives Power Platform administrators and developers a single-pane view of all Microsoft Copilot Studio agents in a Dataverse environment.

### What it does

| Capability | Description |
|---|---|
| **Inventory** | Lists every Copilot Studio agent with owner, auth mode, solution membership, and creation dates |
| **Security Scan** | Scores each agent 0–100 against Microsoft's official *Top 10 Copilot Studio agent security risks* and provides remediation steps |
| **Sharing & Access** | Audits who each agent is shared with and flags over-shared or broadly-exposed agents |
| **Knowledge Sources** | Inventories every knowledge source attached to each agent (websites, files, Dataverse, SharePoint) |
| **Adoption & Lifecycle** | Flags dormant and orphaned agents (agent sprawl) for license cleanup and risk reduction |
| **Deployment Readiness** | Runs four pre-flight checks before promoting an agent to a target environment |
| **ALM Diff** | Side-by-side component comparison of the same agent across two Dataverse environments |
| **ALM & Dependencies** | Maps each agent's solution membership and forward dependency graph, flagging ALM transport risks before you deploy |
| **Governance Report** | One-click self-contained HTML report (KPIs + Microsoft Top-10 scorecard) for security and leadership |
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
| **Dormant / Orphaned** | Agents flagged as Dormant or Orphaned by the Adoption & Lifecycle analysis (disabled owner or no edit in 90+ days) |

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
- **📈 Adoption** — jumps to the Adoption & Lifecycle tab
- **View Deployment** — jumps to the Deployment Readiness tab with the agent pre-selected
- **View ALM Diff** — jumps to the ALM Diff tab

### Export Governance Report

- **📄 Export Governance Report** — generates a single self-contained HTML governance report covering the whole environment (KPI summary, Microsoft Top-10 scorecard, and per-section tables for security, sharing, knowledge, and adoption). See [Governance Report](#14-governance-report) for details.

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

The Security Scanner runs a suite of automated checks against every agent and produces a 0–100 score with actionable remediation steps. The checks are aligned to Microsoft's official **Top 10 Copilot Studio agent security risks** (February 2026), and every finding is tagged with the Microsoft Top-10 risk number it maps to.

### Running a Scan

Click **Run Security Scan**. The scanner queries Dataverse for all agents and their components; results appear within a few seconds. The tab header shows a clean/critical summary — the count of agents with no findings versus agents with at least one critical finding.

### Security Checks

#### SEC-01 — No Authentication (-30 points) · *Top-10 #2*

| | |
|---|---|
| **What it checks** | Whether the agent's authentication mode is set to "No Authentication" |
| **Why it matters** | An unauthenticated agent is accessible by anyone; it cannot access user-specific data or enforce security policies |
| **Remediation** | In Copilot Studio → Settings → Security → Authentication, configure **Azure AD** or **External** authentication |

#### SEC-03 — HTTP Request Actions (-10 points · additional -5 for insecure `http://`) · *Top-10 #3*

| | |
|---|---|
| **What it checks** | Whether any **Action** component (componentType = 1) in the agent calls `OpenApiConnection` or `httpRequest` connector types, **including insecure `http://` endpoints** |
| **Why it matters** | HTTP connectors can exfiltrate data to arbitrary endpoints and may bypass Data Loss Prevention (DLP) connector policies; plain `http://` endpoints additionally send data unencrypted |
| **Remediation** | Review each HTTP action for data exfiltration risk; replace any `http://` endpoint with `https://`; add the connector to your DLP policy in the Power Platform Admin Center |

#### SEC-04 — Email-Based Data Exfiltration (-15 points) · *Top-10 #4*

| | |
|---|---|
| **What it checks** | Whether any **Action** component uses an email-sending operation (e.g., Office 365 Outlook / SMTP send-email) that could move data outside the tenant |
| **Why it matters** | An agent that can send email is a channel for exfiltrating data to external recipients — especially when the recipient is a dynamic / AI-controlled value rather than a fixed address |
| **Remediation** | Confirm the recipient is a fixed, trusted internal address (not a variable or model output); restrict or remove email-send actions; cover the email connector with a DLP policy |

#### SEC-06 — Maker / Author Authentication (-10 points) · *Top-10 #6*

| | |
|---|---|
| **What it checks** | Whether a tool/connection appears to run with **maker credentials** rather than the signed-in end-user's identity |
| **Why it matters** | Actions that execute as the maker rather than the user break separation of duties and can grant end users access they would not otherwise have |
| **Remediation** | Switch connections to end-user authentication so actions run as the signed-in user |

#### SEC-08 — MCP / Custom Tools (-10 points) · *Top-10 #8*

| | |
|---|---|
| **What it checks** | Whether the agent uses Model Context Protocol (MCP) servers or other custom tools/connectors |
| **Why it matters** | MCP servers and custom tools extend the agent with external code and data access that sits outside standard connector governance and may not be covered by DLP |
| **Remediation** | Inventory each MCP server / custom tool, confirm it is from a trusted source, and bring it under DLP and admin review |

#### SEC-10 — Hardcoded Secrets / Credentials (-25 points) · *Top-10 #7*

| | |
|---|---|
| **What it checks** | Scans component content with a regular-expression matcher for hardcoded secrets — API keys, passwords, connection strings, bearer tokens, and JWTs (placeholder and environment-variable references are filtered out) |
| **Why it matters** | Secrets embedded in topics or actions can be read by anyone with access to the agent definition and are a direct path to credential compromise |
| **Remediation** | Remove the secret from the component and move it to a secured store (environment variable, Azure Key Vault, or a secured connection reference), then **rotate** the exposed credential. Matched values are shown **redacted** in the detail panel so the secret is never displayed in full |

#### SEC-05 — Orphaned Owner (-20 points) · *Top-10 #10*

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

### Microsoft Top-10 Mapping

Every security check is aligned to Microsoft's official *Top 10 Copilot Studio agent security risks* (February 2026). Each finding in the results is tagged with its Top-10 risk number so you can report against the Microsoft framework directly. Some risks are covered by other tabs rather than the Security Scanner.

| Top-10 # | Risk | Covered by |
|---|---|---|
| #1 | Overshared agent access | **Sharing & Access** tab |
| #2 | Missing authentication | SEC-01 |
| #3 | Risky HTTP request actions | SEC-03 |
| #4 | Email-based data exfiltration | SEC-04 |
| #5 | Dormant agents | **Adoption & Lifecycle** tab |
| #6 | Maker / author authentication | SEC-06 |
| #7 | Hardcoded credentials | SEC-10 |
| #8 | MCP / custom tools | SEC-08 |
| #9 | Generative orchestration without instructions | *Not yet checked* |
| #10 | Orphaned agents (no active owner) | SEC-05 |

> SEC-07 (agent not in a solution) is an additional ALM-hygiene check beyond Microsoft's Top 10.

### Detail Panel

Click any agent row to see:
- The full score and health label
- Each failed check with its description and its Microsoft Top-10 risk number
- The corresponding remediation step for each issue
- For SEC-10, a **redacted** preview of each matched secret

### Exporting Results

Click **📥 Export CSV** to save the scan results. The CSV includes: Agent Name, Score, Health, Issue Count, Failed Checks (pipe-separated), Remediation Steps (pipe-separated).

---

## 8. Sharing & Access

The **👥 Sharing & Access** tab audits who each agent (bot record) is shared with, so you can spot over-shared or broadly-exposed agents. It addresses **#1 (overshared agent access)** in Microsoft's *Top 10 Copilot Studio agent security risks*.

### Running an Audit

Click **Run Sharing Audit**. The plugin reads each agent's record-level sharing (the same shares the Copilot Studio "Share" dialog manages) and resolves each user/team to a display name.

### Columns

| Column | Description |
|---|---|
| **Agent Name** | Display name of the Copilot Studio agent |
| **Owner** | Owner full name |
| **Shared With** | Count of users/teams the agent is shared with (notes if a team/group is included) |
| **Exposure** | ✅ Owner only / 🟢 Shared (viewers) / 🟡 Shared with editors / 🔴 Shared to team/group |

Click any agent row to see each shared **principal** (user or team) and the exact **access rights** granted (e.g., Read, Write, Share). Teams and editors are highlighted as the broadest exposure.

### Exporting Results

Click **📥 Export CSV** to save the sharing audit for review or access attestation.

---

## 9. Knowledge Sources

The **📚 Knowledge Sources** tab inventories the grounding sources wired into each agent, so you can confirm what data each agent can answer from and flag data-governance risks.

### Running a Scan

Click **Run Knowledge Inventory**. The plugin reads each agent's knowledge components (Knowledge Sources and file attachments) and classifies each source.

### Columns

| Column | Description |
|---|---|
| **Agent Name** | Display name of the Copilot Studio agent |
| **Owner** | Owner full name |
| **Sources** | Count of knowledge sources (notes how many are public web) |
| **Source Types** | The kinds of source in use: 🌐 Public website / 📁 SharePoint / 📁 OneDrive / 🗄️ Dataverse / 📄 Uploaded file / 🔌 Graph connector |
| **Risk** | ✅ None / 🟢 Low / 🟡 Medium / 🔴 High |

Click any agent row to see each individual source with its type and URL/detail.

### Risk Flags

| Flag | Severity | Meaning |
|---|---|---|
| **KS-01** | 🔴 High | Public-website grounding — query text may leave the tenant for public web search; review against DLP / data-governance policy |
| **KS-05** | 🟡 Medium | An inactive knowledge component — the agent is no longer grounded on a source the maker may think is in use |
| **KS-03** | 🟢 Low | No knowledge sources — answers come from the base model and topics only (informational; many agents legitimately have none) |

### Exporting Results

Click **📥 Export CSV** to save the knowledge-source inventory.

---

## 10. Adoption & Lifecycle

The **📈 Adoption & Lifecycle** tab flags **dormant** and **orphaned** agents so you can clean up agent sprawl, reclaim licences and capacity, and reduce risk. It directly addresses **#5 (dormant agents)** in Microsoft's *Top 10 Copilot Studio agent security risks*.

### How it works (two-layer analysis)

The scan works in two layers so it produces useful results in every tenant:

- **Base layer (always available)** — uses **owner-disabled** status and **last-edited age** to estimate whether an agent is still maintained.
- **Usage layer (when available)** — if the `conversationtranscript` table is **retained** in the tenant, the scan adds **real usage** signals: the **last used date** and **conversation counts for the last 30 and 90 days**. The plugin discovers the transcript→agent link automatically; if transcripts are not retained, the usage columns read **n/a** and the verdict falls back to the base layer.

### Running a Scan

Click **Run Adoption Analysis**. Results appear as a table, one row per agent.

### Columns

| Column | Description |
|---|---|
| **Agent Name** | Display name of the Copilot Studio agent |
| **Owner** | Owner full name (⚠️ if owner is disabled) |
| **Last Edited** | Date the agent was last modified (with age in days) |
| **Last Used** | Date of the most recent conversation (when transcript data is available) |
| **30d / 90d** | Conversation counts in the last 30 / 90 days (when transcript data is available) |
| **Lifecycle** | 🟢 Active / 🟡 Watch / 🟠 Dormant / 🔴 Orphaned |

### Lifecycle Labels

| Label | Meaning |
|---|---|
| 🟢 Active | Recently used (or recently edited when no usage data) — healthy |
| 🟡 Watch | Some inactivity; review at next governance cycle |
| 🟠 Dormant | No recent usage / no edit in 90+ days — a candidate for retirement / licence cleanup |
| 🔴 Orphaned | Owner account is disabled — no accountable party |

Click any agent row to see the verdict reason and the underlying signals.

### Exporting Results

Click **📥 Export CSV** to save the adoption results for licence reviews or decommissioning workflows.

---

## 11. Deployment Readiness

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

## 12. ALM Diff

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

## 13. ALM & Dependencies

The **🧬 ALM & Dependencies** tab maps each agent's **solution membership** and its **forward dependency graph** — everything that must travel with the agent to run in another environment — and flags the ALM transport risks that break a deployment. Unlike ALM Diff (which compares one agent across two orgs), this tab audits the **connected environment only** and needs no second connection.

### Running the Map

Click **🧬 Run ALM & Dependency Map**. For every agent the plugin reads its solution memberships and queries the Dataverse dependency API (`RetrieveRequiredComponents`), then enriches the result with knowledge-source and MCP/custom-tool dependencies parsed from the agent's components.

### Columns

| Column | Description |
|---|---|
| **Agent Name** | Display name of the Copilot Studio agent |
| **Owner** | Owner full name |
| **Solution(s)** | ❌ None / ⚠️ Default only / the unmanaged solution name(s) the agent belongs to |
| **Dependencies** | Count of dependencies (notes how many are unpackaged or unconfigured) |
| **Risk** | ✅ None / 🟢 Low / 🟡 Medium / 🔴 High |
| **Flags** | The ALM-0x flag codes that fired |

Click any agent row to expand a detail tree: **📦 Solutions**, **🔗 Dependencies** (each with a health icon — ✅ OK / ⚠️ Unconfigured / ❌ Not packaged / 🌐 External), **❌ Missing / unconfigured**, and **⚠️ Risk flags**. Dependencies resolve to friendly names (connection references, flows, environment variables, child agents), not GUIDs.

### Risk Flags

| Flag | Severity | Meaning |
|---|---|---|
| **ALM-01** | 🔴 High | Agent is not in any solution — it cannot be moved between environments via ALM |
| **ALM-02** | 🔴 High | Agent is orphaned in the Default/Active layer only — add it to an unmanaged solution before it can be exported |
| **ALM-03** | 🔴 High | One or more required components are not packaged in the agent's unmanaged solution and will be missing on import |
| **ALM-04** | 🔴 High | A connection reference the agent uses has no connection mapped — actions will fail until configured |
| **ALM-05** | 🟡 Medium | An environment variable the agent depends on has no value in this environment |
| **ALM-06** | 🟡 Medium | A cloud flow the agent calls is not co-packaged in its solution |
| **ALM-07** | 🟡 Medium | A knowledge target points at an external/hardcoded site (e.g. public web, SharePoint URL) that won't repoint on import |
| **ALM-08** | 🟢 Low | Agent exists only as a managed component here — edit it upstream (development) and redeploy |

### Exporting Results

Click **📥 Export CSV** to save the ALM & dependency map — agent, owner, solutions, managed/unmanaged, dependency counts, risk, flags, and the full dependency list (pipe-separated).

> **Notes:** Connection references and environment variables are matched by object id against the environment's full lists, so their configured/value health is accurate regardless of component-type encoding. A dependency on a component type the plugin doesn't yet classify is shown honestly as "Component type N" rather than dropped.

---

## 14. Governance Report

The **📄 Export Governance Report** button on the **Dashboard** produces a single, self-contained HTML governance report you can hand to security or leadership — no plugin required to open it.

### What it contains

| Section | Contents |
|---|---|
| **KPI summary** | The Dashboard headline numbers (total agents, critical agents, no-auth agents, orphaned owners, dormant / orphaned agents, broadly-shared agents, public-web grounding, not ALM-deployable) |
| **Microsoft Top-10 scorecard** | How the environment scores against each of Microsoft's *Top 10 Copilot Studio agent security risks*, with the number of agents affected by each |
| **Security** | Per-agent security findings with their Top-10 risk numbers |
| **Sharing** | Per-agent sharing/exposure results |
| **Knowledge** | Per-agent knowledge-source inventory |
| **Adoption** | Per-agent staleness (Active / Watch / Dormant / Orphaned) |
| **ALM & dependencies** | Per-agent solution membership, dependency counts, unpackaged dependencies, and ALM risk flags |

### Generating the report

1. Open the **Dashboard** tab.
2. Click **📄 Export Governance Report** — the plugin runs the security, sharing, knowledge, and adoption scans automatically.
3. Choose a location to save the `.html` file, then choose whether to open it.
4. Open the file in any browser — it is fully self-contained (styles embedded, no external dependencies).

---

## 15. Exporting Data

### Agent Inventory CSV

Fields: `Name, Authentication, Owner, Owner Disabled, Status, In Solution, Created On, Last Modified`

### Security Scan CSV

Fields: `Agent Name, Score, Health, Issue Count, Failed Checks, Remediation Steps`

- Failed Checks and Remediation Steps are pipe-separated (`|`) within a single CSV cell
- Files are UTF-8 encoded and timestamped in the filename

### Sharing & Access CSV

Fields: `Agent Name, Owner, Share Count, Shared To Team, Has Editors, Exposure, Shared With`

### Knowledge Sources CSV

Exports the per-agent knowledge inventory: agent name, owner, source count, source types, risk level, and fired flags.

### Adoption & Lifecycle CSV

Fields include: agent name, owner, owner-disabled, status, in-solution, last edited, days since edit, last used, 30-/90-day conversation counts (when transcript data is available), lifecycle label, and the verdict reason.

### ALM & Dependencies CSV

Fields: `Agent, Owner, Solutions, Managed?, Dependency Count, Not Packaged, Unconfigured, Risk, Flags, Dependencies`

- Dependencies are pipe-separated (`|`) within a single cell, each shown as `type: name (health)`

### Governance Report (HTML)

A one-click, self-contained HTML report from the Dashboard. See [Governance Report](#14-governance-report).

---

## 16. Troubleshooting

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

## 17. Known Limitations

| Limitation | Details |
|---|---|
| **Environment Variables with Defaults** | DEP-03 flags env vars that have no `environmentvariablevalue` record, even if a `defaultvalue` is set on the definition. A default value is still technically a valid value; this may produce false positives for variables that rely on defaults. |
| **Carriage Return in CSV** | Values containing `\r` (carriage return without newline) are not quoted in CSV exports. This is rare in Dataverse field values but may cause formatting issues in some CSV readers. |
| **Component Content Truncation** | The Agent Inventory tab truncates component content to 200 characters for display. The full content is used for ALM Diff comparison. |
| **Copilot Studio Online Only** | The `bot` and `botcomponent` Dataverse tables only exist in Power Platform online environments. On-premise Dynamics 365 installations are not supported. |
| **Conversation Transcript Retention** | The Adoption & Lifecycle usage signals (last used date and 30-/90-day conversation counts) require the `conversationtranscript` table to be retained in the tenant. When transcripts are not retained, the staleness verdict is derived from owner-disabled status and last-edited age alone. |
| **Security content heuristics** | SEC-04 (email), SEC-06 (maker auth), and SEC-08 (MCP/tools) detect their risks by matching markers in the agent's component definition. They fail safe — no match means no flag — so a novel agent format may need the marker set extended. |
| **Dependency component types** | The ALM & Dependencies tab resolves connection references, environment variables, cloud flows, and child agents to friendly names. Other component types returned by the dependency API are listed honestly as "Component type N" rather than dropped; connection-reference / environment-variable health is matched by object id, so it stays accurate regardless. |
| **Read-Only** | This plugin is read-only — every tab (including Security, Sharing, Knowledge, and Adoption) only reads from Dataverse and never writes to or modifies data in your environment. |

---

*Copilot Studio Agent Health Monitor is an open-source XrmToolBox community plugin.*
*For issues or feature requests, visit the GitHub repository.*
