using System.Diagnostics;
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
    public abstract class DefaultStartupBase
    {
        protected ILogger Logger;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="configuration"></param>
        protected DefaultStartupBase(IConfiguration configuration)
        {
            Configuration = configuration;
            AdditionalAssemblies = Array.Empty<Assembly>();
            Logger = CreateLogger(configuration);
        }

        private ActivitySource? _source;
        
        /// <summary>
        ///     Gets the <see cref="ActivitySource"/> used for logging.
        /// </summary>
        public ActivitySource Source => _source ??= CreateSource();

        /// <summary>
        ///     Creates a new instance of <see cref="ActivitySource"/> used for logging.
        ///     Called internally by <see cref="Source"/> once.
        /// </summary>
        /// <returns>An instance of <see cref="ActivitySource"/>.</returns>
        protected abstract ActivitySource CreateSource();
        
        public Assembly[] AdditionalAssemblies { get; private set; }

        public IConfiguration Configuration { get; }

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
        /// <param name="services"></param>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            AdditionalAssemblies = LoadAssemblies();
        }

        /// <summary>
        ///     Add App-Configuration before any other Configuration is done
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void AddEarlyConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        ///     Add App-Configuration after the all other Configuration is done
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void AddLateConfiguration(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
        
        /// <summary>
        ///     Configure API-Versioning
        /// </summary>
        /// <param name="options"></param>
        protected virtual void ConfigureApiVersioning(ApiVersioningOptions options)
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(0, 0);
            options.ReportApiVersions = true;
        }

        /// <summary>
        ///     Configure if and which Xml-Docs get loaded from files
        /// </summary>
        /// <param name="options"></param>
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
        ///     Configure additional Endpoints for this Application
        /// </summary>
        /// <param name="endpoints"></param>
        /// <param name="app"></param>
        /// <param name="env"></param>
        public virtual void ConfigureEndpoints(IEndpointRouteBuilder endpoints, IApplicationBuilder app, IWebHostEnvironment env)
        {
            endpoints.MapControllers();
            endpoints.MapMetrics();
        }

        /// <summary>
        ///     Configure additional parts of the MVC-Pipeline
        /// </summary>
        /// <param name="builder"></param>
        protected virtual void ConfigureMvc(IMvcBuilder builder)
        {
        }

        /// <summary>
        ///     Configure the versioned Swagger-Documents for this Application
        /// </summary>
        /// <param name="options"></param>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="provider"></param>
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
        /// <param name="options"></param>
        protected virtual void ConfigureSwaggerGen(SwaggerGenOptions options)
        {
            options.CustomSchemaIds(t => t.FullName);
        }

        /// <summary>
        ///     Configure the Versioned API-Explorer
        /// </summary>
        /// <param name="options"></param>
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
        ///     Configure the <see cref="ISagaJsonSerializerSettingsProvider" /> used throughout the application.
        ///     Register the desired <see cref="ISagaJsonSerializerSettingsProvider" /> using
        ///     <code>services.TryAddTransient{IJsonSerializerSettingsProvider, T}();</code>
        ///     <remarks>DON'T call to base.RegisterJsonSettingsProvider when overriding</remarks>
        /// </summary>
        /// <param name="services"></param>
        protected virtual void RegisterJsonSettingsProvider(IServiceCollection services)
        {}

        /// <summary>
        ///     Add additional Swagger configuration
        /// </summary>
        /// <param name="options"></param>
        /// <param name="app"></param>
        /// <param name="env"></param>
        protected virtual void SwaggerSetup(SwaggerOptions options, IApplicationBuilder app, IWebHostEnvironment env)
        {
        }

        /// <summary>
        ///     Add additional Swagger-UI configuration
        /// </summary>
        /// <param name="options"></param>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="provider"></param>
        protected virtual void SwaggerUiSetup(SwaggerUIOptions options,
                                              IApplicationBuilder app,
                                              IWebHostEnvironment env,
                                              IApiVersionDescriptionProvider provider)
        {
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