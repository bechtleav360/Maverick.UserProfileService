using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace UserProfileService.Common.V2.CommandLineTools;

/// <summary>
///     The command class that provides the base commands to implement
///     other command to use it via the command line.
/// </summary>
public abstract class CommandBase
{
    /// <summary>
    ///     Component that can output data to the configured console.
    /// </summary>
    protected IOutput Output { get; }

    /// <summary>
    ///     The constructor of the class <inheritdoc cref="CommandBase" />.
    /// </summary>
    /// <param name="output">Component that can output data to the configured console.</param>
    protected CommandBase(IOutput output)
    {
        Output = output;
    }

    protected abstract Task<int> OnExecuteAsync(CommandLineApplication app);
}
