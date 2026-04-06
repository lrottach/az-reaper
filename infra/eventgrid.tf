# ==============================================================================
# EventGrid System Topic (subscription-scoped)
#
# The event subscription is created by the postdeploy hook (scripts/postdeploy.sh)
# because it requires the Function App's system key, which is only available
# after the app is deployed and running.
# ==============================================================================

resource "azurerm_eventgrid_system_topic" "reaper" {
  name                   = local.eventgrid_system_topic_name
  resource_group_name    = azurerm_resource_group.main.name
  location               = "global"
  source_resource_id     = "/subscriptions/${var.AZURE_SUBSCRIPTION_ID}"
  topic_type             = "Microsoft.Resources.Subscriptions"

  tags = local.default_tags
}
