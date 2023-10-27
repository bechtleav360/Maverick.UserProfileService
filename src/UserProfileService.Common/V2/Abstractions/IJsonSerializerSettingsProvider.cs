using Newtonsoft.Json;

namespace UserProfileService.Common.V2.Abstractions;

/// <summary>
///     Component that provides access to json-serialization-settings throughout the ServiceBase.
/// </summary>
public interface IJsonSerializerSettingsProvider
{
    /// <summary>
    ///     Provides settings for <see cref="Newtonsoft.Json.JsonConverter" /> instances.
    /// </summary>
    /// <returns>Return a <see cref="JsonSerializerSettings" /> object.</returns>
    JsonSerializerSettings GetNewtonsoftSettings();
}
