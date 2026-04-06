#!/bin/bash
set -euo pipefail

# ==============================================================================
# Post-deploy hook: Create EventGrid event subscription
#
# This script runs after 'azd deploy' to configure the EventGrid event
# subscription with the Function App's system key for webhook authentication.
# Variables are injected by azd from Terraform outputs.
# ==============================================================================

FUNCTION_APP_NAME="${AZURE_FUNCTION_APP_NAME}"
RESOURCE_GROUP_NAME="${AZURE_RESOURCE_GROUP_NAME}"
SYSTEM_TOPIC_NAME="${AZURE_EVENTGRID_SYSTEM_TOPIC_NAME}"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID}"
EVENT_SUBSCRIPTION_NAME="evs-azure-reaper"

echo "Retrieving Function App EventGrid system key..."

# Retry loop: the system key may not be available immediately after deployment
MAX_RETRIES=5
RETRY_DELAY=10
SYSTEM_KEY=""

for i in $(seq 1 $MAX_RETRIES); do
  SYSTEM_KEY=$(az functionapp keys list \
    --name "$FUNCTION_APP_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --query "systemKeys.eventgrid_extension" \
    --output tsv 2>/dev/null || echo "")

  if [ -n "$SYSTEM_KEY" ] && [ "$SYSTEM_KEY" != "null" ]; then
    break
  fi

  if [ "$i" -lt "$MAX_RETRIES" ]; then
    echo "System key not available yet, retrying in ${RETRY_DELAY}s... (attempt $i/$MAX_RETRIES)"
    sleep "$RETRY_DELAY"
  fi
done

if [ -z "$SYSTEM_KEY" ] || [ "$SYSTEM_KEY" = "null" ]; then
  echo "ERROR: Could not retrieve EventGrid system key after $MAX_RETRIES attempts."
  echo "The Function App may still be starting. Try running: azd hooks run postdeploy"
  exit 1
fi

WEBHOOK_URL="https://${FUNCTION_APP_NAME}.azurewebsites.net/runtime/webhooks/EventGrid?functionName=EventGridTrigger&code=${SYSTEM_KEY}"

echo "Creating EventGrid event subscription..."
az eventgrid system-topic event-subscription create \
  --name "$EVENT_SUBSCRIPTION_NAME" \
  --system-topic-name "$SYSTEM_TOPIC_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --endpoint-type webhook \
  --endpoint "$WEBHOOK_URL" \
  --included-event-types "Microsoft.Resources.ResourceWriteSuccess" \
  --subject-begins-with "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/" \
  --output none

echo "EventGrid event subscription '${EVENT_SUBSCRIPTION_NAME}' created successfully."
