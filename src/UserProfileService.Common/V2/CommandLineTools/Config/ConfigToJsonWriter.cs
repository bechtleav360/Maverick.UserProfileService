using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace UserProfileService.Common.V2.CommandLineTools.Config;

/// <summary>
///     Contains a method to write JSON data to <see cref="ITarget" />s.
/// </summary>
public class ConfigToJsonWriter : IAsyncDisposable
{
    private readonly ITarget _Target;

    /// <summary>
    ///     Initializes a new instance of <see cref="ConfigToJsonWriter" /> using a specified <paramref name="target" />.
    /// </summary>
    /// <param name="target">The instance to be used for writing JSON data.</param>
    public ConfigToJsonWriter(ITarget target)
    {
        _Target = target;
    }

    private static async Task WriteJsonAsync(
        IConfiguration config,
        Utf8JsonWriter writer,
        bool convertArrays,
        CancellationToken ct = default)
    {
        List<IConfigurationSection> children = config.GetChildren().ToList();

        ct.ThrowIfCancellationRequested();

        if (children.Count == 0 && config is IConfigurationSection section)
        {
            WriteValue(writer, section.Value);
            await writer.FlushAsync(ct);

            return;
        }

        if (convertArrays && IsArray(children.FirstOrDefault()))
        {
            writer.WriteStartArray();

            foreach (IConfigurationSection child in children)
            {
                ct.ThrowIfCancellationRequested();
                await WriteJsonAsync(child, writer, true, ct);
            }

            writer.WriteEndArray();
        }
        else
        {
            writer.WriteStartObject();

            foreach (IConfigurationSection child in children)
            {
                ct.ThrowIfCancellationRequested();
                writer.WritePropertyName(child.Key);
                await WriteJsonAsync(child, writer, convertArrays, ct);
            }

            writer.WriteEndObject();
        }

        await writer.FlushAsync(ct);
    }

    private static void WriteValue(
        Utf8JsonWriter writer,
        string value)
    {
        if (long.TryParse(value, out long l))
        {
            writer.WriteNumberValue(l);

            return;
        }

        if (double.TryParse(
                value,
                NumberStyles.AllowDecimalPoint | NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double d))
        {
            writer.WriteNumberValue(d);

            return;
        }

        if (bool.TryParse(value, out bool b))
        {
            writer.WriteBooleanValue(b);

            return;
        }

        if (string.IsNullOrEmpty(value))
        {
            writer.WriteNullValue();

            return;
        }

        writer.WriteStringValue(value);
    }

    private static bool IsArray(IConfigurationSection section)
    {
        if (section == null || !section.Exists())
        {
            return false;
        }

        return int.TryParse(section.Key, out int num) && num >= 0;
    }

    /// <summary>
    ///     Write an <see cref="IConfiguration" /> instance to its JSON document using a internal target.
    /// </summary>
    /// <param name="config">The configuration to be written.</param>
    /// <param name="convertArrays">A boolean value indicating whether the arrays should be converted or not.</param>
    /// <param name="indented">A boolean value indicating whether intention will be used in the JSON document or not.</param>
    /// <param name="ct">The cancellation token to monitor cancellation requests.</param>
    /// <returns>A task representing the asynchronous write operation.</returns>
    public async Task WriteJsonAsync(
        IConfiguration config,
        bool convertArrays = true,
        bool indented = true,
        CancellationToken ct = default)
    {
        await using var jWriter = new Utf8JsonWriter(
            _Target.GetStream(),
            new JsonWriterOptions
            {
                Indented = indented
            });

        if (config == null)
        {
            jWriter.WriteStartObject();
            jWriter.WriteEndObject();
        }
        else
        {
            await WriteJsonAsync(config, jWriter, convertArrays, ct);
        }

        await _Target.CompleteAsync(ct);
    }

    /// <inheritdoc cref="IAsyncDisposable" />
    public async ValueTask DisposeAsync()
    {
        if (_Target == null)
        {
            return;
        }

        await _Target.DisposeAsync();
    }
}
