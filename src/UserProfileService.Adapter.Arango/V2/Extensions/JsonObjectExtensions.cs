using System.Globalization;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Contains extension methods related to conversion from or to <see cref="JToken" /> (either <see cref="JObject" /> or
///     <see cref="JArray" />).
/// </summary>
internal static class JsonObjectExtensions
{
    /// <summary>
    ///     Gets the <see cref="JToken" /> of the <paramref name="instance" />.
    /// </summary>
    /// <param name="instance">The instance to be serialized.</param>
    /// <param name="serializer">The serializer to be used.</param>
    /// <returns>The serialized <see cref="JToken" /> of <paramref name="instance" />.</returns>
    internal static JToken GetJsonDocument(
        this object instance,
        JsonSerializer serializer)
    {
        return JToken.FromObject(
            instance,
            serializer);
    }

    /// <summary>
    ///     Gets the <see cref="JToken" /> of the <paramref name="instance" />.<br />
    /// </summary>
    /// <remarks>
    ///     The default <see cref="JsonSerializer" /> with specified <paramref name="settings" /> will be used.
    /// </remarks>
    /// <param name="instance">The instance to be serialized.</param>
    /// <param name="settings">The settings to configure the default json serializer..</param>
    /// <returns>The serialized <see cref="JToken" /> of <paramref name="instance" />.</returns>
    internal static JToken GetJsonDocument(
        this object instance,
        JsonSerializerSettings settings)
    {
        return GetJsonDocument(instance, JsonSerializer.CreateDefault(settings));
    }

    /// <summary>
    ///     Gets the <see cref="JToken" /> of the <paramref name="instance" />.
    /// </summary>
    /// <remarks>
    ///     The default <see cref="JsonSerializer" /> with default Maverick UPS JSON settings will be used (i.e. using
    ///     StringEnumConverter, ISO date format, ...).
    /// </remarks>
    /// <param name="instance">The instance to be serialized.</param>
    /// <param name="additionalConverters">
    ///     Additional json converter to be used (<see cref="StringEnumConverter" /> is already
    ///     be added).
    /// </param>
    /// <returns>The serialized <see cref="JToken" /> of <paramref name="instance" />.</returns>
    internal static JToken GetJsonDocument(
        this object instance,
        params JsonConverter[] additionalConverters)
    {
        return GetJsonDocument(
            instance,
            new JsonSerializerSettings
            {
                Converters = additionalConverters.Append(new StringEnumConverter())
                    .ToList(),
                Culture = CultureInfo.InvariantCulture,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });
    }
}
