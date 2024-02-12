using System;
using System.Collections.Generic;
using System.IO;
using Maverick.Client.ArangoDb.Public.Configuration;
using Microsoft.Extensions.Options;

namespace UserProfileService.Arango.UnitTests.V2.Mocks
{
    public class MockArangoConfigInOptionsMonitor : IOptionsMonitor<ArangoConfiguration>,
        IOptionsSnapshot<ArangoConfiguration>
    {
        /// <inheritdoc />
        public ArangoConfiguration CurrentValue { get; } = new ArangoConfiguration
        {
            ConnectionString = "Endpoints=http://localhost:8951;database=dbName;userName=root;password=1;",
            MinutesBetweenChecks = 1,
            ClusterConfiguration = new ArangoClusterConfiguration
            {
                DocumentCollections = new Dictionary<string, ArangoCollectionClusterConfiguration>
                {
                    {
                        "*", new ArangoCollectionClusterConfiguration
                        {
                            NumberOfShards = 3,
                            ReplicationFactor = 2,
                            WriteConcern = 2
                        }
                    },
                    {
                        "ProfileS", new ArangoCollectionClusterConfiguration
                        {
                            NumberOfShards = 1,
                            ReplicationFactor = 2,
                            WriteConcern = 1
                        }
                    }
                }
            }
        };

        /// <inheritdoc />
        public ArangoConfiguration Value => CurrentValue;

        public ArangoConfiguration Get(string name)
        {
            return CurrentValue;
        }

        /// <inheritdoc />
        public IDisposable OnChange(Action<ArangoConfiguration, string> listener)
        {
            return Stream.Null;
        }
    }
}
