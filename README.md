![Azure Reaper banner](./assets/reaper_banner.png)

# Azure Reaper

**In the age-old dance of creation and destruction, behold Azure Reaper, a guardian wrought in code and cloud. Its purpose noble, it wields the power to vanquish Azure resources marked by time’s decree, ensuring realms of development and testing stand uncluttered, their legacies preserved in the annals of digital lore.**

> [!IMPORTANT]
> Please note that Azure Reaper is currently undergoing a major rewrite to leverage the latest .NET and Azure Functions frameworks. The version available on the main branch is under active development and may be unstable. For those interested in the previous stable version, it can be found in the ./archive directory of this repository. I appreciate your interest and patience as I am working to improve the capabilities and performance of Azure Reaper.

Azure Reaper is a sophisticated automation tool built on the robust foundation of Azure Functions to streamline the management of your cloud environment. This solution specializes in automatically deleting groups of resources tagged to your specifications, making it ideal for development and test environments. With Azure Reaper, you not only save money by eliminating unnecessary resource sprawl, but you also reduce the burden of manual cleanup. Leverage this seamless integration into your workflow to increase efficiency, reduce costs, and maintain a focused, clutter-free cloud environment.
The idea is based on Jeff Holan's great [functions-csharp-entities-grimreaper](https://github.com/jeffhollan/functions-csharp-entities-grimreaper), rewritten in a more simple form without the Twilio SMS part. 

## Tags
Azure Reaper uses specific tags to manage the lifecycle of Azure resource groups. Below are the tags involved and their use cases:
| Tag Name  |  Description | Example Value  |  Responsibility | Comments |
|---|---|---|---|---|
|AzReaperLifetime|This tag is applied when the resource group is created by the engineer. It specifies the lifespan of the resource group in minutes before it should be deleted.|60 (for 60 minutes lifetime|User|
|AzReaperStatus|This tag is applied by Azure Reaper after successful validation and scheduling of the resource group’s deletion. It indicates that the resource group is approved for deletion.|Approved |Azure Reaper |Can be used to return comments or error messages from Azure Reaper|
|AzReaperDeletionTime|Could be used to message back the exact time and date of the scheduled death.  | 2024-05-31T15:30:00Z  | Azure Reaper  | Not yet implmeneted!|

## Limitations
Azure Reaper currently has the following limitations:
| Description  |  Status |
|---|---|
| It is not possible to stop the deletion of a scheduled resource group. 😅 However, a lock can be applied to prevent deletion.  | ✅ Workaround Available  |
| Azure Reaper has only been tested with a single subscription. Multi-subscription support is planned.  | 🔨 Planned Improvement  |
|Azure Reaper operates only at the Azure Resource Group level, not on individual resources| 🗒️ Current Functionality  |
These limitations will be actively addressed in future updates to help make Azure Reaper work and play better.

## Status
Azure Reaper is under active development and is constantly evolving. The capabilities and performance of the project are continually being improved. As development progresses, comprehensive documentation, getting started guides, and deployment instructions will be provided. Your patience and contributions are greatly appreciated. Stay tuned for update!
