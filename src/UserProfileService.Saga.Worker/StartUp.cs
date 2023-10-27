﻿using System;
using System.Diagnostics;
using System.Reflection;
using Asp.Versioning.ApiExplorer;
using MassTransit;
using Maverick.Client.ArangoDb.Public.Configuration;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Prometheus;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Adapter.Marten;
using UserProfileService.Adapter.Marten.DependencyInjection;
using UserProfileService.Common.Health;
using UserProfileService.Common.Health.Extensions;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Services;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.EventCollector;
using UserProfileService.EventCollector.DependencyInjection;
using UserProfileService.Events.Implementation.V3;
using UserProfileService.EventSourcing.Abstractions.DependencyInjection;
using UserProfileService.Hosting.Abstraction;
using UserProfileService.Informer.DependencyInjection;
using UserProfileService.Marten.EventStore.DependencyInjection;
using UserProfileService.Marten.EventStore.Implementations;
using UserProfileService.Messaging.ArangoDb.Configuration;
using UserProfileService.Messaging.DependencyInjection;
using UserProfileService.Projection.Common.DependencyInjection;
using UserProfileService.Projection.Common.Services;
using UserProfileService.Projection.FirstLevel.DependencyInjection;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;
using UserProfileService.Projection.SecondLevel.Assignments.Extensions;
using UserProfileService.Projection.SecondLevel.DependencyInjection;
using UserProfileService.Projection.SecondLevel.Extensions;
using UserProfileService.Projection.SecondLevel.VolatileDataStore;
using UserProfileService.Projection.SecondLevel.VolatileDataStore.DependencyInjection;
using UserProfileService.Projection.VolatileData.DependencyInjection;
using UserProfileService.Saga.Common.DependencyInjection;
using UserProfileService.Saga.Common.Implementations;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.Saga.Validation.DependencyInjection;
using UserProfileService.Saga.Worker.Abstractions;
using UserProfileService.Saga.Worker.Configuration;
using UserProfileService.Saga.Worker.Services;
using UserProfileService.Saga.Worker.Setup;
using UserProfileService.Saga.Worker.States;
using UserProfileService.Saga.Worker.States.Factories;
using UserProfileService.Saga.Worker.Utilities;

namespace UserProfileService.Saga.Worker;

public class StartUp : DefaultStartupBase
{
    public StartUp(IConfiguration configuration) : base(configuration)
    {
    }

    /// <inheritdoc />
    protected override ActivitySource CreateSource()
    {
        return new ActivitySource(
            "Maverick.UserProfileService.Saga.Worker",
            GetAssemblyVersion());
    }

    protected override void AddLateConfiguration(
        IApplicationBuilder app,
        IWebHostEnvironment env)
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
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("v1/swagger.json", "UserProfileService Saga Worker"); });
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    public override void ConfigureServices(IServiceCollection services)
    {
        IConfigurationSection arangoConfigurationSection =
            Configuration.GetSection(WellKnownConfigurationKeys.ProfileStorage);

        services
            .AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies())
            .AddArangoRepositoriesToReadFromProfileStorage(
                arangoConfigurationSection,
                WellKnownDatabasePrefixes.ApiService,
                Logger);

        services.AddTransient<IProjectionReadService, ProjectionReadService>();
        services.AddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();
        services.AddProjectionResponseService();

        // first level projection
        services.AddFirstLevelProjectionService(
            b =>
                b.AddArangoFirstLevelRepository(arangoConfigurationSection)
                    .AddActivitySource(Source)
                    .AddFirstLevelEventHandlers()
                    .AddFirstLevelStreamNameResolver()
                    .AddFirstLevelMapper()
                    .AddFirstLevelTupleCreator()
                    .AddHealthCheckStore()
                    .AddHandlerResolver()
                    .AddFirstLevelSagaService(sb => sb.AddArangoEventLogWriter(arangoConfigurationSection)));

        // second level projection
        services.AddSecondLevelProjectionService(
            b =>
                b.AddArangoSecondLevelRepository(arangoConfigurationSection)
                    .AddActivitySource(Source)
                    .AddSecondLevelEventHandlers()
                    .AddSecondLevelStreamNameResolver()
                    .AddSecondLevelMapper()
                    .AddHealthCheckStore());

        // assignments projection
        services.AddAssignmentProjectionService(
            b =>
                b.AddArangoAssignmentRepository(arangoConfigurationSection)
                    .AddActivitySource(Source)
                    .AddSecondLevelEventHandlers()
                    .AddSecondLevelStreamNameResolver()
                    .AddSecondLevelMapper()
                    .AddHealthCheckStore());

        // volatile projection
        services.AddVolatileDataProjectionService(
            b =>
                b.AddMartenVolatileDataProjectionRepository(arangoConfigurationSection)
                    .AddSecondLevelEventHandlers()
                    .AddSecondLevelStreamNameResolver()
                    .AddHealthCheckStore());

        services.AddMartenEventStore(
            Configuration,
            Constants.EventStorage.MartenSectionName,
            typeof(IUserProfileServiceEvent),
            (opt, provider) =>
            {
                opt.AddFirstLevelProjection(provider);
                opt.AddSecondLevelProjection(provider);
                opt.AddSecondLevelAssignmentProjection(provider);
                opt.AddSecondLevelVolatileDataProjection(provider);
            },
            SagaWorkerConverter.GetAllConvertersForMartenProjections());

        services.AddMartenVolatileUserSettingsStore(Configuration.GetSection(WellKnownConfigurationKeys.MartenSettings))
            .AddMartenUserStore(Logger);

        services.AddEventPublisherDependencies(
            setup => setup.AddVolatileDataEventPublisher(
                    config => config.SupportEvent<UserSettingsSectionCreatedEvent>()
                        .SupportEvent<UserSettingObjectUpdatedEvent>()
                        .SupportEvent<UserSettingSectionDeletedEvent>()
                        .SupportEvent<UserSettingObjectDeletedEvent>())
                .AddDefaultEventPublisher());

        services.AddArangoDatabaseCleanupProvider(
            (_, setup) =>
            {
                setup.FirstLevelProjectionCollection =
                    Configuration.GetValue<TimeSpan?>(
                        $"{CleanupConfiguration.DefaultSectionName}:FirstLevelProjection");

                setup.AssignmentProjectionCollection =
                    Configuration.GetValue<TimeSpan?>(
                        $"{CleanupConfiguration.DefaultSectionName}:AssignmentProjection");

                setup.EventCollectorCollections =
                    Configuration.GetValue<TimeSpan?>($"{CleanupConfiguration.DefaultSectionName}:EventCollector");

                setup.ServiceProjectionCollection =
                    Configuration.GetValue<TimeSpan?>($"{CleanupConfiguration.DefaultSectionName}:Service");
            });

        // Event collector agent
        IConfigurationSection eventCollectorSection = Configuration.GetSection("EventCollector");

        services
            .AddEventCollectorStore(
                arangoConfigurationSection,
                WellKnownDatabasePrefixes.EventCollector)
            .AddEventCollectorAgent(eventCollectorSection);

        IConfigurationSection seedingSection = Configuration.GetSection("Seeding");
        services.Configure<SeedingConfiguration>(seedingSection);

        services.Configure<CleanupConfiguration>(Configuration.GetSection(CleanupConfiguration.DefaultSectionName));

        IConfigurationSection validationSection = Configuration.GetSection("Validation");

        services.AddSagaValidation<ProjectionReadService, VolatileRepoValidationService>(validationSection);

        bool enableSeedingService =
            !seedingSection.GetValue<bool>(nameof(SeedingConfiguration.Disabled));

        if (enableSeedingService)
        {
            services.AddHostedService<SeedingService>();
        }

        services
            .AddControllers()
            .AddNewtonsoftJson();

        services.AddHostedService<DatabaseCleanupBackgroundService>();

        // Add Projection-Services
        services
            .Configure<EventBusConnectionConfiguration>(Configuration.GetSection(WellKnownConfigurationKeys.EventBus))
            .AddHostedService<CronJobServiceManager>();

        const string scheduledTag = "scheduled";
        services.AddHealthStatusStore();

        services.AddMaverickHealthChecks(
            builder =>
            {
                if (enableSeedingService)
                {
                    builder.AddStoredHealthCheck(
                        "Seeding",
                        "seeding",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Readiness });
                }

                builder.AddCheck<ArangoDbHealthCheck>(
                        "arangodb-internal",
                        HealthStatus.Unhealthy,
                        new[] { scheduledTag })
                    .AddStoredHealthCheck(
                        "ArangoDB",
                        "arangodb-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Liveness, HealthCheckTags.Readiness })
                    .AddCheck<MartenEventStoreHealthCheck>(
                        "marteneventstore-internal",
                        HealthStatus.Unhealthy,
                        new[] { scheduledTag })
                    .AddStoredHealthCheck(
                        "MartenEventStore",
                        "marteneventstore-internal",
                        HealthStatus.Unhealthy,
                        new[] { HealthCheckTags.Liveness, HealthCheckTags.Readiness })
                    .AddCheck<ProjectionServiceHealthCheck>(
                        "HostedProjectionService",
                        tags: new[] { HealthCheckTags.Readiness })
                    .AddCheck<ArangoGlobalHealthCheck>(
                        "ArangoDBOperations",
                        tags: new[] { HealthCheckTags.Readiness });
            });

        services.AddScheduledHealthChecks(
            builder =>
                builder.SetFilterPredicate(h => h.Tags.Contains(scheduledTag))
                    .SetDelay(Configuration[WellKnownConfigKeys.HealthCheckDelay]));

        services.AddHealthPublisher(
            builder =>
                builder.SetWorkerName(WellKnownWorkerNames.SagaWorker)
                    .SetFilterPredicate(h => h.Tags.Contains(HealthCheckTags.Readiness))
                    .SetDelay(Configuration[WellKnownConfigKeys.HealthPushDelay]));

        services.AddMessaging(
                MessageSourceBuilder.GroupedApp(
                    "saga-worker",
                    Constants.Messaging.ServiceGroup),
                Configuration,
                new[]
                {
                    typeof(Program).Assembly,
                    typeof(IValidationService).Assembly,
                    typeof(EventCollectorAgent).Assembly
                },
                bus =>
                {
                    bus.AddSagaStateMachine<CommandProcessStateMachine, CommandProcessState>()
                        .ArangoRepository(
                            r =>
                            {
                                var arangoConfiguration =
                                    arangoConfigurationSection.Get<ArangoConfiguration>();

                                r.DatabaseConfiguration("saga-state-client", arangoConfiguration);

                                // Default is Optimistic
                                r.ConcurrencyMode = ConcurrencyMode.Pessimistic;

                                // Optional, default is "saga_state_{typeof(TSaga).Name}"
                                r.CollectionName = "SagaWorker_Command_StateMachine";
                            });
                })
            .AddTransient<ICommandServiceFactory, CommandServiceFactory>();
        
        services.AddNoneMessageInformer();
    }
}
