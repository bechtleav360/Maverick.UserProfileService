﻿using Asp.Versioning;
using Hellang.Middleware.ProblemDetails;
using Maverick.UserProfileService.FilterUtility.Abstraction;
using Maverick.UserProfileService.FilterUtility.Implementations;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Prometheus;
using System.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Abstractions;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Adapter.Marten.DependencyInjection;
using UserProfileService.Api.Common.Abstractions;
using UserProfileService.Common.Health;
using UserProfileService.Common.Health.Consumers;
using UserProfileService.Common.Health.Extensions;
using UserProfileService.Common.Health.Implementations;
using UserProfileService.Common.Health.Report;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Common.V2.Implementations;
using UserProfileService.Common.V2.Services;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Configuration;
using UserProfileService.Extensions;
using UserProfileService.FilterHelper;
using UserProfileService.Hosting.Abstraction;
using UserProfileService.Hosting.Tracing;
using UserProfileService.Messaging.DependencyInjection;
using UserProfileService.Saga.Validation.DependencyInjection;
using UserProfileService.Services;
using UserProfileService.Swagger;
using UserProfileService.Utilities;

namespace UserProfileService
{
    /// <summary>
    ///     Startup class to set up required services during starting the application.
    /// </summary>
    public class UserProfileStartUp : DefaultStartupBase
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="UserProfileStartUp" /> class with specified <paramref name="configuration" />.
        /// </summary>
        /// <param name="configuration"> The configuration object to be used to configure services. </param>
        public UserProfileStartUp(IConfiguration configuration) : base(configuration)
        {

        }


        /// <inheritdoc />
        protected override void AddLateConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            _logger.LogInformation("Path bases configured");

            app.UseForwardedHeaders();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ResponseHeaderMiddleware>();

            _logger.LogInformation("Custom middleware types configured");

            app.UseProblemDetails();

            var useSwaggerUi = Configuration.GetValue("Features:UseSwaggerUI", false);
            
            if (useSwaggerUi)
            {
                _logger.LogInformation("Support for SwaggerUI activated");
                app.UseMaverickSwaggerWithVersions();
            }

            app.UseCors(
                options => options.SetIsOriginAllowed(x => _ = true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());

            _logger.LogInformation("Metrics activated");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(
                endpoints =>
                {
                    endpoints.MapControllers();
                    endpoints.MapMetrics();

                    // for convenience, display all health checks
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

                    endpoints.MapHealthChecks(
                        "/health/state",
                        new HealthCheckOptions
                        {
                            Predicate = check => check.Tags.Contains(HealthCheckTags.State),
                            ResponseWriter = MaverickHealthReportWriter.WriteHealthReport
                        });
                });

            _logger.LogInformation("Initialization completed");
        }

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            IConfigurationSection identitySettings =
                Configuration.GetSection(WellKnownConfigurationKeys.IdentitySettings);

            services.AddSyncRequester(
                options => { Configuration.Bind(SyncConstants.SyncConfigSection, options); });

            _logger.LogInformation("UPS sync requester registered");

            services.Configure<IdentitySettings>(identitySettings);

            services.AddMaverickIdentity(identitySettings, _logger);

            services.AddSupportForAnonymousRequests(identitySettings, _logger);

            services.AddAuthorization(
                o =>
                    o.AddPolicy(
                        "default",
                        b => b.RequireAuthenticatedUser()));

            services.AddRouting(options => options.LowercaseUrls = true);

            services.AddControllers(
                    o =>
                    {
                        o.AllowEmptyInputInBodyModelBinding = true;
                        o.ModelBinderProviders.Insert(0, new QueryFilterBinderProvider());
                        o.InputFormatters.Add(new TextPlainInputFormatter());
                    })
                .AddNewtonsoftJson(o => o.SerializerSettings.AddDefaultConverters());

            _logger.LogInformation("Controller routes added");

            services.AddSwaggerGenNewtonsoftSupport();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddMaverickSwaggerWithVersions<MaverickSwagger>();
            
            services.AddPayloadValidation();

            _logger.LogInformation("Payload validation registered");

            services.AddSingleton(
                new ServiceDescriptor(
                    typeof(IActivitySourceWrapper),
                    _ => new ActivitySourceWrapper(StartupHelpers.Source),
                    ServiceLifetime.Singleton));

            services.AddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();

            _logger.LogInformation("Json settings provider registered");

            services.AddApiVersioning(
                    options =>
                    {
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        options.ReportApiVersions = true;
                    })
                .AddApiExplorer(
                    options =>
                    {
                        options.AssumeDefaultVersionWhenUnspecified = true;
                        options.DefaultApiVersion = new ApiVersion(1, 0);
                        // ReSharper disable once StringLiteralTypo
                        options.GroupNameFormat = "'v'VVV";
                        options.SubstituteApiVersionInUrl = true;
                    });

            services.AddArangoRepositoriesForService(
                Configuration.GetSection(WellKnownConfigurationKeys.ProfileStorage),
                _logger);

            _logger.LogInformation("Arango profile storage registered");

            services.AddMartenVolatileUserSettingsStore(
                    Configuration.GetSection(WellKnownConfigurationKeys.MartenSettings))
                .AddMartenUserStore(_logger);

            _logger.LogInformation("Volatile user settings store registered");

            services.TryAddScoped<IOperationHandler, ApiOperationHandler>();
            services.TryAddScoped<IVolatileDataOperationHandler, ApiOperationHandler>();

            _logger.LogInformation("Operation handlers registered");

            services.TryAddScoped<IFilterUtility<Filter>, FilterUtility>();
            services.TryAddScoped<IFilterUtility<List<ViewFilterModel>>, ViewFilterUtility>();

            services.Configure<ProfileDeputyConfiguration>(
                Configuration.GetSection(WellKnownConfigurationKeys.ProfileDeputyConfiguration));

            services.AddScoped<IDeputyService, DeputyService>();

            _logger.LogInformation("Deputy service registered");

            services.AddDefaultCursorApiProvider();

            services.AddArangoTicketStore(
                Configuration.GetSection(WellKnownConfigurationKeys.ProfileStorage),
                WellKnownDatabasePrefixes.ApiService,
                _logger);

            _logger.LogInformation("Arango ticket store registered");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserContextStore, UserContextStore>();

            _logger.LogInformation("User context store registered");

            services.AddScoped<IAuthorizationHandler, DenyAnonymousAuthorizationRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, RolesAuthorizationRequirementHandler>();
            services.AddAutoMapper(typeof(Program));
            services.AddHostedService<InitializationService>();
            _logger.LogInformation("Initialization service registered (hosted service)");

            const string scheduledTag = "scheduled";

            services.AddMaverickHealthChecks(
                b =>
                    b.AddTypeActivatedCheck<DistributedHealthCheck>(
                            WellKnownWorkerNames.SagaWorker,
                            null,
                            new[] { HealthCheckTags.State },
                            WellKnownWorkerNames.SagaWorker)
                        .AddTypeActivatedCheck<DistributedHealthCheck>(
                            WellKnownWorkerNames.Sync,
                            null,
                            Array.Empty<string>(),
                            WellKnownWorkerNames.Sync)
                        .AddCheck<ArangoDbHealthCheck>(
                            "arangodb-internal",
                            HealthStatus.Unhealthy,
                            new[] { scheduledTag })
                        .AddStoredHealthCheck(
                            "ArangoDB",
                            "arangodb-internal",
                            HealthStatus.Unhealthy,
                            new[] { HealthCheckTags.Liveness, HealthCheckTags.State }));

            services.AddScheduledHealthChecks(
                setup =>
                    setup.SetFilterPredicate(h => h.Tags.Contains(scheduledTag))
                        .SetDelay(Configuration[WellKnownConfigKeys.HealthCheckDelay]));

            services.AddSingleton<IDistributedHealthStatusStore, DistributedHealthStatusStore>();

            _logger.LogInformation("Dependencies of health store registered");

            // Register messaging
            RegisterMessaging(services,Configuration);

            _logger.LogInformation("Message handlers registered");

            services.AddProblemDetails(StartupHelpers.ConfigureProblemDetails);
            services.AddForwardedHeaders();

            // Add Tracing via OpenTelemetry
            RegisterTracing(services, Configuration);
            
            services.TryAddSingleton<IOperationRedirectionMapper,OperationRedirectionMapper>();
        }

        public override void RegisterTracing(IServiceCollection services, IConfiguration configuration)
        {
            var tracingOptions = Configuration.GetSection("Tracing").Get<TracingOptions>();

            services.AddUserProfileServiceTracing(
                options =>
                {
                    options.ServiceName = tracingOptions.ServiceName;
                    options.OtlpEndpoint = tracingOptions.OtlpEndpoint;
                });
            
            services.AddForwardedHeaders();
        }
        
        public override void RegisterMessaging(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMessaging(
                MessageSourceBuilder.GroupedApp("api", Constants.Messaging.ServiceGroup),
                Configuration,
                new[] { typeof(Program).Assembly },
                cfg =>
                {
                    cfg.AddConsumer<HealthCheckMessageConsumer>()
                       .Endpoint(
                           e =>
                           {
                               // If the name is not changed here, e.Temporary cannot be set to true
                               // because MassTransit throws an exception stating that the state does
                               // not match some other definition.
                               e.Name = Constants.Messaging.HealthCheckApiConsumerEndpoint;
                               e.Temporary = true;
                           });
                });
        }

        /// <inheritdoc />
        protected override ActivitySource CreateSource()
        {
            return StartupHelpers.Source;
        }
    }
}