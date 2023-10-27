using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Report;

/// <summary>
///     A health check report entry
/// </summary>
public class MaverickHealthReportEntry
{
    /// <summary>
    ///     Gets additional key-value pairs describing the health of the component.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

    /// <summary>
    ///     Gets the health status of the component that was checked.
    /// </summary>
    public HealthStatus Status { get; set; } = HealthStatus.Unhealthy;
}
