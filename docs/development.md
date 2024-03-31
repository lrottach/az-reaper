# Local Development Guide

## Required tools
- Visual Studio Code (or similar IDE)
- Dotnet SDK 8.0 LTS
- Azure Functions Core Tools
- Azre CLI
- Azure Developer CLI
- Docker

## Setup
### Azurite
Azurite is a lightweight server clone of Azure Blob, Queue, and Table Storage that simulates most of the commands supported by it with minimal dependencies.
I prefer running Azurite for local development using Docker. You can run Azurite using the following command:

```bash
docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite
```

### Local Settings
The Azure Functions Core Tools uses a `local.settings.json` file to store local settings.
Create a `local.settings.json` file in the root of the project with the following content:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"
  }
}
```