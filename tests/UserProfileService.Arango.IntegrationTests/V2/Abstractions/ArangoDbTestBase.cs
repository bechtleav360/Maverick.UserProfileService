using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using AutoMapper;
using JsonSubTypes;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.Client.ArangoDb.Public.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.IntegrationTests.V2.Constants;
using UserProfileService.Arango.IntegrationTests.V2.Implementations;
using UserProfileService.Arango.IntegrationTests.V2.Mocks;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Implementations;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.EventCollector.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Abstraction.Stores;
using MappingProfiles = UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.MappingProfiles;

namespace UserProfileService.Arango.IntegrationTests.V2.Abstractions
{
    public abstract class ArangoDbTestBase
    {
        public const string ArangoDbClientName = "ArangoTest";
        public const string ArangoDbClientEventLogStoreName = "ArangoTestEventLogStore";
        public const string ReadTestPrefix = "readTest_";
        public const string WriteTestPrefix = "writeTest_";
        public const string FirstLevelProjectionPrefix = "firstLevel_";
        public const string FirstLevelProjectionReadPrefix = "firstLevelRead_";
        public const string SecondLevelProjectionPrefix = "secondLevel_";
        public const string WriteTestQueryPrefix = "writeTestQuery_";
        public const string TicketsTestPrefix = "ticketsTest_";
        public const string EventCollectorTestPrefix = "eventCollectorTest_";
        public const string ArangoSyncEntityStoreTestPrefix = "syncTest_";
        public const string SecondLevelAssignmentsReadPrefix = "secondLevelAssignmentRead_";
        public const string SecondLevelAssignmentReadQueryPrefix = "secondLevelAssignmentReadQuery_";
        
        protected IServiceCollection Services { get; }
        protected IConfigurationRoot Configuration { get; }

        protected static JsonSerializerSettings DefaultSerializerSettings =>
            new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                Converters =
                {
                    new StringEnumConverter()
                }
            };

        protected ArangoDbTestBase()
        {
            Configuration = LoadConfig();

            Services = new ServiceCollection();
            Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
            Services.AddLogging(b => b.AddDebug().SetMinimumLevel(LogLevel.Debug).AddSimpleLogMessageCheckLogger());
            Services.AddSingleton(Configuration);
            Services.Configure<ArangoConfiguration>(Configuration.GetSection(ProfileStorageConfigKey));

            Services.AddArangoClientFactory()
                .AddArangoClient(
                    ArangoDbClientName,
                    p => p.GetRequiredService<IOptionsSnapshot<ArangoConfiguration>>()?.Value,
                    defaultSerializerSettings: new JsonSerializerSettings
                    {
                        Converters = WellKnownJsonConverters
                            .GetDefaultProfileConverters()
                            .Append(
                                JsonSubtypesConverterBuilder
                                    .Of<TicketBase>(nameof(TicketBase.Type))
                                    .RegisterSubtype<TicketA>("TicketA")
                                    .RegisterSubtype<TicketB>("TicketB")
                                    .Build())
                            .Concat(
                                WellKnownJsonConverters
                                    .GetDefaultFirstLevelProjectionConverters())
                            .ToList()
                    })
                .AddArangoClient(
                    ArangoDbClientEventLogStoreName,
                    p => p.GetRequiredService<IOptionsSnapshot<ArangoConfiguration>>()?.Value,
                    defaultSerializerSettings: new JsonSerializerSettings
                    {
                        Converters = WellKnownJsonConverters
                            .GetDefaultProfileConverters()
                            .Append(new StringEnumConverter())
                            .Append(new EventLogTupleReadOnlyMemoryJsonConverter())
                            .Append(new EventLogIgnoreEventJsonConverter())
                            .ToList(),
                        NullValueHandling = NullValueHandling.Ignore
                    });

            Services.AddSingleton<IDbInitializer, MockArangoDbInitializer>();

            Services.AddScoped<IReadService>(
                p => new ArangoReadService(
                    p,
                    p.GetRequiredService<IDbInitializer>(),
                    p.GetRequiredService<ILogger<ArangoReadService>>(),
                    ArangoDbClientName,
                    ReadTestPrefix));

            Services.AddScoped<IFirstLevelProjectionRepository>(
                sp =>
                    new ArangoFirstLevelProjectionRepository(
                        ArangoDbClientName,
                        GetFirstLevelProjectionPrefix(),
                        sp.GetRequiredService<
                            ILogger<ArangoFirstLevelProjectionRepository>>(),
                        sp));

            Services.AddScoped<IEventCollectorStore>(
                p => new ArangoEventCollectorStore(
                    p.GetRequiredService<ILogger<ArangoEventCollectorStore>>(),
                    p,
                    new MockArangoDbInitializer(),
                    ArangoDbClientName,
                    EventCollectorTestPrefix));

            Services.AddScoped<IFirstProjectionEventLogWriter>(
                sp =>
                    new Adapter.Arango.V2.Implementations.ArangoEventLogStore(
                        sp.GetRequiredService<
                            ILogger<Adapter.Arango.V2.Implementations.ArangoEventLogStore>>(),
                        sp,
                        ArangoDbClientEventLogStoreName,
                        GetFirstLevelProjectionPrefix()));

            Services.AddScoped<ISecondLevelProjectionRepository>(
                sp =>
                    new ArangoSecondLevelProjectionRepository(
                        sp.GetRequiredService<ILogger<ArangoSecondLevelProjectionRepository>>(),
                        sp,
                        sp.GetRequiredService<IMapper>(),
                        SecondLevelProjectionPrefix,
                        ArangoDbClientName));

            Services.AddScoped<IEntityStore>(
                p => new ArangoSyncEntityStore(
                    p.GetRequiredService<ILogger<ArangoSyncEntityStore>>(),
                    p,
                    new JsonSerializerSettings
                    {
                        Converters = WellKnownJsonConverters
                                     .GetDefaultProfileConverters()
                                     .Append(new StringEnumConverter())
                                     .Append(new EventLogTupleReadOnlyMemoryJsonConverter())
                                     .Append(new EventLogIgnoreEventJsonConverter())
                                     .ToList(),
                        NullValueHandling = NullValueHandling.Ignore
                    },
                    p.GetRequiredService<IDbInitializer>(),ArangoDbClientName, ArangoSyncEntityStoreTestPrefix));

            Services.AddScoped<ITicketStore>(
                p => new ArangoTicketStore(
                    p.GetRequiredService<ILogger<ArangoTicketStore>>(),
                    p,
                    new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter>
                        {
                            JsonSubtypesConverterBuilder.Of<TicketBase>(nameof(TicketBase.Type))
                                .RegisterSubtype<TicketA>("TicketA")
                                .RegisterSubtype<TicketB>("TicketB")
                                .Build()
                        }
                    },
                    new MockArangoDbInitializer(),
                    ArangoDbClientName,
                    TicketsTestPrefix));

            Services.AddTransient<IPathTreeRepository>(
                p => new PathTreeRepository(
                    p.GetRequiredService<IArangoDbClient>(),
                    SecondLevelProjectionPrefix,
                    new JsonSerializer
                    {
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.IsoDateFormat,
                        Converters =
                        {
                            new StringEnumConverter()
                        }
                    }));
        }

        private const string ProfileStorageConfigKey = "ProfileStorage";

        protected virtual string GetFirstLevelProjectionPrefix()
        {
            return FirstLevelProjectionPrefix;
        }

        protected string GetArangoConnectionString()
        {
            return Configuration?.GetSection(ProfileStorageConfigKey).Get<ArangoConfiguration>()?.ConnectionString;
        }

        protected IHttpClientFactory GetHttpClientFactory()
        {
            return GetServiceProvider().GetRequiredService<IHttpClientFactory>();
        }

        protected virtual IArangoDbClient GetArangoClient(string name = ArangoDbClientName)
        {
            return GetServiceProvider().GetRequiredService<IArangoDbClientFactory>().Create(name);
        }

        protected IConfigurationRoot LoadConfig()
        {
            return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile($"{WellKnownFiles.TestSettingsFile}")
                .Build();
        }

        protected IMapper GetMapper()
        {
            return GetServiceProvider().GetRequiredService<IMapper>();
        }

        public IServiceProvider GetServiceProvider()
        {
            return Services.BuildServiceProvider();
        }
    }
}
