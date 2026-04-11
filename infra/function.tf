# ==============================================================================
# Azure Function App and supporting resources (Flex Consumption)
# ==============================================================================

data "azurerm_client_config" "current" {}

# Storage Account (Durable Functions backend + deployment packages)
resource "azurerm_storage_account" "reaper" {
  name                            = local.storage_account_name
  resource_group_name             = azurerm_resource_group.main.name
  location                        = azurerm_resource_group.main.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "StorageV2"
  allow_nested_items_to_be_public = false
  https_traffic_only_enabled      = true
  min_tls_version                 = "TLS1_2"

  tags = local.default_tags
}

# Blob container for Flex Consumption deployment packages
resource "azurerm_storage_container" "deployment" {
  name                  = "deploymentpackage"
  storage_account_id    = azurerm_storage_account.reaper.id
  container_access_type = "private"
}

# Log Analytics Workspace
resource "azurerm_log_analytics_workspace" "reaper" {
  name                = local.log_analytics_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = local.default_tags
}

# Application Insights (workspace-based)
resource "azurerm_application_insights" "reaper" {
  name                = local.app_insights_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  workspace_id        = azurerm_log_analytics_workspace.reaper.id
  application_type    = "web"

  tags = local.default_tags
}

# App Service Plan (Flex Consumption)
resource "azurerm_service_plan" "reaper" {
  name                = local.app_service_plan_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "FC1"

  tags = local.default_tags
}

# Function App (Flex Consumption)
resource "azurerm_function_app_flex_consumption" "reaper" {
  name                = local.function_app_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.reaper.id

  # Deployment package storage (identity-based)
  storage_container_type      = "blobContainer"
  storage_container_endpoint  = "${azurerm_storage_account.reaper.primary_blob_endpoint}deploymentpackage"
  storage_authentication_type = "SystemAssignedIdentity"

  # Runtime
  runtime_name    = "dotnet-isolated"
  runtime_version = "10.0"

  # Scaling
  maximum_instance_count = 100
  instance_memory_in_mb  = 2048

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_insights_connection_string = azurerm_application_insights.reaper.connection_string
  }

  app_settings = {
    "AzureWebJobsStorage"              = ""
    "AzureWebJobsStorage__accountName" = azurerm_storage_account.reaper.name
    "AzureWebJobsStorage__credential"  = "managedidentity"
    "LifetimeTagName"                  = var.lifetime_tag_name
    "StatusTagName"                    = var.status_tag_name
    "DeletionTimeTagName"              = var.deletion_time_tag_name
  }

  tags = merge(local.default_tags, {
    "azd-service-name" = "functions"
  })
}

# ==============================================================================
# RBAC: Custom least-privilege role for Azure Reaper
# ==============================================================================

resource "azurerm_role_definition" "reaper" {
  name        = "Azure Reaper Operator (${var.AZURE_ENV_NAME})"
  scope       = "/subscriptions/${var.AZURE_SUBSCRIPTION_ID}"
  description = "Allows reading, tagging, and deleting resource groups for Azure Reaper"

  permissions {
    actions = [
      "Microsoft.Resources/subscriptions/resourceGroups/read",
      "Microsoft.Resources/subscriptions/resourceGroups/write",
      "Microsoft.Resources/subscriptions/resourceGroups/delete",
    ]
  }
}

resource "azurerm_role_assignment" "reaper_operator" {
  scope              = "/subscriptions/${var.AZURE_SUBSCRIPTION_ID}"
  role_definition_id = azurerm_role_definition.reaper.role_definition_resource_id
  principal_id       = azurerm_function_app_flex_consumption.reaper.identity[0].principal_id
}

# ==============================================================================
# RBAC: Storage access for managed identity
# ==============================================================================

resource "azurerm_role_assignment" "storage_blob" {
  scope                = azurerm_storage_account.reaper.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_function_app_flex_consumption.reaper.identity[0].principal_id
}

resource "azurerm_role_assignment" "deployer_storage_blob" {
  scope                = azurerm_storage_account.reaper.id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "azurerm_role_assignment" "storage_queue" {
  scope                = azurerm_storage_account.reaper.id
  role_definition_name = "Storage Queue Data Contributor"
  principal_id         = azurerm_function_app_flex_consumption.reaper.identity[0].principal_id
}

resource "azurerm_role_assignment" "storage_table" {
  scope                = azurerm_storage_account.reaper.id
  role_definition_name = "Storage Table Data Contributor"
  principal_id         = azurerm_function_app_flex_consumption.reaper.identity[0].principal_id
}

resource "time_sleep" "wait_for_deployer_storage_blob_rbac" {
  depends_on = [azurerm_role_assignment.deployer_storage_blob]

  create_duration = "45s"
}
