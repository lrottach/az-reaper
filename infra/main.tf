# ==============================================================================
# Shared locals and resource group
# ==============================================================================

locals {
  resource_group_name         = var.resource_group_name != "" ? var.resource_group_name : "rg-${var.AZURE_ENV_NAME}"
  storage_account_name        = var.storage_account_name != "" ? var.storage_account_name : "st${replace(var.AZURE_ENV_NAME, "-", "")}${random_string.suffix.result}"
  function_app_name           = var.function_app_name != "" ? var.function_app_name : "func-${var.AZURE_ENV_NAME}"
  app_service_plan_name       = var.app_service_plan_name != "" ? var.app_service_plan_name : "asp-${var.AZURE_ENV_NAME}"
  log_analytics_name          = var.log_analytics_name != "" ? var.log_analytics_name : "log-${var.AZURE_ENV_NAME}"
  app_insights_name           = var.app_insights_name != "" ? var.app_insights_name : "appi-${var.AZURE_ENV_NAME}"
  eventgrid_system_topic_name = var.eventgrid_system_topic_name != "" ? var.eventgrid_system_topic_name : "evgt-${var.AZURE_ENV_NAME}"

  default_tags = {
    "azd-env-name" = var.AZURE_ENV_NAME
    "project"      = "azure-reaper"
  }
}

resource "random_string" "suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = var.AZURE_LOCATION
  tags     = local.default_tags
}
