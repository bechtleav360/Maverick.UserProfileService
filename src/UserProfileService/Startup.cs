using Asp.Versioning;
using Hellang.Middleware.ProblemDetails;
using Maverick.UserProfileService.FilterUtility.Abstraction;
using Maverick.UserProfileService.FilterUtility.Implementations;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Prometheus;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Abstractions;
using UserProfileService.Adapter.Arango.V2.Abstractions;
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
using UserProfileService.Hosting;
using UserProfileService.Hosting.Abstraction;
using UserProfileService.Hosting.Tracing;
using UserProfileService.Messaging.DependencyInjection;
using UserProfileService.OpenApiSpec.Examples;
using UserProfileService.Saga.Validation.DependencyInjection;
using UserProfileService.Services;
using UserProfileService.Utilities;

namespace UserProfileService
{
    /// <summary>
    ///     Startup class to set up required services during starting the application.
    /// </summary>
    public class Startup : DefaultStartupBase
    {

        /// <summary>
        ///     Initializes a new instance of the <see cref="Startup" /> class with specified <paramref name="configuration" />.
        /// </summary>
        /// <param name="configuration"> The configuration object to be used to configure services. </param>
        public Startup(IConfiguration configuration) : base(configuration)
        {

        }

        /// <inheritdoc />
        protected override void AddLateConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
            
            Logger.LogInformation("Path bases configured");

            app.UseForwardedHeaders();

            app.UseMiddleware<RequestLoggingMiddleware>();
            app.UseMiddleware<ResponseHeaderMiddleware>();

            Logger.LogInformation("Custom middleware types configured");

            app.UseProblemDetails();

           
                app.UseSwagger();

                app.UseSwaggerUI(
                    c =>
                    {
                        c.SwaggerEndpoint("v2/swagger.json", "UserProfileService v2");
                        c.DocExpansion(DocExpansion.None);
                    });

                Logger.LogInformation("Support for SwaggerUI activated");
            

            app.UseCors(
                options => options.SetIsOriginAllowed(x => _ = true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());

            Logger.LogInformation("Metrics activated");

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

            Logger.LogInformation("Initialization completed");
        }

        /// <inheritdoc />
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();

            IConfigurationSection identitySettings =
                Configuration.GetSection(WellKnownConfigurationKeys.IdentitySettings);

            services.AddSyncRequester(
                options => { Configuration.Bind(SyncConstants.SyncConfigSection, options); });

            Logger.LogInformation("UPS sync requester registered");

            services.Configure<IdentitySettings>(identitySettings);

            services.AddMaverickIdentity(identitySettings, Logger);

            services.AddSupportForAnonymousRequests(identitySettings, Logger);

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

            Logger.LogInformation("Controller routes added");

            services.AddSwaggerGenNewtonsoftSupport();

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(
                c =>
                {
                    c.SwaggerDoc(
                        "v2",
                        new OpenApiInfo
                        {
                            Version = "v2",
                            Title = "UserProfileService v2 API",
                            Description =
                                "Maverick user profile service that will manage user and security related information.",
                            Contact = new OpenApiContact
                            {
                                Name = @"A/V 360° Solutions",
                                Email = string.Empty
                            }
                        });

                    c.OperationFilter<QueryFilterOperationFilter>();
                    c.OperationFilter<CustomHeaderOperationFilter>();
                    c.OperationFilter<AddDefaultValues>();
                    c.OperationFilter<RequestBodyExampleGeneratorFilter>();
                    // Set the comments path for the Swagger JSON and UI.
                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                    c.IncludeXmlComments(xmlPath, true);

                    var jwtSecurityScheme = new OpenApiSecurityScheme
                    {
                        Name = "JWT access token authentication",
                        Description = "Enter bearer token **_only_**",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        BearerFormat = "JWT",
                        Reference = new OpenApiReference
                        {
                            Id = JwtBearerDefaults.AuthenticationScheme,
                            Type = ReferenceType.SecurityScheme
                        }
                    };

                    c.AddSecurityDefinition(
                        jwtSecurityScheme.Reference.Id,
                        jwtSecurityScheme);

                    c.AddSecurityRequirement(
                        new OpenApiSecurityRequirement
                        {
                        { jwtSecurityScheme, new List<string>() }
                        });

                    c.MapType<JsonArray>(
                        () => new OpenApiSchema
                        {
                            Type = "array",
                            Default = new OpenApiArray(),
                            Description = "Array as JSON text (wrapped by '[', ']')",
                            Items = new OpenApiSchema
                            {
                                Type = "object",
                                Default = new OpenApiObject
                                {
                                { "prop1", new OpenApiString("value1") },
                                { "prop2", new OpenApiInteger(4711) }
                                }
                            }
                        });
                });

            services.AddPayloadValidation();

            Logger.LogInformation("Payload validation registered");

            services.AddSingleton(
                new ServiceDescriptor(
                    typeof(IActivitySourceWrapper),
                    _ => new ActivitySourceWrapper(StartupHelpers.Source),
                    ServiceLifetime.Singleton));

            services.AddSingleton<IJsonSerializerSettingsProvider, DefaultJsonSettingsProvider>();

            Logger.LogInformation("Json settings provider registered");

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
                Logger);

            Logger.LogInformation("Arango profile storage registered");

            services.AddMartenVolatileUserSettingsStore(
                    Configuration.GetSection(WellKnownConfigurationKeys.MartenSettings))
                .AddMartenUserStore(Logger);

            Logger.LogInformation("Volatile user settings store registered");

            services.TryAddScoped<IOperationHandler, ApiOperationHandler>();
            services.TryAddScoped<IVolatileDataOperationHandler, ApiOperationHandler>();

            Logger.LogInformation("Operation handlers registered");

            services.TryAddScoped<IFilterUtility<Filter>, FilterUtility>();
            services.TryAddScoped<IFilterUtility<List<ViewFilterModel>>, ViewFilterUtility>();

            services.Configure<ProfileDeputyConfiguration>(
                Configuration.GetSection(WellKnownConfigurationKeys.ProfileDeputyConfiguration));

            services.AddScoped<IDeputyService, DeputyService>();

            Logger.LogInformation("Deputy service registered");

            services.AddDefaultCursorApiProvider();

            services.AddArangoTicketStore(
                Configuration.GetSection(WellKnownConfigurationKeys.ProfileStorage),
                WellKnownDatabasePrefixes.ApiService,
                Logger);

            Logger.LogInformation("Arango ticket store registered");

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddScoped<IUserContextStore, UserContextStore>();

            Logger.LogInformation("User context store registered");

            services.AddScoped<IAuthorizationHandler, DenyAnonymousAuthorizationRequirementHandler>();
            services.AddScoped<IAuthorizationHandler, RolesAuthorizationRequirementHandler>();
            services.AddAutoMapper(typeof(Program));
            services.AddHostedService<InitializationService>();
            Logger.LogInformation("Initialization service registered (hosted service)");

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

            Logger.LogInformation("Dependencies of health store registered");

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

            Logger.LogInformation("Message handlers registered");

            services.AddProblemDetails(StartupHelpers.ConfigureProblemDetails);
            services.AddForwardedHeaders();

            // Add Tracing via OpenTelemetry
            var tracingOptions = Configuration.GetSection("Tracing").Get<TracingOptions>();

            services.AddUserProfileServiceTracing(
                options =>
                {
                    options.ServiceName = tracingOptions.ServiceName;
                    options.OtlpEndpoint = tracingOptions.OtlpEndpoint;
                });
        }

        /// <inheritdoc />
        protected override ActivitySource CreateSource()
        {
            return StartupHelpers.Source;
        }
    }
}
