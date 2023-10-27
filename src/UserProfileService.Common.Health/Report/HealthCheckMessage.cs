using System;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Common.Health.Report;

/// <summary>
///     This message is emitted on a schedule informing the service about the health-status of a worker.
/// </summary>
[Message(ServiceGroup = "user-profile", ServiceName = "api")]
public record HealthCheckMessage
{
    /// <summary>
    ///     Specifies the name of the instance which is reporting the health-status.
    ///     Can be used
    /// </summary>
    public string InstanceName { get; set; }

    /// <summary>
    ///     Contains the status of the reporting instance.
    /// </summary>
    public HealthStatus Status { get; set; }

    /// <summary>
    ///     Specifies the time until when the check is valid. Should be in UTC.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    ///     Specifies the Version of the instance.
    /// </summary>
    public string Version { get; set; }

    /// <summary>
    ///     Contains the name of the worker which reports its health-status. e.g. SagaWorker
    /// </summary>
    public string WorkerName { get; set; }
}
