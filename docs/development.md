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

Start Azurite using Docker Compose (or Podman Compose):

```bash
# Start in background
docker compose up -d azurite

# View logs
docker compose logs -f azurite

# Stop
docker compose down
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