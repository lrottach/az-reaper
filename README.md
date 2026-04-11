<p><a target="_blank" href="https://app.eraser.io/workspace/l4XQEUndY4eCyx0QUlsY" id="edit-in-eraser-github-link"><img alt="Edit in Eraser" src="https://firebasestorage.googleapis.com/v0/b/second-petal-295822.appspot.com/o/images%2Fgithub%2FOpen%20in%20Eraser.svg?alt=media&amp;token=968381c8-a7e7-472a-8ed6-4a6626da5501"></a></p>

[![SonarQube Cloud](https://sonarcloud.io/images/project_badges/sonarcloud-highlight.svg)](https://sonarcloud.io/summary/new_code?id=lrottach_az-reaper)

![Azure Reaper banner](./assets/reaper_banner.png "")

# Azure Reaper
**In the age-old dance of creation and destruction, behold Azure Reaper, a guardian wrought in code and cloud. Its purpose noble, it wields the power to vanquish Azure resources marked by time’s decree, ensuring realms of development and testing stand uncluttered, their legacies preserved in the annals of digital lore.**

>  [!IMPORTANT]
Please note that Azure Reaper is currently undergoing a major rewrite to leverage the latest .NET and Azure Functions frameworks. The version available on the main branch is under active development and may be unstable. For those interested in the previous stable version, it can be found in the ./archive directory of this repository. I appreciate your interest and patience as I am working to improve the capabilities and performance of Azure Reaper. 

Azure Reaper is a automation tool built on the robust foundation of Azure Functions to streamline the management of your cloud environment. This solution specializes in automatically deleting groups of resources tagged to your specifications, making it ideal for development and test environments. With Azure Reaper, you not only save money by eliminating unnecessary resource sprawl, but you also reduce the burden of manual cleanup. Leverage this seamless integration into your workflow to increase efficiency, reduce costs, and maintain a focused, clutter-free cloud environment.
The idea is based on Jeff Holan's great [﻿functions-csharp-entities-grimreaper](https://github.com/jeffhollan/functions-csharp-entities-grimreaper), rewritten in a more simple form without the Twilio SMS part. 

## Architecture
![Azure Reaper - High Level](/.eraser/l4XQEUndY4eCyx0QUlsY___In9Uw2nCBah8b789trG5jB2NNPv1___---figure---qetgun-3A-w2o2ntuvxo4---figure---pievCzn1kLeqIGOqKsGwqw.png "Azure Reaper - High Level")

## Tags
Azure Reaper uses specific tags to manage the lifecycle of Azure resource groups. Tag names are configurable via environment variables (`LifetimeTagName`, `StatusTagName`). Below are the default tags and their use cases:

| Tag Name | Description | Example Value | Responsibility | Comments |
| ----- | ----- | ----- | ----- | ----- |
| CloudReaperLifetime | This tag is applied when the resource group is created by the engineer. It specifies the lifespan of the resource group in minutes before it should be deleted. The value must be a positive integer (> 0). Values of 0, negative numbers, or non-integer strings are ignored. Configurable via `LifetimeTagName`. | 60 (for 60 minutes lifetime) | User |  |
| CloudReaperStatus | This tag is applied by Azure Reaper after successful validation and scheduling of the resource group’s deletion. It indicates that the resource group is confirmed for deletion. Configurable via `StatusTagName`. | Confirmed | Azure Reaper | Can be used to return comments or error messages from Azure Reaper |
| CloudReaperDeletionTime | Could be used to message back the exact time and date of the scheduled death. | 2024-05-31T15:30:00Z | Azure Reaper | Not yet implmeneted! |
## Limitations
Azure Reaper currently has the following limitations:

| Description | Status |
| ----- | ----- |
| It is not possible to stop the deletion of a scheduled resource group. 😅 However, a lock can be applied to prevent deletion. | ✅ Workaround Available |
| Azure Reaper has only been tested with a single subscription. Multi-subscription support is planned. | 🔨 Planned Improvement |
| Azure Reaper operates only at the Azure Resource Group level, not on individual resources | 🗒️ Current Functionality |
| These limitations will be actively addressed in future updates to help make Azure Reaper work and play better. |  |
## Status
Azure Reaper is under active development and is constantly evolving. The capabilities and performance of the project are continually being improved. As development progresses, comprehensive documentation, getting started guides, and deployment instructions will be provided. Your patience and contributions are greatly appreciated. Stay tuned for update!



# Local Development

### Prerequisites

| Tool | Version | Purpose |
|------|---------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | 10.0+ | Build and run the Functions app |
| [Azure Functions Core Tools](https://learn.microsoft.com/azure/azure-functions/functions-run-local) | 4.x | Local Functions runtime (`func start`) |
| [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) | latest | Azure authentication and management |
| [Azure Developer CLI](https://learn.microsoft.com/azure/developer/azure-developer-cli/install-azd) | 1.x | Streamlined deployment with `azd up` |
| [Terraform](https://developer.hashicorp.com/terraform/install) | latest | Infrastructure provisioning |
| [Docker](https://docs.docker.com/get-docker/) or [Podman](https://podman.io/) | latest | Run Azurite (Azure Storage emulator) |

> **Tip:** A [Dev Container](#dev-container) is included with all prerequisites pre-installed — just open the project in VS Code or GitHub Codespaces.

### Quick Start

**1. Clone the repository**

```bash
git clone https://github.com/lrottach/az-reaper.git
cd az-reaper
```

**2. Start Azurite**

```bash
docker compose up -d azurite   # or: podman compose up -d azurite
```

**3. Create local settings**

```bash
cp src/AzureReaper.Functions/local.settings.sample.json \
   src/AzureReaper.Functions/local.settings.json
```

**4. Build and run**

```bash
cd src/AzureReaper.Functions
dotnet build
func start
```

The function app will start at `http://localhost:7071`.

### Testing the EventGrid Trigger

The function listens for EventGrid events at:

```
POST http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridTrigger
```

Required header: `aeg-event-type: Notification`

**Using VS Code REST Client:** Open `rest/eventgrid.http` and click "Send Request".

**Using curl:**

```bash
curl -X POST \
  -H "Content-Type: application/json" \
  -H "aeg-event-type: Notification" \
  -d @rest/payload.example.json \
  http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridTrigger
```

### Environment Variables

Configured in `src/AzureReaper.Functions/local.settings.json` (see [`local.settings.sample.json`](src/AzureReaper.Functions/local.settings.sample.json) for a template):

| Variable | Default | Mandatory | Description |
|----------|---------|-----------|-------------|
| `AzureWebJobsStorage` | `UseDevelopmentStorage=true` | Yes | Storage connection — uses local Azurite |
| `FUNCTIONS_WORKER_RUNTIME` | `dotnet-isolated` | Yes | Required for the isolated worker model |
| `LifetimeTagName` | `CloudReaperLifetime` | No | Custom tag name for resource group lifetime (minutes) |
| `StatusTagName` | `CloudReaperStatus` | No | Custom tag name applied when deletion is scheduled |

### Dev Container

The repository includes a Dev Container (`.devcontainer/`) that provides a ready-to-go environment with .NET 10, Azure CLI, Azure Functions Core Tools, Docker, and GitHub CLI pre-installed. Open the project in VS Code with the [Dev Containers extension](https://marketplace.visualstudio.com/items?itemName=ms-vscode-remote.remote-containers) or use [GitHub Codespaces](https://github.com/features/codespaces) to get started instantly.


<!--- Eraser file: https://app.eraser.io/workspace/l4XQEUndY4eCyx0QUlsY --->
