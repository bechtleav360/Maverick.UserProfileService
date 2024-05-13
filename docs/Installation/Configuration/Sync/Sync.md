# Sync Configuration
A valid sync configuration can look like this. Please note that you should create a database in ArangoDB and PostgreSQL and grant permissions for the user. This is not done automatically.
The LDAP configuration currently in use is invalid; it's purely an EXAMPLE. Here's a guide on how to properly [configure the LDAP Connector](LdapConfiguration.md) for synchronization.

```json
{
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
      "Password": "1",
      "Port": 5672,
      "User": "sb",
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
  "Redis": {
    "AbortOnConnectFail": false,
    "AllowAdmin": true,
    "ConnectRetry": 5,
    "ConnectTimeout": 5000,
    "EndpointUrls": [
      "localhost:6379"
    ],
    "ExpirationTime": 7200,
    "Password": null,
    "User": null
  },
  "Routing": {
    "DiscardResponsePathBase": "",
    "PathBase": ""
  },
  "SyncConfiguration": {
    "SourceConfiguration": {
      "Systems": {"Ldap": {
        "EntitiesMapping": {
          "DisplayName": "displayname",
          "Email": "mail",
          "FirstName": "givenName",
          "LastName": "sn",
          "Name": "Name",
          "UserName": "cn"
        },
        "LdapConfiguration": [
          {
            "Connection": {
              "AuthenticationType": "None",
              "BasePath": "dc=ad, dc=example, dc=com",
              "ConnectionString": "LDAP://ad.exmpale.com",
              "Description": "Default AD of A365 development environment",
              "IgnoreCertificate": false,
              "Port": 389,
              "ServiceUser": "CN=dev,OU=ExampleOU,OU=ExampleOU2,OU=DEVOU,DC=ad,DC=example,DC=com",
              "ServiceUserPassword": "Password",
              "UseSsl": false
            },
            "LdapQueries": [
              {
                "Filter": "(&(|(objectClass=user)(objectClass=inetOrgPerson))(!(objectClass=computer))(!(UserAccountControl:1.2.840.113556.1.4.803:=2)))",
                "SearchBase": "OU=Users,OU=Accounts,OU=Management"
              }
            ]
          }
        ],
        "Source": {
          "users": {
            "ForceDelete": "False",
            "Operations": "Add,Update,Delete"
          }
        },
        "Validation": {
          "Commands": {
            "External": {
              "profile-deleted": false
            }
          },
          "Internal": {
            "User": {
              "DuplicateEmailAllowed": false
            }
          }
        }
      }
    }
    },
    "Tracing": {
      "OtlpEndpoint": "",
      "ServiceName": "userprofile-sync"
    }
  }
}
```
The service is configured to allow access to all necessary third-party components through the localhost endpoints.