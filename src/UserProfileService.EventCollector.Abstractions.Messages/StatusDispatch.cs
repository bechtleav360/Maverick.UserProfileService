using System;

namespace UserProfileService.EventCollector.Abstractions.Messages;

/// <summary>
///     Defines when a status should be sent out for the already collected responses.
/// </summary>
public class StatusDispatch
{
    /// <summary>
    ///     Defines the number of collected responses after which a status should be sent out.
    ///     The modulo of the specified number of responses must be zero.
    /// </summary>
    public int Modulo { get; set; }

    /// <summary>
    ///     Create an instance of <see cref="StatusDispatch" />.
    /// </summary>
    /// <param name="modulo">Defines the number of collected responses after which a status should be sent out.</param>
    public StatusDispatch(int modulo)
    {
        if (modulo < 1)
        {
            throw new ArgumentException("Modulo must not be less than 1.", nameof(modulo));
        }

        Modulo = modulo;
    }
}
