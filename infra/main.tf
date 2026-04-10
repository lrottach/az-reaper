# ==============================================================================
# Shared locals and resource group
# ==============================================================================

locals {
  normalized_env_name               = regexreplace(lower(var.AZURE_ENV_NAME), "[^a-z0-9]", "-")
  normalized_location               = regexreplace(lower(var.AZURE_LOCATION), "[^a-z0-9]", "-")
  name_suffix                       = "${local.normalized_env_name}-azreaper-${local.normalized_location}"
  storage_account_default_name      = substr(regexreplace("st${local.normalized_env_name}azreaper${local.normalized_location}", "[^a-z0-9]", ""), 0, 24)
  resource_group_name               = var.resource_group_name != "" ? var.resource_group_name : "rg-${local.name_suffix}"
  storage_account_name              = var.storage_account_name != "" ? var.storage_account_name : local.storage_account_default_name
  function_app_name                 = var.function_app_name != "" ? var.function_app_name : "func-${local.name_suffix}"
  app_service_plan_name             = var.app_service_plan_name != "" ? var.app_service_plan_name : "asp-${local.name_suffix}"
  log_analytics_name                = var.log_analytics_name != "" ? var.log_analytics_name : "log-${local.name_suffix}"
  app_insights_name                 = var.app_insights_name != "" ? var.app_insights_name : "appi-${local.name_suffix}"
  eventgrid_system_topic_name       = var.eventgrid_system_topic_name != "" ? var.eventgrid_system_topic_name : "evgt-${local.name_suffix}"
  eventgrid_event_subscription_name = var.eventgrid_event_subscription_name != "" ? var.eventgrid_event_subscription_name : "evs-${local.name_suffix}"

  default_tags = {
    "azd-env-name" = var.AZURE_ENV_NAME
    "project"      = "azure-reaper"
  }
}

resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.AZURE_LOCATION
  tags     = local.default_tags
}
