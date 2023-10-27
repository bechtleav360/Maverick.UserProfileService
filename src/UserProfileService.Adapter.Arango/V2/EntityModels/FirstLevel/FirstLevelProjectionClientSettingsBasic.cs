namespace UserProfileService.Adapter.Arango.V2.EntityModels.FirstLevel;

internal class FirstLevelProjectionClientSettingsBasic
{
    /// <summary>
    ///     The key of the client setting.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    ///     The id of the Profile which owns the client settings.
    /// </summary>
    public string ProfileId { get; set; }

    /// <summary>
    ///     The value of the client settings.
    /// </summary>
    /// s
    public string Value { get; set; }
}
