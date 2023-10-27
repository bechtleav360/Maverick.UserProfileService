using System;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Result of command handling during sync process.
/// </summary>
public class CommandResult
{
    /// <summary>
    ///     Error that occurred while executing the command. Command was not executed on the target system.
    /// </summary>
    public Exception Exception { get; set; }

    /// <summary>
    ///     Identifier of the command sent to the target system.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///     Indicates whether the command was successful.
    /// </summary>
    public bool Success => Exception != null;

    /// <summary>
    ///     Create an instance of <see cref="CommandResult" />.
    /// </summary>
    /// <param name="id">Identifier of the command sent to the target system.</param>
    /// <param name="exception">Error that occurred while executing the command. Command was not executed on the target system. </param>
    public CommandResult(Guid id, Exception exception = null)
    {
        Id = id;
        Exception = exception;
    }
}
