﻿using System.Diagnostics;
using System.Reflection;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using UserProfileService.Common.Logging.Extensions;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace UserProfileService.Hosting.Abstraction
{
    /// <summary>
    ///     A default start up that can be used to register services and
    ///     make a pre or post configuration.
    /// </summary>
    public abstract class DefaultStartupBase
    {
        protected readonly ILogger _logger;

        /// <summary>
        ///     Creates an object of type <see cref="DefaultStartupBase" />.
        /// </summary>
        /// <param name="configuration">Contains the configuration to initialize the application.</param>
        protected DefaultStartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
            AdditionalAssemblies = Array.Empty<Assembly>();
            _logger = CreateLogger(configuration);
        }

        private ActivitySource? _source;

        /// <summary>
        ///     Gets the <see cref="ActivitySource" /> used for logging if the request
        ///     is going over several services.
        /// </summary>
        protected ActivitySource Source => _source ??= CreateSource();

        /// <summary>
        ///     Creates a new instance of <see cref="ActivitySource"/> used for logging.
        ///     Called internally by <see cref="Source"/> once.
        /// </summary>
        /// <returns>An instance of <see cref="ActivitySource"/>.</returns>
        protected abstract ActivitySource CreateSource();

        /// <summary>
        ///     Assemblies that can be loaded additionally.
        /// </summary>
        public Assembly[] AdditionalAssemblies { get; private set; }

        
        protected IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            AddEarlyConfiguration(app, env);
            
            app.UseReverseProxyPathBases(Configuration);

            app.UseForwardedHeaders();

            app.UseRouting();

            app.UseHttpMetrics();

            AddLateConfiguration(app, env);
        }

        /// <summary>
        ///     This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" />"> used to register services for the application.</param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            AdditionalAssemblies = LoadAssemblies();
            AddLateConfigureServices(services);
        }

        protected virtual void AddLateConfigureServices(IServiceCollection services)
        {
        }

        /// <summary>
        ///     Add App-Configuration before any other Configuration is done.
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        protected virtual void AddEarlyConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        ///     Add App-Configuration after the all other Configuration is done
        /// </summary>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        protected virtual void AddLateConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
        
        /// <summary>
        ///     Configure API-Versioning
        /// </summary>
        /// <param name="options">The option that can be configured.</param>
        protected virtual void ConfigureApiVersioning(ApiVersioningOptions options)
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(0, 0);
            options.ReportApiVersions = true;
        }

        /// <summary>
        ///     Configure if and which Xml-Docs get loaded from files.
        /// </summary>
        /// <param name="options">The swagger options that can be configured.</param>
        protected virtual void ConfigureAssemblyDocumentation(SwaggerGenOptions options)
        {
            foreach (var ass in AdditionalAssemblies.Concat(new[] {Assembly.GetEntryAssembly(), Assembly.GetExecutingAssembly()}))
            {
                var docFile = Path.Combine(AppContext.BaseDirectory, $"{ass.GetName().Name}.xml");

                if (File.Exists(docFile))
                {
                    options.IncludeXmlComments(docFile);
                }
            }
        }

        /// <summary>
        ///     Configure additional endpoints for this application.
        /// </summary>
        /// <param name="endpoints">
        ///     Defines a contract for a route builder in an application. A route builder specifies the routes for
        ///     an application.
        /// </param>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        public virtual void ConfigureEndpoints(
            IEndpointRouteBuilder endpoints,
            IApplicationBuilder app,
            IWebHostEnvironment env)
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
        }

        /// <summary>
        ///     Configure additional parts of the MVC-Pipeline
        /// </summary>
        /// <param name="builder">An interface for configuring MVC services.</param>
        protected virtual void ConfigureMvc(IMvcBuilder builder)
        {
        }

        /// <summary>
        ///     Configure the versioned Swagger-Documents for this Application
        /// </summary>
        /// <param name="options">The swagger option that can be configured.</param>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        /// <param name="provider">
        ///     Defines the behavior of a provider that discovers and describes API version information within
        ///     an application.
        /// </param>
        protected virtual void ConfigureSwaggerEndpoints(
            SwaggerUIOptions options,
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IApiVersionDescriptionProvider provider)
        {
            foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(v => v.ApiVersion))
            {
                options.SwaggerEndpoint(
                    $"/swagger/{description.GroupName}/swagger.json",
                    $"Saga-API {description.GroupName.ToUpperInvariant()}");
            }
        }

        /// <summary>
        ///     Configure how SwaggerGen operates
        /// </summary>
        /// <param name="options">The swagger option that can be configured.</param>
        protected virtual void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            options.CustomSchemaIds(t => t.FullName);
        }

        /// <summary>
        ///     Configure the Versioned API-Explorer
        /// </summary>
        /// <param name="options">Provides additional implementation specific to ASP.NET Core.</param>
        protected virtual void ConfigureVersionedApiExplorer(ApiExplorerOptions options)
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(0, 0);
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        }
        
        /// <summary>
        ///     Entry-Point to define additional Assemblies to be searched for Controllers, View, etc.
        ///     When Derived this does not need be overloaded, except when additional Assemblies are necessary.
        ///     When overridden, extend base-result instead of overriding - otherwise not all resources will be registered
        /// </summary>
        /// <returns></returns>
        protected virtual Assembly[] LoadAssemblies() => new[] {typeof(DefaultStartupBase).Assembly};

        /// <summary>
        ///     Configure the json provider used throughout the application.
        ///     Register the desired json settings using.
        ///     <remarks>DON'T call to base.RegisterJsonSettingsProvider when overriding</remarks>
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection" /> that is used to register services for the appliaction.</param>
        protected virtual void RegisterJsonSettingsProvider(IServiceCollection services)
        {
        }

        /// <summary>
        ///     Add additional Swagger configuration
        /// </summary>
        /// <param name="options">The swagger options that are used to configure swagger.</param>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        protected virtual void SwaggerSetup(SwaggerOptions options, IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        ///     Add additional Swagger-UI configuration.
        /// </summary>
        /// <param name="options">The swagger ui options that can be configured.</param>
        /// <param name="app">Defines a class that provides the mechanisms to configure an application's request pipeline.</param>
        /// <param name="env">Provides information about the web hosting environment an application is running in.</param>
        /// <param name="provider">Defines the behavior of a provider that discovers and describes API version information within an application.</param>
        protected virtual void SwaggerUiSetup(SwaggerUIOptions options,
                                              IApplicationBuilder app,
                                              IWebHostEnvironment env,
                                              IApiVersionDescriptionProvider provider)
        {
            options.SwaggerEndpoint("v1/B", "fasdf");
            
        }

        /// <summary>
        ///     Retrieves the version of the executing assembly.
        /// </summary>
        /// <returns>The version as a string or <see langword="null"/> if there is none.</returns>
        protected static string? GetAssemblyVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        }

        private static ILogger CreateLogger(IConfiguration configuration)
        {
            ILoggerFactory? loggerFactory = LoggerFactory.Create(
                loggingBuilder =>
                {
                    loggingBuilder.UseSpecificLogging(configuration);
                });

            return loggerFactory.CreateLogger("Registration");
        }
    }
}