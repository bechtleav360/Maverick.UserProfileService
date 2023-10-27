using System;
using UserProfileService.EventCollector.Abstractions.Messages;
using UserProfileService.Messaging.Annotations;

namespace UserProfileService.Validation.Abstractions.Message;

/// <summary>
///     Defines a message that triggered a validation process.
/// </summary>
[Message(Name = "validation-triggered", Version = "v1")]
public class ValidationTriggered : IEventCollectorMessage
{
    /// <inheritdoc cref="IEventCollectorMessage" />
    public Guid CollectingId { get; set; }

    /// <summary>
    ///     Related command e.g UserDelete
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => get modifier can be used.
    public string Command { get; set; }

    /// <summary>
    ///     Payload to validate.
    /// </summary>
    // ReSharper disable once UnusedAutoPropertyAccessor.Global => get modifier can be used.
    public string Payload { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="ValidationTriggered" />.
    /// </summary>
    /// <param name="payload">Payload to validate.</param>
    /// <param name="command">Payload related command to validate.</param>
    public ValidationTriggered(string payload, string command)
    {
        CollectingId = Guid.NewGuid();
        Payload = payload;
        Command = command;
    }
}
