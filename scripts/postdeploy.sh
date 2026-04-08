#!/bin/bash
set -euo pipefail

# ==============================================================================
# Post-deploy hook: Create EventGrid event subscription
#
# This script runs after 'azd deploy' to configure the EventGrid event
# subscription to target the EventGridTrigger function directly via its
# Azure resource ID. Variables are injected by azd from Terraform outputs.
# ==============================================================================

FUNCTION_APP_NAME="${AZURE_FUNCTION_APP_NAME}"
RESOURCE_GROUP_NAME="${AZURE_RESOURCE_GROUP_NAME}"
SYSTEM_TOPIC_NAME="${AZURE_EVENTGRID_SYSTEM_TOPIC_NAME}"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID}"
EVENT_SUBSCRIPTION_NAME="${AZURE_EVENTGRID_EVENT_SUBSCRIPTION_NAME}"
FUNCTION_NAME="EventGridTrigger"
FUNCTION_RESOURCE_ID="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.Web/sites/${FUNCTION_APP_NAME}/functions/${FUNCTION_NAME}"

echo "Waiting for Azure Function resource to become available..."

# Retry loop: the function resource may not be discoverable immediately after deployment
MAX_RETRIES=5
RETRY_DELAY=10
FUNCTION_EXISTS="false"

for i in $(seq 1 $MAX_RETRIES); do
  if az functionapp function show \
    --name "$FUNCTION_APP_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --function-name "$FUNCTION_NAME" \
    --output none 2>/dev/null; then
    FUNCTION_EXISTS="true"
    break
  fi

  if [ "$i" -lt "$MAX_RETRIES" ]; then
    echo "Function resource not available yet, retrying in ${RETRY_DELAY}s... (attempt $i/$MAX_RETRIES)"
    sleep "$RETRY_DELAY"
  fi
done

if [ "$FUNCTION_EXISTS" != "true" ]; then
  echo "ERROR: Could not resolve Function resource '${FUNCTION_NAME}' after $MAX_RETRIES attempts."
  echo "The Function App deployment may still be finalizing. Try running: azd hooks run postdeploy"
  exit 1
fi

echo "Creating or updating EventGrid event subscription..."
if az eventgrid system-topic event-subscription show \
  --name "$EVENT_SUBSCRIPTION_NAME" \
  --system-topic-name "$SYSTEM_TOPIC_NAME" \
  --resource-group "$RESOURCE_GROUP_NAME" \
  --output none 2>/dev/null; then

  az eventgrid system-topic event-subscription update \
    --name "$EVENT_SUBSCRIPTION_NAME" \
    --system-topic-name "$SYSTEM_TOPIC_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --endpoint-type azurefunction \
    --endpoint "$FUNCTION_RESOURCE_ID" \
    --included-event-types "Microsoft.Resources.ResourceWriteSuccess" \
    --subject-begins-with "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/" \
    --output none
else
  az eventgrid system-topic event-subscription create \
    --name "$EVENT_SUBSCRIPTION_NAME" \
    --system-topic-name "$SYSTEM_TOPIC_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --endpoint-type azurefunction \
    --endpoint "$FUNCTION_RESOURCE_ID" \
    --included-event-types "Microsoft.Resources.ResourceWriteSuccess" \
    --subject-begins-with "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/" \
    --output none
fi

echo "EventGrid event subscription '${EVENT_SUBSCRIPTION_NAME}' configured successfully."
