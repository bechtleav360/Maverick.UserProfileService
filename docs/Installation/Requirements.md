# Requirements

## Third-party components
To start the service, you need at least the Service and the SagaWorker components, along with the following third-party components:

* [ArangoDb](https://www.arangodb.com/) - open-source graph-and document database where user data will be stored. [How to configure Arangodb](Configuration/Database%20Connection.md).
* [RabbitMq](https://www.rabbitmq.com/) - multi-protocol messaging and streaming broker - used to send messages between applications. [How to configure Rabbitmq](Configuration/Message%20Broker%20Connection.md).
* [PostgreSQL](https://www.postgresql.org/) - fast relational database used as volatile store and eventStore. [How to configure PostgreSQL](Configuration/Database%20Connection.md).

If you want to use UPS-Sync, you will also need this third-party component:

* [redis](https://redis.com/) - open source, in-memory data structure store, used as a database, cache, and message broker. [How to configure redis](Configuration/Redis.md).

## Running the UPS from Code
To start using the UserProfileService code, you will need the latest .NET 8.0 SDK, which you can download from [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0), and the Git source system, available for download [here](https://git-scm.com/downloads).

## Running the UPS with Docker-Images
If you want to use the UPS from Docker images, you'll need to install Docker first. For convenience, you will also need Docker Compose, which can be downloaded [here](https://docs.docker.com/compose/install/). You can also get the latest images from `ghcr.io/bechtleav360`.

## Configure the UPS
To configure the UPS, SagaWorker, or the Sync component, refer to the [Configuration section](Configuration/Base%20Path.md).