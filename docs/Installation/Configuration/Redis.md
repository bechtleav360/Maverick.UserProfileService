# Redis
The UserProfileService-Sync uses Redis as a temporary storage for the synchronized data.

An example configuration section could look like this:

```json
  "Redis": {
    "ServiceName": "redis",
    "AbortOnConnectFail": "False",
    "AllowAdmin": "True",
    "ConnectRetry": 5,
    "ConnectTimeout": 5000,
    "EndpointUrls": [
      "localhost:6379"
    ],
    "ExpirationTime": 7200,
    "Password": "",
    "User": ""
  }
```

The `EndpointUrls` define the endpoints for Redis. Please note that `EndpointUrls` is an array where you can store more than one Redis endpoint. In this section, Redis is only bound to localhost. The port **6379** is the standard port for Redis.

The `User` and `Password` are used for authentication to the Redis system.

The other configuration options for redis:

`ServiceName` -  The service name used to resolve a service via the [Sentinel](https://redis.io/docs/management/sentinel/).

`AbortOnConnectFail` - A connection will not be established while no servers are available.

`AllowAdmin` - Enables a range of commands that are considered risky.

`ConnectRetry` - The number of times to repeat connect attempts during initial Connect.
 
`ConnectTimeout` - Timeout (ms) for connect operations.

`ExpirationTime` - Expiration time after the stored values in redis expires and are deleted (in seconds).