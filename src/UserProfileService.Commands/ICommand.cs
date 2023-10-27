namespace UserProfileService.Commands;

/// <summary>
///     Defines a command related message.
/// </summary>
public interface ICommand
{
    /// <summary>
    ///     Id of the command.
    /// </summary>
    CommandIdentifier Id { get; set; }
}
