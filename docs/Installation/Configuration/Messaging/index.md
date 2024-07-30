# Configure Messaging
The UPS can be configured to use two different queue communication systems for communication between its workers. You can use:

* [RabbitMq](RabbitMq.md)
* [AzureServiceBus](AzureServiceBus.md)

The important key is `Type`. This key configures the communication system. The JSON to configure the messaging looks similar to these examples:


```json
{
  "Messaging": {
    "MessageType": {
        ...
    },
    "Type": "MessageType"
  }
}
```
For the `Type` key, you can use the value `RabbitMq` or `AzureServiceBus`.
