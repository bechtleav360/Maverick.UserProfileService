using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Maverick.Client.ArangoDb.Protocol;

internal static class Serializer
{
    internal static JsonSerializerSettings GetDefaultInternalJsonSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    internal static JsonSerializerSettings GetDefaultJsonSettings()
    {
        return new JsonSerializerSettings
        {
            ContractResolver = new DefaultContractResolver(),
            Formatting = Formatting.None,
            Culture = CultureInfo.InvariantCulture,
            NullValueHandling = NullValueHandling.Ignore
        };
    }

    /// <summary>
    ///     Serialized the object with default json setting, or with specified one to a string.
    /// </summary>
    /// <param name="value">The value that should be serialized.</param>
    /// <param name="settings">
    ///     Optional settings that can be set while creating an arango client. The default json setting will
    ///     than be overwritten.
    /// </param>
    /// <returns>The serialized object within a string.</returns>
    public static string SerializeObject(this object value, JsonSerializerSettings settings)
    {
        if (value == null)
        {
            throw new ArgumentNullException("The object to serialize was null!");
        }

        return JsonConvert.SerializeObject(value, settings);
    }
}
