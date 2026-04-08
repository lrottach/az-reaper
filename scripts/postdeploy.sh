#!/bin/bash
set -euo pipefail

# ==============================================================================
# Post-deploy hook: Create EventGrid event subscription
#
# This script runs after 'azd deploy' to configure the EventGrid event
# subscription. It tries the native Azure Function endpoint first and falls
# back to the Event Grid webhook endpoint if the hosting plan does not accept
# the resource-ID-based target yet. Variables are injected by azd from
# Terraform outputs.
# ==============================================================================

FUNCTION_APP_NAME="${AZURE_FUNCTION_APP_NAME}"
RESOURCE_GROUP_NAME="${AZURE_RESOURCE_GROUP_NAME}"
SYSTEM_TOPIC_NAME="${AZURE_EVENTGRID_SYSTEM_TOPIC_NAME}"
SUBSCRIPTION_ID="${AZURE_SUBSCRIPTION_ID}"
EVENT_SUBSCRIPTION_NAME="${AZURE_EVENTGRID_EVENT_SUBSCRIPTION_NAME}"
FUNCTION_NAME="EventGridTrigger"
FUNCTION_RESOURCE_ID="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP_NAME}/providers/Microsoft.Web/sites/${FUNCTION_APP_NAME}/functions/${FUNCTION_NAME}"
MAX_RETRIES=5
RETRY_DELAY=10

configure_event_subscription() {
  local endpoint_type="$1"
  local endpoint="$2"

  if az eventgrid system-topic event-subscription show \
    --name "$EVENT_SUBSCRIPTION_NAME" \
    --system-topic-name "$SYSTEM_TOPIC_NAME" \
    --resource-group "$RESOURCE_GROUP_NAME" \
    --output none 2>/dev/null; then

    az eventgrid system-topic event-subscription update \
      --name "$EVENT_SUBSCRIPTION_NAME" \
      --system-topic-name "$SYSTEM_TOPIC_NAME" \
      --resource-group "$RESOURCE_GROUP_NAME" \
      --endpoint-type "$endpoint_type" \
      --endpoint "$endpoint" \
      --included-event-types "Microsoft.Resources.ResourceWriteSuccess" \
      --subject-begins-with "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/" \
      --output none
  else
    az eventgrid system-topic event-subscription create \
      --name "$EVENT_SUBSCRIPTION_NAME" \
      --system-topic-name "$SYSTEM_TOPIC_NAME" \
      --resource-group "$RESOURCE_GROUP_NAME" \
      --endpoint-type "$endpoint_type" \
      --endpoint "$endpoint" \
      --included-event-types "Microsoft.Resources.ResourceWriteSuccess" \
      --subject-begins-with "/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/" \
      --output none
  fi

  return $?
}

echo "Waiting for Azure Function resource to become available..."

# Retry loop: the function ARM resource may not be discoverable immediately after deployment
FUNCTION_EXISTS="false"

for i in $(seq 1 $MAX_RETRIES); do
  if az resource show \
    --ids "$FUNCTION_RESOURCE_ID" \
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
echo "Trying native Azure Function endpoint..."

set +e
NATIVE_OUTPUT=$(configure_event_subscription "azurefunction" "$FUNCTION_RESOURCE_ID" 2>&1)
NATIVE_EXIT_CODE=$?
set -e

if [ "$NATIVE_EXIT_CODE" -eq 0 ]; then
  echo "EventGrid event subscription '${EVENT_SUBSCRIPTION_NAME}' configured successfully."
  exit 0
fi

if echo "$NATIVE_OUTPUT" | grep -Eq "Endpoint validation|Destination endpoint not found|invalid trigger type"; then
  echo "Native Azure Function endpoint is not available yet on this hosting plan. Falling back to webhook delivery."
else
  echo "$NATIVE_OUTPUT" >&2
  exit "$NATIVE_EXIT_CODE"
fi

echo "Retrieving Function App EventGrid system key for webhook fallback..."

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

WEBHOOK_URL="https://${FUNCTION_APP_NAME}.azurewebsites.net/runtime/webhooks/EventGrid?functionName=${FUNCTION_NAME}&code=${SYSTEM_KEY}"
configure_event_subscription "webhook" "$WEBHOOK_URL"

echo "EventGrid event subscription '${EVENT_SUBSCRIPTION_NAME}' configured successfully."
