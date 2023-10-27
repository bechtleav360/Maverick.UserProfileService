namespace UserProfileService.Commands;

/// <summary>
///     Defines a initiator for <see cref="SubmitCommand" />.
/// </summary>
public class CommandInitiator
{
    /// <summary>
    ///     Unique id of initiator.
    /// </summary>
    public string Id { get; }

    /// <summary>
    ///     Type of initiator.
    /// </summary>
    public CommandInitiatorType Type { get; }

    /// <summary>
    ///     Create an instance of <see cref="CommandInitiator" />.
    /// </summary>
    /// <param name="id">Unique id of initiator.</param>
    /// <param name="type">Type of initiator.</param>
    public CommandInitiator(string id, CommandInitiatorType type = CommandInitiatorType.Unknown)
    {
        Id = id;
        Type = type;
    }
}
