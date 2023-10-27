using System;

namespace UserProfileService.Common.V2.Contracts;

/// <summary>
///     Model for saving the saga status.
/// </summary>
public class SagaStatus
{
    /// <summary>
    ///     The correlation id which for given for the
    ///     saga.
    /// </summary>
    public string CorrelationId { set; get; }

    /// <summary>
    ///     Time when entry has been created.
    /// </summary>
    public DateTime CreatedAt { set; get; }

    /// <summary>
    ///     Unique guid for identify the saga.
    /// </summary>
    public string Id { set; get; }

    /// <summary>
    ///     The command name of the saga.
    ///     Depends of the services which name
    ///     are you.
    /// </summary>
    public string JobType { set; get; }

    /// <summary>
    ///     A time stamp when the saga status was logged.
    /// </summary>
    public DateTime ModifiedAt { set; get; } = DateTime.UtcNow;

    /// <summary>
    ///     The current status of the saga.
    ///     This state is global. An enum
    ///     exists for these.
    /// </summary>
    public SagaState Status { set; get; }

    /// <summary>
    ///     Own codes for the status.
    /// </summary>
    public int StatusCode { set; get; }
}
