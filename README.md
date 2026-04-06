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
Azure Reaper is under active development and is constantly evolving. The capabilities and performance of the project are continually being improved.

# Deployment guide
Azure Reaper is deployed with Azure Developer CLI (`azd`) and Terraform.

1. Install the required tooling: `az`, `azd`, `dotnet` 10, and Azure Functions Core Tools 4.x.
2. Sign in to Azure:
   ```bash
   az login
   azd auth login
   ```
3. Create or select an `azd` environment and choose a short environment name such as `d1`:
   ```bash
   azd env new d1
   azd env set AZURE_LOCATION westeurope
   ```
4. Run the deployment:
   ```bash
   azd up
   ```

By default, resource names are generated from `AZURE_ENV_NAME` and `AZURE_LOCATION` using the pattern `<prefix>-<env>-azreaper-<location>`, for example `rg-d1-azreaper-westeurope`. The storage account uses the same inputs but is sanitized to satisfy Azure naming rules (lowercase alphanumeric only, maximum 24 characters).

You can still bring your own names by setting Terraform override variables in the `azd` environment before running `azd up`:

```bash
azd env set TF_VAR_resource_group_name rg-custom-reaper
azd env set TF_VAR_storage_account_name std1azreaperweu
azd env set TF_VAR_function_app_name func-custom-reaper
azd env set TF_VAR_app_service_plan_name asp-custom-reaper
azd env set TF_VAR_log_analytics_name log-custom-reaper
azd env set TF_VAR_app_insights_name appi-custom-reaper
azd env set TF_VAR_eventgrid_system_topic_name evgt-custom-reaper
azd env set TF_VAR_eventgrid_event_subscription_name evs-custom-reaper
```

After infrastructure provisioning, `azd` deploys the Function App and runs the postdeploy hook to create or update the Event Grid event subscription that targets the `EventGridTrigger` function.



<!--- Eraser file: https://app.eraser.io/workspace/l4XQEUndY4eCyx0QUlsY --->
