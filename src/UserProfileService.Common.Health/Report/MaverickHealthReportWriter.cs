using System;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace UserProfileService.Common.Health.Report;

/// <summary>
///     A Output writer to transform the AspNetCore <see cref="HealthReport" />
///     to a <see cref="MaverickHealthReport" /> and write it to the output
/// </summary>
public static class MaverickHealthReportWriter
{
    private const string DefaultContentType = "application/json";

    private static readonly Lazy<JsonSerializerSettings> _options =
        new Lazy<JsonSerializerSettings>(CreateSerializerSettings);

    private static Version Version => Assembly.GetEntryAssembly()?.GetName().Version;

    private static JsonSerializerSettings CreateSerializerSettings()
    {
        var serializerSettings = new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new[] { new StringEnumConverter() }
        };

        return serializerSettings;
    }

    /// <summary>
    ///     Transform a <see cref="HealthReport" /> to a <see cref="MaverickHealthReport" />
    ///     and write the response as JSON
    /// </summary>
    /// <param name="httpContext">The <see cref="HttpContext" /> to operate on</param>
    /// <param name="report">The <see cref="HealthReport" /> to transform</param>
    public static async Task WriteHealthReport(HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = DefaultContentType;
        string version = Version?.ToString() ?? "0.0.0.0";
        var healthReport = MaverickHealthReport.CreateFrom(version, report);
        await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(healthReport, _options.Value));
    }

    /// <summary>
    ///     Transform a <see cref="HealthReport" /> to a <see cref="MaverickHealthReport" />
    ///     and write the response as JSON
    /// </summary>
    /// <param name="appVersion">The version of the running application</param>
    /// <param name="httpContext">The <see cref="HttpContext" /> to operate on</param>
    /// <param name="report">The <see cref="HealthReport" /> to transform</param>
    public static async Task WriteHealthReport(string appVersion, HttpContext httpContext, HealthReport report)
    {
        httpContext.Response.ContentType = DefaultContentType;
        var healthReport = MaverickHealthReport.CreateFrom(appVersion, report);
        await httpContext.Response.WriteAsync(JsonConvert.SerializeObject(healthReport, _options.Value));
    }
}
