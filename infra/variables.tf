# ==============================================================================
# Variables injected by Azure Developer CLI
# ==============================================================================

variable "AZURE_ENV_NAME" {
  type        = string
  description = "Name of the azd environment"
}

variable "AZURE_LOCATION" {
  type        = string
  description = "Azure region for all resources"
}

variable "AZURE_SUBSCRIPTION_ID" {
  type        = string
  description = "Azure subscription ID"
}

# ==============================================================================
# Resource naming variables (defaults computed from AZURE_ENV_NAME in locals)
# ==============================================================================

variable "resource_group_name" {
  type        = string
  description = "Name of the resource group"
  default     = ""
}

variable "storage_account_name" {
  type        = string
  description = "Name of the storage account (3-24 chars, lowercase alphanumeric only)"

  validation {
    condition     = can(regex("^[a-z0-9]{3,24}$", var.storage_account_name))
    error_message = "Storage account name must be 3-24 characters, lowercase letters and numbers only."
  }
}

variable "function_app_name" {
  type        = string
  description = "Name of the function app"
  default     = ""
}

variable "app_service_plan_name" {
  type        = string
  description = "Name of the app service plan"
  default     = ""
}

variable "log_analytics_name" {
  type        = string
  description = "Name of the Log Analytics workspace"
  default     = ""
}

variable "app_insights_name" {
  type        = string
  description = "Name of the Application Insights instance"
  default     = ""
}

variable "eventgrid_system_topic_name" {
  type        = string
  description = "Name of the EventGrid system topic"
  default     = ""
}

# ==============================================================================
# Application configuration
# ==============================================================================

variable "lifetime_tag_name" {
  type        = string
  description = "Tag name for resource group lifetime (minutes until deletion)"
  default     = "CloudReaperLifetime"
}

variable "status_tag_name" {
  type        = string
  description = "Tag name for reaper status tracking"
  default     = "CloudReaperStatus"
}
