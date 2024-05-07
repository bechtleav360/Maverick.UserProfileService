# Database Connections

## ArangoDb

The UserProfileService uses a graph database called [ArangoDb](https://www.arangodb.com/).

Example configuration section (as pasrt of the complete appsettings file):

```json
{
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
  }
}
```

The `connection string` contains the endpoint of the ArangoDb graph databse, the credentials and the database to use.

_Side note:_ The specified user must have manage permissions for this database. The service will create collections and therefore needs additional rights.

The `cluster configuration` contains information about sharding in a cluster environment when collections are created. It will be ignored on _single-node_ installations.

`MinutesBetweenChecks` defines the timespan the database initializer unit will wait until it will ensure all collections has been created. It will do this at the starting of the service as well.  
This shall minimize the requests sending to ArangoDb during execution of the application.

## PostgreSQL

The UserProfileService uses [PostgreSQL](https://www.postgresql.org/) as a relational database. It can be configured as follows:
 
The `Connection string` defines all parameters to establish a database connection (see [NpgSql docs - connection string parameters](https://www.npgsql.org/doc/connection-string-parameters.html]))

`DatabaseSchema` defines the name of the schema to be used.

Example:

```json
{
  "Marten": {
    "ConnectionString": "Host=localhost;Port=5432;Username=myUser;Password=myPassword;Database=UserProfileService",
    "DatabaseSchema": "UserProfile"
  }
}
```