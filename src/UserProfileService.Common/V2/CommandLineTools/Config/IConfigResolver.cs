using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

/// <summary>
///     Contains methods to retrieve configuration data.
/// </summary>
public interface IConfigResolver
{
    /// <summary>
    ///     Gets the config of a source, optionally with environment variables or not.
    /// </summary>
    /// <param name="includeEnvironmentVariables">If true, the result will contains environment variables.</param>
    /// <returns>
    ///     A task representing the asynchronous read operation. It wraps the found configuration data as an
    ///     <see cref="IConfiguration" /> instance.
    /// </returns>
    Task<IConfiguration> GetConfigAsync(bool includeEnvironmentVariables);
}
