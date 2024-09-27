# Message Configuration
The UPS can be configured to use two different queue communication systems for communication between its workers. You can use:

* [RabbitMq](RabbitMq.md)
* [ServiceBus](AzureServiceBus.md)

The important key is `Type`. This key configures the communication system. The JSON to configure the messaging looks similar to these examples:

??? abstract "Messaging configuration"
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
For the `Type` key, you can use the value `RabbitMq` or `ServiceBus`. The keywords are case-insensitive, so it doesn't matter how you write them.
