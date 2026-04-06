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
  description = "Optional override for the resource group name"
  default     = ""
}

variable "storage_account_name" {
  type        = string
  description = "Optional override for the storage account name"
  default     = ""

  validation {
    condition     = var.storage_account_name == "" || can(regex("^[a-z0-9]{3,24}$", var.storage_account_name))
    error_message = "Storage account names must be 3-24 characters and use lowercase letters and numbers only."
  }
}

variable "function_app_name" {
  type        = string
  description = "Optional override for the function app name"
  default     = ""
}

variable "app_service_plan_name" {
  type        = string
  description = "Optional override for the app service plan name"
  default     = ""
}

variable "log_analytics_name" {
  type        = string
  description = "Optional override for the Log Analytics workspace name"
  default     = ""
}

variable "app_insights_name" {
  type        = string
  description = "Optional override for the Application Insights instance name"
  default     = ""
}

variable "eventgrid_system_topic_name" {
  type        = string
  description = "Optional override for the EventGrid system topic name"
  default     = ""
}

variable "eventgrid_event_subscription_name" {
  type        = string
  description = "Optional override for the EventGrid event subscription name"
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
