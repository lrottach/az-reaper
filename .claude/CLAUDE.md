# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Azure Reaper is an Azure Functions application that automatically deletes Azure Resource Groups marked with a `ReaperLifetime` tag after a specified duration. The project uses .NET 10 with the isolated worker model.

- **Active code**: `src/` directory (NET 10.0)
- **Stable archive**: `archive/` directory (NET 6.0, previous version)

## Build Commands

All commands should be run from the `src/AzureReaper.Functions` directory:

```bash
# Build
dotnet build

# Clean and build for release
dotnet clean -c Release && dotnet build -c Release

# Publish for deployment
dotnet publish -c Release -o bin/Release/net10.0/publish

# Run locally (requires Azurite running)
func start
```

## Local Development Setup

1. Start Azurite for local storage emulation:
   ```bash
   docker compose up -d azurite
   ```

2. Create `src/AzureReaper.Functions/local.settings.json`:
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
- `Common/TagHandler.cs` - Validates `ReaperLifetime` tag format
- `Common/StringHandler.cs` - Parses Azure resource IDs from event subjects

**Tag System:**
- `ReaperLifetime` - Required tag on resource groups, value is minutes until deletion
- `ReaperStatus` - Applied by the system as "Approved" when deletion is scheduled

## Infrastructure

Infrastructure as Code is in `infra/main.bicep` (subscription-scoped deployment).

## Technology Stack

- .NET 10.0 with Azure Functions v4 (isolated worker model)
- `FunctionsApplication.CreateBuilder` hosting pattern with ASP.NET Core integration
- Durable Functions with Distributed Tracing V2 enabled
- Azure SDK (Azure.ResourceManager, Azure.Identity)
- EventGrid triggers
