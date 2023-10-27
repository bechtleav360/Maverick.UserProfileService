namespace UserProfileService.Messaging.Abstractions.Models;

/// <summary>
///     Some user or service that starts or process the saga.
/// </summary>
public class SagaInitiator
{
    /// <summary>
    ///     Identifier of initiator for the saga.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     Defines the type of the initiator that starts or process the saga
    /// </summary>
    public InitiatorType Type { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="SagaInitiator" /> as unknown initiator.
    /// </summary>
    public SagaInitiator()
    {
        Id = string.Empty;
        Type = InitiatorType.Unknown;
    }
}
