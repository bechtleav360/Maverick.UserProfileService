# Message broker connection

The UserProfileService uses [RabbitMq](https://www.rabbitmq.com/) to send its internal messages.

An example configuration section could look like this:

```json
{
  "Messaging": {
    "RabbitMQ": {
      "Host": "localhost",
      "Password": "myPassword",
      "Port": 5672,
      "User": "myUser",
      "VirtualHost": "/"
    },
    "Type": "RabbitMQ"
  }
}
```

`Host`, `Port` and `VirtualHost` define the endpoint of rabbitMQ.

`User` and `Password` are credential information to connect to RabbitMQ.
