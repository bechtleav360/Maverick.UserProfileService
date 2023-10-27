using System;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

/// <summary>
///     Includes Sub-commands related to configuration.
/// </summary>
[Command(
    Name = "config",
    Description = "Operations regarding the configuration of an UPS application.",
    UnrecognizedArgumentHandling = UnrecognizedArgumentHandling.StopParsingAndCollect)]
[Subcommand(typeof(ExportConfigSubCommand))]
[HelpOption]
public class ConfigCommand : CommandBase
{
    /// <summary>
    ///     Initializes a new instance of <see cref="ConfigCommand" />.
    /// </summary>
    /// <param name="output">used to take care of output messages (usually STDOUT).</param>
    public ConfigCommand(
        IOutput output) : base(output)
    {
    }

    /// <inheritdoc cref="CommandBase" />
    protected override Task<int> OnExecuteAsync(CommandLineApplication app)
    {
        Output.WriteErrorLine("You must specify a sub-command.");
        app.ShowHelp();

        return Task.FromResult(1);
    }

    /// <summary>
    ///     Command that will export configuration in various ways.
    /// </summary>
    [Command(
        Name = "export",
        Description = "Export remote config in various ways.")]
    private class ExportConfigSubCommand : CommandBase
    {
        private readonly IConfigResolver _ConfigResolver;
        private readonly IHostEnvironment _HostEnvironment;

        /// <summary>
        ///     Option to leave arrays in <see cref="IConfiguration" /> as they are (with numeric keys instead of property names).
        ///     If false, they will be converted to arrays.
        /// </summary>
        [Option(
            "--raw",
            "The arrays won't be converted in the resulting JSON document.",
            CommandOptionType.NoValue)]
        public bool ArrayRaw { get; }

        /// <summary>
        ///     Boolean value indicating whether the appsettings files will be overwritten or not.
        /// </summary>
        [Option(
            "--force",
            "Forces overwriting an existing appsettings.json file. Otherwise the operation will be skipped.",
            CommandOptionType.NoValue)]
        // ReSharper disable once MemberCanBePrivate.Local
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public bool ForceOverwrite { get; set; }

        /// <summary>
        ///     Option to include environment variables in result.
        /// </summary>
        [Option(
            "--env",
            "Include the environment variables.",
            CommandOptionType.NoValue)]
        public bool IncludeEnvironment { get; }

        /// <summary>
        ///     Option to avoid indented JSON output, if set to true.
        /// </summary>
        [Option(
            "--oneline",
            "JSON document won't be written indented, but in one line.",
            CommandOptionType.NoValue)]
        public bool NotIndented { get; }

        /// <summary>
        ///     Option to configure the name of the file that will contain the exported configuration. If not set,
        ///     <see cref="IOutput" /> will be used.
        /// </summary>
        [Option(
            "-f|--file",
            "The name of the file where to export the configuration. If not provided, STDOUT will be used. An existing file will be overwritten.",
            CommandOptionType.SingleValue)]
        // ReSharper disable once MemberCanBePrivate.Local
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string PathAndFileName { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ExportConfigSubCommand" />.
        /// </summary>
        /// <param name="output">Used to take care of output messages (usually STDOUT).</param>
        /// <param name="configResolver">Used to retrieve configuration data.</param>
        /// <param name="hostEnvironment">The environment information where this command is running.</param>
        public ExportConfigSubCommand(
            IOutput output,
            IConfigResolver configResolver,
            IHostEnvironment hostEnvironment)
            : base(output)
        {
            _ConfigResolver = configResolver ?? throw new ArgumentNullException(nameof(configResolver));
            _HostEnvironment = hostEnvironment;
        }

        /// <inheritdoc cref="CommandBase" />
        protected override async Task<int> OnExecuteAsync(CommandLineApplication app)
        {
            if (_HostEnvironment != null && _HostEnvironment.IsProduction())
            {
                Output.WriteErrorLine("This command is deactivated for production environments and cannot be used.");

                return 1;
            }

            IConfiguration config = await _ConfigResolver.GetConfigAsync(IncludeEnvironment);

            if (string.IsNullOrWhiteSpace(PathAndFileName))
            {
                await using var configWriter = new ConfigToJsonWriter(new OutputConsoleTarget(Output));

                await configWriter.WriteJsonAsync(config, !ArrayRaw, !NotIndented);

                return 0;
            }

            string fullPath;

            try
            {
                fullPath = Path.GetFullPath(PathAndFileName);
            }
            catch (ArgumentException)
            {
                Output.WriteErrorLine("Path is not valid. (parameter --filename)");

                return 1;
            }
            catch (SecurityException)
            {
                Output.WriteErrorLine("Not allowed to access provided path. (parameter --filename)");

                return 1;
            }
            catch (NotSupportedException)
            {
                Output.WriteErrorLine(
                    "Path is not valid: It contains colons that are not part of volume identifier. (parameter --filename)");

                return 1;
            }
            catch (PathTooLongException)
            {
                Output.WriteErrorLine(
                    "Path is not valid: It is too long for the current operating system. (parameter --filename)");

                return 1;
            }

            string fileName = Path.GetFileName(fullPath);

            if (string.IsNullOrWhiteSpace(fileName))
            {
                Output.WriteErrorLine("File name as part of provided path must not be empty! (parameter --filename)");

                return 1;
            }

            if (!ForceOverwrite
                && File.Exists(fullPath)
                && Regex.IsMatch(fileName, "appsettings(\\.[\\w\\d_]+)?\\.json"))
            {
                Output.WriteErrorLine($"Cannot overwrite an existing {fileName}, if option '--force' is not set.");

                return 1;
            }

            await using var configFileWriter = new ConfigToJsonWriter(new FileTarget(fullPath));

            await configFileWriter.WriteJsonAsync(config, !ArrayRaw, !NotIndented);

            Output.WriteLine("File written.");

            return 0;
        }
    }
}
