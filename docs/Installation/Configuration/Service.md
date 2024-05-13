# Service Configuration

A valid service configuration can look like this. Please note that you should create a database in ArangoDB and PostgreSQL and grant permissions for the user. This is not done automatically:

```json
{
  "Delays": {
    "HealthCheck": "1:00:00",
    "HealthPush": "1:00:00"
  },

  "Features": {
    "UseSwaggerUI": true
  },

 "IdentitySettings": {
    "ApiName": "",
    "ApiSecret": "",
    "Authority": "",
    "EnableAnonymousImpersonation": false,
    "EnableAuthorization": false,
    "EnableCaching": false,
    "RequireHttpsMetadata": false
  },
  
  "Logging": {
    "EnableLogFile": false,
    "LogFileMaxHistory": 3,
    "LogFilePath": "logs",
    "LogFormat": "json",
    "LogLevel": {
      "default": "Information"
    }
  },

  "Marten": {
    "ConnectionString": "Host=localhost;Port=5432;Username=myUser;Password=myPassword;Database=UserProfileService",
    "DatabaseSchema": "UserProfile",
    "StreamNamePrefix": "ups",
    "SubscriptionName": "UserProfileServiceStream"
  },
  
  "Messaging": {
    "RabbitMQ": {
      "Host": "localhost",
      "Password": "guest",
      "Port": 5672,
      "User": "guest",
      "VirtualHost": "/"
    },
    "Type": "RabbitMQ"
  },
  
  "ProfileStorage": {
    "ConnectionString": "Endpoints=http://localhost:8529;UserName=myUser;Password=myPassword;database=UserProfileService",
    "MinutesBetweenChecks": 60
  },
  
  "Routing": {
    "DiscardResponsePathBase": "",
    "PathBase": ""
  },
 
  "SyncProxyConfiguration": {
    "Endpoint": ""
  },

  "TicketStore": {
    "Backend": "arangodb"
  },
  
  "Tracing": {
    "OtlpEndpoint": "",
    "ServiceName": "userprofile-service"
  },
  
  "UseForwardedHeaders": false
}
```

The service is configured to allow access to all necessary third-party components through the localhost endpoints.
