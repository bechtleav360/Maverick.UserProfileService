using System;
using System.Linq;
using Newtonsoft.Json;

namespace UserProfileService.Common.V2.Extensions;

/// <summary>
///     Contains methods related to Newtonsoft Json.
/// </summary>
public static class JsonExtensions
{
    /// <summary>
    ///     Gets new settings extracted from a provided <paramref name="serializer" />.
    /// </summary>
    /// <remarks>
    ///     Not all settings can be extracted. <see cref="JsonSerializerSettings.Error" /> and
    ///     <see cref="JsonSerializerSettings.ReferenceResolverProvider" /> will be set to default values.<br />
    ///     Reference types won't be copied, except the list, that contains converters.
    /// </remarks>
    /// <param name="serializer">The serializer whose settings will be taken.</param>
    /// <returns><see cref="JsonSerializerSettings" /> extracted from a <see cref="JsonSerializer" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="serializer" /> is <c>null</c></exception>
    public static JsonSerializerSettings GetSettings(this JsonSerializer serializer)
    {
        if (serializer == null)
        {
            throw new ArgumentNullException(nameof(serializer));
        }

        return new JsonSerializerSettings
        {
            CheckAdditionalContent = serializer.CheckAdditionalContent,
            ConstructorHandling = serializer.ConstructorHandling,
            Culture = serializer.Culture,
            DateFormatHandling = serializer.DateFormatHandling,
            DateFormatString = serializer.DateFormatString,
            DateParseHandling = serializer.DateParseHandling,
            DateTimeZoneHandling = serializer.DateTimeZoneHandling,
            DefaultValueHandling = serializer.DefaultValueHandling,
            FloatFormatHandling = serializer.FloatFormatHandling,
            FloatParseHandling = serializer.FloatParseHandling,
            Formatting = serializer.Formatting,
            MaxDepth = serializer.MaxDepth,
            MetadataPropertyHandling = serializer.MetadataPropertyHandling,
            MissingMemberHandling = serializer.MissingMemberHandling,
            NullValueHandling = serializer.NullValueHandling,
            ObjectCreationHandling = serializer.ObjectCreationHandling,
            PreserveReferencesHandling = serializer.PreserveReferencesHandling,
            ReferenceLoopHandling = serializer.ReferenceLoopHandling,
            StringEscapeHandling = serializer.StringEscapeHandling,
            TypeNameAssemblyFormatHandling = serializer.TypeNameAssemblyFormatHandling,
            TypeNameHandling = serializer.TypeNameHandling,
            Converters = serializer.Converters.ToList(),
            TraceWriter = serializer.TraceWriter,
            Context = serializer.Context,
            ContractResolver = serializer.ContractResolver,
            EqualityComparer = serializer.EqualityComparer,
            SerializationBinder = serializer.SerializationBinder
        };
    }

    /// <summary>
    ///     Removes a converter from a list of converters in provided <paramref name="settings" />
    /// </summary>
    /// <param name="settings">The settings whose convert list should be modified.</param>
    /// <param name="jsonConverter">The <see cref="JsonConverter" /> to be removed.</param>
    /// <returns>The modified <paramref name="settings" /> instance that can used further in a "fluent" way.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="settings" /> is <c>null</c></exception>
    public static JsonSerializerSettings RemoveConverter(
        this JsonSerializerSettings settings,
        JsonConverter jsonConverter)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        settings.Converters.Remove(jsonConverter);

        return settings;
    }
}
