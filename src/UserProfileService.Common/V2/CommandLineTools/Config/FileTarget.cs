using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

internal class FileTarget : ITarget
{
    private readonly Stream _Stream;

    public FileTarget(string outputFilename)
    {
        _Stream = new FileStream(
            outputFilename,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read);
    }

    /// <inheritdoc cref="ITarget" />
    public async ValueTask DisposeAsync()
    {
        if (_Stream == null)
        {
            return;
        }

        await _Stream.FlushAsync();
        _Stream.Close();
        await _Stream.DisposeAsync();
    }

    /// <inheritdoc cref="ITarget" />
    public Task CompleteAsync(CancellationToken ct = default)
    {
        return _Stream.FlushAsync(ct);
    }

    /// <inheritdoc cref="ITarget" />
    public Stream GetStream()
    {
        return _Stream;
    }
}
