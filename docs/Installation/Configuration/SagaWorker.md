# SagaWorker Configuration

A valid saga worker configuration can look like this. Please note that you should create a database in ArangoDB and PostgreSQL and grant permissions for the user. This is not done automatically:

```json
{
# Cleanup old data from the collections
"Cleanup": {
    "AssignmentProjection": null,
    "EventCollector": null,
    "Facade": null,
    "FirstLevelProjection": null,
    "Interval": "5:00:00:00",
    "Service": null
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
    "ClusterConfiguration": {
      "DocumentCollections": {
        "*": {
          "NumberOfShards": 3,
          "ReplicationFactor": 2,
          "WriteConcern": 1
        }
      },
      "EdgeCollections": {
        "*": {
          "NumberOfShards": 3,
          "ReplicationFactor": 2,
          "WriteConcern": 1
        }
      }
    },
    "ConnectionString": "Endpoints=http://localhost:8529;UserName=myUser;Password=myPassword;database=UserProfileService",
    "MinutesBetweenChecks": 60
  },

  "Routing": {
    "DiscardResponsePathBase": "",
    "PathBase": ""
  },

  # Seeding-Service is disabled
  "Seeding": {
    "Disabled": true
  },

  "Tracing": {
    "OtlpEndpoint": "",
    "ServiceName": "userprofile-saga-worker"
  },

 # Validation Rules for the worker
  "Validation": {
    "Commands": {
      "External": {
        "profile-deleted": false
      }
    },
    # Functions with the same name are not allowed
    "Internal": {
      "Function": {
        "DuplicateAllowed": false
      },
      # Groups with the same name are not allowed (case insensitive)
      "Group": {
        "Name": {
          "Duplicate": false,
          "IgnoreCase": true,
          "Regex": "^[a-zA-Z0-9ÄÖÜäöüß_\\]\\[\\-\\.\\\\ @]+$"
        }
      },
      # Users with a dublicated emails are not allowed
      "User": {
        "DuplicateEmailAllowed": false
      }
    }
  }
}
```
The saga woker is configured to allow access to all necessary third-party components through the localhost endpoints.
