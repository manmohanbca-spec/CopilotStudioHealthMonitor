# Copilot Studio Agent Health Monitor

> An XrmToolBox plugin to audit, secure, govern, and validate Microsoft Copilot Studio agents across Power Platform environments. Fully **read-only** against Dataverse — it never writes to your environment.

![XrmToolBox](https://img.shields.io/badge/XrmToolBox-Plugin-0078D4?style=flat-square)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?style=flat-square)
![Platform](https://img.shields.io/badge/Power%20Platform-Dataverse-742774?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## Features

| Tab | What you get |
|---|---|
| **Dashboard** | KPI cards (total agents, critical count, no-auth count, orphaned owners, dormant / orphaned) + risk-ranked agent table with colour-coded health, jump buttons to every tab (including 📈 Adoption), and a **📄 Export Governance Report** button |
| **Agent Inventory** | Full agent list with filter/search, owner info, authentication mode, solution membership, creation dates, and expandable component details |
| **Security Scanner** | Automated 0–100 security score for every agent, aligned to Microsoft's official **"Top 10 Copilot Studio agent security risks"** (Feb 2026) — checks for no authentication, insecure `http://` & HTTP-action endpoints, email-based data exfiltration, maker/author authentication, MCP / custom tools, hardcoded secrets & credentials (regex scan with redacted previews), disabled owners, and solution membership. Every finding is tagged with its Top-10 risk number, and the header shows a clean / critical summary with per-issue remediation steps |
| **Sharing & Access** | Audits who each agent is shared with (users & teams) and at what access level, surfacing over-shared and broadly-exposed agents |
| **Knowledge Sources** | Inventories the knowledge sources (websites, files, Dataverse, SharePoint) wired into each agent and flags public-web grounding and inactive sources |
| **Adoption & Lifecycle** | Flags dormant and orphaned agents (agent sprawl) using owner status + last-edited age, and — when the conversation-transcript table is retained — real usage (last-used date + 30/90-day conversation counts), with a staleness label per agent |
| **Deployment Readiness** | 4 pre-deployment checks for a selected agent — optionally verified against a connected target environment (UAT/PROD) |
| **ALM Diff** | Side-by-side bot component comparison across two environments: Match / Content Differs / Missing in Target / Only in Target |
| **ALM & Dependencies** | Per-agent solution membership (managed/unmanaged, Default-only orphans) and a forward dependency map — connection references, environment variables, cloud flows, knowledge targets, MCP tools — with ALM transport risk flags (ALM-01…08) for dependencies that are unpackaged, unconfigured, or won't repoint across environments |
| **Governance Report** | One-click, self-contained HTML governance report (KPI summary + Microsoft Top-10 scorecard + per-section tables for security, sharing, knowledge, and adoption) to hand to security / leadership |

---

## Quick Start

1. Install via the **XrmToolBox Tool Library** → search *"Copilot Studio Agent Health Monitor"*
2. Connect XrmToolBox to your Dataverse environment
3. Open the plugin — agents load automatically
4. Use the **Dashboard** for an instant health overview, or drill into individual tabs

See the full [User Guide](USER_GUIDE.md) for detailed instructions on every feature.

---

## Installation

### Via XrmToolBox Tool Library (recommended)

1. Open XrmToolBox
2. Click **Tool Library**
3. Search for **Copilot Studio Agent Health Monitor**
4. Click **Install** → restart XrmToolBox

### Manual (DLL drop)

1. Download `CopilotStudioHealthMonitor.dll` from [Releases](../../releases)
2. Copy to `%APPDATA%\MscrmTools\XrmToolBox\Plugins\`
3. Restart XrmToolBox

---

## Security Checks

| Check ID | MS Top-10 | Description | Score Impact |
|---|---|---|---|
| SEC-01 | #2 | No Authentication configured | −30 |
| SEC-03 | #3 | HTTP Request / OpenApiConnection actions, incl. insecure `http://` | −10 (−15 if insecure) |
| SEC-04 | #4 | Email-sending action — data exfiltration risk | −15 |
| SEC-05 | #10 | Agent owner account is disabled | −20 |
| SEC-06 | #6 | Tool/connection uses maker (author) authentication | −10 |
| SEC-07 | — | Agent not included in any Dataverse solution (ALM hygiene) | −5 |
| SEC-08 | #8 | MCP / custom tools configured | −10 |
| SEC-10 | #7 | Possible hardcoded secret / credential (regex scan, redacted preview) | −25 |

Score ≥ 85 = 🟢 Healthy · Score 60–84 = 🟡 Needs Attention · Score < 60 = 🔴 Critical

> Checks are aligned to Microsoft's official **"Top 10 Copilot Studio agent security risks"** (Feb 2026); each finding is tagged with its Top-10 risk number. Risk #1 (oversharing) is covered by the **Sharing & Access** tab, #5 (dormant agents) by the **Adoption & Lifecycle** tab, and #9 (generative orchestration without instructions) is not yet checked.

---

## Deployment Checks

| Check ID | Description | Org |
|---|---|---|
| DEP-01 | Agent belongs to a Dataverse solution | Source |
| DEP-02 | Authentication is configured (not No Auth) | Source |
| DEP-03 | All environment variables have values | Target (if connected) |
| DEP-04 | All connection references are mapped | Target (if connected) |

---

## Requirements

- XrmToolBox 1.2024.9.69 or later
- .NET Framework 4.7.2
- Windows 10 / Windows Server 2016 or later
- Dataverse online environment with Copilot Studio enabled
- System Administrator or System Customizer role

---

## Building from Source

```powershell
# Clone and build
git clone https://github.com/manmohanbca-spec/CopilotStudioHealthMonitor.git
cd CopilotStudioHealthMonitor

# Build Release
& "D:\VS\MSBuild\Current\Bin\MSBuild.exe" `
    "CopilotStudioHealthMonitor\CopilotStudioHealthMonitor.csproj" `
    /p:Configuration=Release /t:Rebuild /nologo

# Deploy to XrmToolBox
Copy-Item "CopilotStudioHealthMonitor\bin\Release\CopilotStudioHealthMonitor.dll" `
    "$env:APPDATA\MscrmTools\XrmToolBox\Plugins\" -Force
```

---

## Known Limitations

- Read-only — never writes to your environment
- Requires Dataverse online (Copilot Studio is cloud-only)
- DEP-03 may report false positives for env vars that have a default value but no explicit value record
- Component content is truncated to 200 characters in the Inventory view (full content used for ALM Diff)

---

## Contributing

Pull requests are welcome. Please open an issue first for significant changes.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/my-feature`)
3. Run the test suite: `dotnet test CopilotStudioHealthMonitor.Tests\`
4. Submit a PR

---

## License

MIT — see [LICENSE](LICENSE)
