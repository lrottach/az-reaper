# ==============================================================================
# Azure Function App and supporting resources
# ==============================================================================

# Storage Account (Durable Functions backend + Functions runtime)
resource "azurerm_storage_account" "reaper" {
  name                     = local.storage_account_name
  resource_group_name      = azurerm_resource_group.main.name
  location                 = azurerm_resource_group.main.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  tags = local.default_tags
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

# App Service Plan (Linux Consumption)
resource "azurerm_service_plan" "reaper" {
  name                = local.app_service_plan_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  os_type             = "Linux"
  sku_name            = "Y1"

  tags = local.default_tags
}

# Function App
resource "azurerm_linux_function_app" "reaper" {
  name                = local.function_app_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  service_plan_id     = azurerm_service_plan.reaper.id

  storage_account_name       = azurerm_storage_account.reaper.name
  storage_account_access_key = azurerm_storage_account.reaper.primary_access_key

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version              = "10.0"
      use_dotnet_isolated_runtime = true
    }
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"              = "dotnet-isolated"
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.reaper.connection_string
    "LifetimeTagName"                       = var.lifetime_tag_name
    "StatusTagName"                         = var.status_tag_name
  }

  tags = merge(local.default_tags, {
    "azd-service-name" = "functions"
  })
}

# Role Assignment: Contributor on subscription for managed identity
resource "azurerm_role_assignment" "contributor" {
  scope                = "/subscriptions/${var.AZURE_SUBSCRIPTION_ID}"
  role_definition_name = "Contributor"
  principal_id         = azurerm_linux_function_app.reaper.identity[0].principal_id
}
