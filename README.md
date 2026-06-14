# Copilot Studio Agent Health Monitor

> An XrmToolBox plugin to audit, secure, and validate Microsoft Copilot Studio agents across Power Platform environments.

![XrmToolBox](https://img.shields.io/badge/XrmToolBox-Plugin-0078D4?style=flat-square)
![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?style=flat-square)
![Platform](https://img.shields.io/badge/Power%20Platform-Dataverse-742774?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)

---

## Features

| Tab | What you get |
|---|---|
| **Dashboard** | KPI cards (total agents, critical count, no-auth count, orphaned owners) + risk-ranked agent table with colour-coded health |
| **Agent Inventory** | Full agent list with filter/search, owner info, authentication mode, solution membership, creation dates, and expandable component details |
| **Security Scanner** | Automated 0–100 security score for every agent across 4 checks (authentication, HTTP actions, owner status, solution membership) with per-issue remediation steps |
| **Deployment Readiness** | 4 pre-deployment checks for a selected agent — optionally verified against a connected target environment (UAT/PROD) |
| **ALM Diff** | Side-by-side bot component comparison across two environments: Match / Content Differs / Missing in Target / Only in Target |

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

| Check ID | Description | Score Impact |
|---|---|---|
| SEC-01 | No Authentication configured | −30 |
| SEC-03 | HTTP Request / OpenApiConnection actions detected | −10 |
| SEC-05 | Agent owner account is disabled | −20 |
| SEC-07 | Agent not included in any Dataverse solution | −5 |

Score ≥ 85 = 🟢 Healthy · Score 60–84 = 🟡 Needs Attention · Score < 60 = 🔴 Critical

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
