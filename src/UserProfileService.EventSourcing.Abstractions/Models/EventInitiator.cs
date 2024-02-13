namespace UserProfileService.EventSourcing.Abstractions.Models;

/// <summary>
///     Some user or service that initiates or publish the message.
/// </summary>
public class EventInitiator
{
    /// <summary>
    ///     Identifier of initiator for the event.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     Defines the type of the initiator that initiates or publish the message
    /// </summary>
    public InitiatorType Type { get; set; }
}
