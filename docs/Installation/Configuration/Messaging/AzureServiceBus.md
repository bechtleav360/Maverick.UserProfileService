# Azure Service Bus

If you want to use [AzureServicBus](https://learn.microsoft.com/en-us/azure/service-bus-messaging/service-bus-messaging-overview) for communication, you need to set `ServiceBus` as the value for the `Type` key. This will ensure that internal messaging uses Azure Service Bus.

An example configuration section could look like this:

```json
{
  "Messaging": {
    "ServiceBus": {
      "ConnectionString": "Endpoint=sb://<NamespaceName>.servicebus.windows.net/;
      SharedAccessKeyName=<KeyName>;SharedAccessKey=<KeyValue>",
    },
    "Type": "ServiceBus"
  }
}
```

`Endpoint` - The URL of your Service Bus namespace. This always starts with **sb://**, followed by your Service Bus namespace name, and ends with **.servicebus.windows**.net/.

`SharedAccessKeyName` - The name of the shared access policy you are using. This is usually a policy that grants access to the Service Bus namespace or a specific queue/topic.

`SharedAccessKey` - The key associated with the shared access policy. This is a secret value that acts like a password for the connection.

