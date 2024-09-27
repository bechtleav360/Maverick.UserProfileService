# RabbitMQ

If you want to use [RabbitMq](https://www.rabbitmq.com/) for communication, you need to set `RabbitMQ` as the value for the `Type` key. This will ensure that internal messaging uses RabbitMQ.


An example configuration section could look like this:

??? abstract "RabbitMQ example configuration"
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

`Host` - The RabbitMQ server's hostname or IP address.

`Port` - The port number on which RabbitMQ is listening.

`VirtualHost` - The virtual host in RabbitMQ to which you want to connect.

`User` - The username used to authenticate with RabbitMQ.

`Password` - The password associated with the RabbitMQ user.

