using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

internal class OutputConsoleTarget : ITarget
{
    private readonly IOutput _Output;
    private readonly MemoryStream _Stream;

    public OutputConsoleTarget(IOutput output)
    {
        _Output = output;
        _Stream = new MemoryStream();
    }

    /// <inheritdoc cref="ITarget" />
    public async ValueTask DisposeAsync()
    {
        if (_Stream == null)
        {
            return;
        }

        _Stream.Close();
        await _Stream.DisposeAsync();
    }

    /// <inheritdoc cref="ITarget" />
    public async Task CompleteAsync(CancellationToken ct = default)
    {
        string line;
        using var reader = new StreamReader(_Stream);
        _Stream.Position = 0;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            _Output.WriteLine(line);
        }
    }

    /// <inheritdoc cref="ITarget" />
    public Stream GetStream()
    {
        return _Stream;
    }
}
