using Newtonsoft.Json.Linq;

namespace UserProfileService.Adapter.Arango.V2.EntityModels;

internal class ClientSettingsBasic
{
    /// <summary>
    ///     Contains the id of the profile from which the ClientSettings were.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     Contains the key which is used to identify the ClientSettings.
    /// </summary>
    public string SettingsKey { get; set; }

    /// <summary>
    ///     Contains the value of the ClientSettings.
    /// </summary>
    public JObject Value { get; set; }
}
