# ==============================================================================
# Outputs consumed by azd and the postdeploy hook
# ==============================================================================

output "AZURE_RESOURCE_GROUP_NAME" {
  value = azurerm_resource_group.main.name
}

output "AZURE_FUNCTION_APP_NAME" {
  value = azurerm_linux_function_app.reaper.name
}

output "AZURE_FUNCTION_URI" {
  value = "https://${azurerm_linux_function_app.reaper.default_hostname}"
}

output "AZURE_EVENTGRID_SYSTEM_TOPIC_NAME" {
  value = azurerm_eventgrid_system_topic.reaper.name
}

output "AZURE_EVENTGRID_EVENT_SUBSCRIPTION_NAME" {
  value = local.eventgrid_event_subscription_name
}

output "AZURE_SUBSCRIPTION_ID" {
  value = var.AZURE_SUBSCRIPTION_ID
}
