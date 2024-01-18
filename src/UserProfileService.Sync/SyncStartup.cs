using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using MassTransit;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Converters;
using Prometheus;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Common.Health;
using UserProfileService.Common.Health.Consumers;
using UserProfileService.Common.Health.Extensions;
using UserProfileService.Common.Health.Implementations;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Hosting.Abstraction;
using UserProfileService.Marten.EventStore.DependencyInjection;
using UserProfileService.Messaging.ArangoDb.Configuration;
using UserProfileService.Messaging.DependencyInjection;
using UserProfileService.Projection.Common.DependencyInjection;
using UserProfileService.Redis;
using UserProfileService.Redis.DependencyInjection;
using UserProfileService.Sync.Abstraction.Configurations;
using UserProfileService.Sync.Abstraction.Converters;
using UserProfileService.Sync.Abstraction.Factories;
using UserProfileService.Sync.Abstractions;
using UserProfileService.Sync.Configuration;
using UserProfileService.Sync.ConfigurationProvider;
using UserProfileService.Sync.Converter;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Factories;
using UserProfileService.Sync.Handlers;
using UserProfileService.Sync.Projection.DependencyInjection;
using UserProfileService.Sync.Projection.Extensions;
using UserProfileService.Sync.Services;
using UserProfileService.Sync.States;
using UserProfileService.Sync.Utilities;
using UserProfileService.Sync.Validation;

namespace UserProfileService.Sync;

/// <summary>
///     Startup to configure the UserProfileService.Sync. Based on <see cref="DefaultStartupBase"/>.
/// </summary>
public class SyncStartup : DefaultStartupBase
{
    /// <inheritdoc />
    public SyncStartup(IConfiguration configuration) : base(configuration)
    {
    }

    #region Overrides of DefaultStartupBase

    /// <inheritdoc />
    protected override void AddLateConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseRouting();
        app.UseHttpMetrics();

        app.UseEndpoints(
            endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics();

                //for convenience, display all health checks
                endpoints.MapHealthChecks(
                    "/health",
                    new HealthCheckOptions
                    {
                        ResponseWriter = MaverickHealthReportWriter.WriteHealthReport
                    });

                endpoints.MapHealthChecks(
                    "/health/ready",
                    new HealthCheckOptions
                    {
                        Predicate = check => check.Tags.Contains(HealthCheckTags.Readiness),
                        ResponseWriter = MaverickHealthReportWriter.WriteHealthReport
                    });

                endpoints.MapHealthChecks(
                    "/health/live",
                    new HealthCheckOptions
                    {
                        Predicate = check => check.Tags.Contains(HealthCheckTags.Liveness),
                        ResponseWriter = MaverickHealthReportWriter.WriteHealthReport
                    });
            });

        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "UserProfileService Sync V1"); });
    }

    /// <inheritdoc />
    public override void ConfigureServices(IServiceCollection services)
    {
        services.RemoveAll<IHttpMessageHandlerBuilderFilter>();

       services.TryAddTransient<IValidateOptions<SyncConfiguration>, SyncConfigurationValidation>();
       services.TryAddTransient<IValidateOptions<LdapSystemConfiguration>, LdapConfigurationValidation>();

        services.AddSyncConfigurationProvider(
            new[] { typeof(LdapConfigurationDependencyRegistration).Assembly },
            Configuration,
            _logger);

        services.Configure<SyncConfiguration>(Configuration.GetSection("SyncConfiguration"));

        IConfigurationSection arangoConfigurationSection =
            Configuration.GetSection(WellKnownConfigurationKeys.ProfileStorage);

        services
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
            .AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v1",
                        new OpenApiInfo
                        {
                            Version = "v1",
                            Title = "UserProfileService Sync API",
                            Description =
                                "Maverick user profile service sync that will manage to synchronize entities (User, Groups,..).",
                            Contact = new OpenApiContact
                            {
                                Name = "A/V 360° Solutions",
                                Email = string.Empty
                            }
                        });

                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                    try
                    {
                        c.IncludeXmlComments(xmlPath);
                    }
                    catch
                    {
                        // ignored
                    }
                })
            .AddArangoRepositoriesToReadFromProfileStorage(
                arangoConfigurationSection,
                WellKnownDatabasePrefixes.ApiService,
                _logger);

        services.AddMartenEventStore(
            Configuration,
            Constants.EventStorage.MartenSectionName,
            typeof(IUserProfileServiceEvent),
            (opt, provider) => { opt.AddSyncProjection(provider); },
            SyncJsonConverter.GetAllConvertersForMartenProjections());

        services.TryAddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();

        services.AddMaverickDestinationSystems();
        services.AddSagaEntityProcessorFactories();

        services.TryAddTransient<ISyncSourceSystemFactory, SyncSourceSystemFactory>();
        services.TryAddTransient<ISynchronizationService, SynchronizationService>();
        services.TryAddTransient<IScheduleService, ScheduleService>();

        services.TryAddTransient<ISyncModelComparerFactory, SyncModelComparerFactory>();
        services.TryAddSingleton(typeof(IConverterFactory<>), typeof(ExternalIdConverterFactory<>));

        services.AddProjectionResponseService();

        // Redis temp store
        services.AddRedis(Configuration.GetSection("Redis"), _logger);
        services.TryAddSingleton<ICacheStore, RedisCacheStore>();
        services.TryAddSingleton<ITempStore, RedisTempObjectStore>();
        services.TryAddSingleton<IProcessTempHandler, ProcessTempHandler>();

        services.AddSyncProjectionService(
            b =>
                b.AddArangoSyncRepository(arangoConfigurationSection)
                    .AddActivitySource(Source)
                    .AddSyncEventHandlers()
                    .AddSyncStreamNameResolver()
                    .AddHealthCheckStore());

        services
            .AddControllers()
            .AddNewtonsoftJson(
                options => { options.SerializerSettings.Converters.Add(new StringEnumConverter()); });

        services.AddSwaggerGenNewtonsoftSupport();

        services.TryAddSingleton<IDistributedHealthStatusStore>(
            s =>
                new DistributedHealthStatusStore(
                    s.GetRequiredService<ILogger<DistributedHealthStatusStore>>(),
                    GetIncludedWorkerHealthChecks()));

        services.AddMaverickHealthChecks(
            builder =>
                builder
                    .AddCheck<ArangoDbHealthCheck>(
                        "arangodb-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Scheduled })
                    .AddStoredHealthCheck(
                        "ArangoDB",
                        "arangodb-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Liveness })
                    .AddCheck<RedisHealthCheck>(
                        "redis-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Scheduled })
                    .AddStoredHealthCheck(
                        "Redis",
                        "redis-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Liveness }));

        services
            .AddMessaging(
                MessageSourceBuilder.GroupedApp(
                    "sync",
                    Constants.Messaging.ServiceGroup),
                Configuration,
                new[] { typeof(Program).Assembly },
                bus =>
                {
                    bus.AddSagaStateMachine<ProcessStateMachine, ProcessState>(
                            s => s.UseInMemoryOutbox())
                        .ArangoRepository(
                            r =>
                            {
                                var arangoConfiguration =
                                    arangoConfigurationSection.Get<ArangoConfiguration>();

                                r.DatabaseConfiguration("sync-client", arangoConfiguration);

                                // Default is Optimistic
                                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                                // Optional, default is "saga_state_{typeof(TSaga).Name}"
                                r.CollectionName = "Sync_Process_StateMachine";
                            });

                    bus.AddConsumer<HealthCheckMessageConsumer>()
                        .Endpoint(e => { e.Temporary = true; });
                });

        // Projection for sync and state machine (currently for initiator)
        services.AddSecondLevelSyncProjection();

        services.AddScheduledHealthChecks(
            builder =>
                builder.SetFilterPredicate(h => h.Tags.Contains(HealthCheckTags.Scheduled))
                    .SetDelay(Configuration[WellKnownConfigKeys.HealthCheckDelay]));

        services.AddHealthPublisher(
            builder =>
                builder.SetWorkerName(WellKnownWorkerNames.Sync)
                    .SetFilterPredicate(h => h.Tags.Contains(HealthCheckTags.Readiness))
                    .SetDelay(Configuration[WellKnownConfigKeys.HealthPushDelay]));

        services.TryAddSingleton<ISyncProcessCleaner, SyncProcessCleaner>();
        services.TryAddSingleton<ISyncProcessSynchronizer, DefaultSynchronizer>();
        services.AddModelComparer();

        // Configure base services
        base.ConfigureServices(services);
    }

    /// <summary>
    ///     Names of the workers whose health checks should be
    ///     included in the healthcheck report of the Sync.
    /// </summary>
    /// <returns>The names of the workers</returns>
    protected virtual string[] GetIncludedWorkerHealthChecks() => Array.Empty<string>();

    /// <inheritdoc />
    protected override ActivitySource CreateSource() => new ActivitySource(
        "Maverick.UserProfileService.Sync",
        GetAssemblyVersion());

    #endregion
}