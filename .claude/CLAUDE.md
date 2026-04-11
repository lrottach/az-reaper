# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Azure Reaper is an Azure Functions application that automatically deletes Azure Resource Groups marked with a `CloudReaperLifetime` tag after a specified duration. The project uses .NET 10 with the isolated worker model.

- **Active code**: `src/` directory (.NET 10.0)
- **Stable archive**: `archive/` directory (.NET 6.0, previous version — do not modify)

## Rewrite Context

This project is a complete rewrite of the original Azure Reaper (.NET 6, in-process model) to modern .NET 10 with the isolated worker model. The old version is preserved in `archive/` for reference.

**Approach:** Step-by-step rewrite and test — each feature is built, verified, and tracked via GitHub issues before moving to the next.

**Goals:**
- Modern .NET 10 LTS with Azure Functions isolated worker model
- Azure SDK (`Azure.ResourceManager`) instead of manual REST API calls
- Azure Developer CLI (`azd`) for streamlined deployment
- Comprehensive Bicep infrastructure
- Improved documentation and a blog post about the rewrite

**CLI Prerequisites:**
- `dotnet` (10.0+) — installed
- `az` (Azure CLI) — installed
- `gh` (GitHub CLI) — installed
- `func` (Azure Functions Core Tools 4.x) — installed
- `azd` (Azure Developer CLI 1.x) — installed

## Build Commands

All commands should be run from the `src/AzureReaper.Functions` directory:

```bash
# Build
dotnet build

# Clean and build for release
dotnet clean -c Release && dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o bin/Release/net10.0/publish

# Run locally (requires Azurite running and func CLI installed)
func start
```

## Local Development Setup

Requires Docker or Podman for running Azurite (Azure Storage emulator). The project uses `docker-compose.yml` at the repo root.

1. Start Azurite for local storage emulation:
   ```bash
   # Docker
   docker compose up -d azurite

   # Podman
   podman compose up -d azurite
   ```

2. Create `src/AzureReaper.Functions/local.settings.json` (see `local.settings.sample.json`):
   ```json
   {
     "IsEncrypted": false,
     "Values": {
       "AzureWebJobsStorage": "UseDevelopmentStorage=true",
       "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
     }
   }
   ```

3. Test EventGrid trigger locally using `rest/eventgrid.http` or POST to:
   ```
   POST http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridTrigger
   Header: aeg-event-type: Notification
   ```

## Architecture

**Event Flow:**
1. EventGrid triggers on `Microsoft.Resources.ResourceWriteSuccess` events
2. `EventGridTrigger.cs` receives and validates the event
3. `AzureResourceEntity` (Durable Entity) manages resource state and deletion scheduling
4. `AzureResourceService` executes Azure SDK operations

**Key Components:**
- `EventGridTrigger.cs` - Entry point, parses EventGrid events
- `Entities/AzureResourceEntity.cs` - Durable entity for state management and orchestration
- `Services/AzureResourceService.cs` - Azure Resource Manager SDK wrapper
- `Common/TagHandler.cs` - Validates `CloudReaperLifetime` tag format
- `Common/StringHandler.cs` - Parses Azure resource IDs from event subjects

**Tag System (configurable via `LifetimeTagName` / `StatusTagName` environment variables):**
- `CloudReaperLifetime` - Required tag on resource groups, value is minutes until deletion
- `CloudReaperStatus` - Applied by the system as "Confirmed" when deletion is scheduled
- `CloudReaperDeletionTime` - Applied by the system with the ISO 8601 UTC timestamp of the scheduled deletion

## Infrastructure

Infrastructure as Code is in `infra/main.bicep` (subscription-scoped deployment).

## Development Guidelines

This project follows Azure Functions best practices based on official Microsoft documentation. When writing or modifying code, use the Microsoft Learn MCP tools to look up current guidance before implementing patterns for:

- Azure Functions hosting, triggers, and bindings
- Durable Functions entities and orchestrations
- Azure SDK usage (`Azure.ResourceManager`, `Azure.Identity`)
- Bicep templates and Azure Developer CLI (`azd`)

Always prefer the **isolated worker model** patterns over the legacy in-process model. Do not use deprecated APIs or packages.

**Branching & merges:** The `main` branch is protected. All work must happen on feature branches pushed to GitHub. Merging into `main` is only possible through a pull request.

**Commit messages** use conventional prefixes: `feat:`, `refactor:`, `fix:`, `docs:`.

**Pull request titles** use the format: `feature: <description>`.

## Technology Stack

- .NET 10.0 LTS with Azure Functions v4 (isolated worker model)
- `FunctionsApplication.CreateBuilder` hosting pattern with ASP.NET Core integration
- Durable Functions with Distributed Tracing V2 enabled
- Azure SDK (Azure.ResourceManager, Azure.Identity)
- EventGrid triggers
- Planned: Azure Developer CLI (`azd`) for deployment, GitHub Actions for CI/CD
