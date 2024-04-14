curl -X POST -H "aeg-event-type: Notification" -H "Content-Type: application/json" -d @./docs/sample-payload.json http://localhost:7071/runtime/webhooks/EventGrid?functionName=EventGridTrigger
