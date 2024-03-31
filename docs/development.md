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
