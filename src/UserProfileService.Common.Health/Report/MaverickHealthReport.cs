using System.Collections.Generic;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UserProfileService.Common.Health.Report;

/// <summary>
///     The Maverick health report defined by ADR-012
/// </summary>
public class MaverickHealthReport
{
    /// <summary>
    ///     A <see cref="IReadOnlyDictionary{TKey,T}" /> containing the results from each health check.
    /// </summary>
    /// <remarks>
    ///     The keys in this dictionary map the name of each executed health check to a
    ///     <see cref="MaverickHealthReportEntry" /> for the result data returned from the corresponding health check.
    /// </remarks>
    public Dictionary<string, MaverickHealthReportEntry> Entries { get; }

    /// <summary>
    ///     Gets a <see cref="HealthStatus" /> representing the aggregate status of all the health checks.
    ///     The value of <see cref="Status" /> will be the most servere status reported by a health check.
    ///     If no checks were executed, the value is always <see cref="HealthStatus.Healthy" />.
    /// </summary>
    public HealthStatus Status { get; private set; }

    /// <summary>
    ///     The version of the application
    /// </summary>
    public string Version { get; }

    /// <summary>
    ///     Create a new <see cref="MaverickHealthReport" /> from the specified results.
    /// </summary>
    /// <param name="version">The version of the application</param>
    /// <param name="entries">A <see cref="IReadOnlyDictionary{TKey, T}" /> containing the results from each health check.</param>
    public MaverickHealthReport(string version, Dictionary<string, MaverickHealthReportEntry> entries = null)
    {
        Version = version;
        Entries = entries ?? new Dictionary<string, MaverickHealthReportEntry>();
    }

    /// <summary>
    ///     Create a new <see cref="MaverickHealthReport" /> from the provided <see cref="HealthReport" />.
    /// </summary>
    /// <param name="version">The version of the application</param>
    /// <param name="report">The <see cref="HealthReport" /> to transform</param>
    /// <returns>A new <see cref="MaverickHealthReport" /> object</returns>
    public static MaverickHealthReport CreateFrom(string version, HealthReport report)
    {
        var healthReport = new MaverickHealthReport(version)
        {
            Status = report.Status
        };

        foreach (KeyValuePair<string, HealthReportEntry> kvp in report.Entries)
        {
            string key = kvp.Key;
            HealthReportEntry value = kvp.Value;

            var entry = new MaverickHealthReportEntry
            {
                Data = value.Data,
                Status = value.Status
            };

            healthReport.Entries.Add(key, entry);
        }

        return healthReport;
    }
}
