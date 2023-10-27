namespace UserProfileService.Utilities;

/// <summary>
///     Contains constants for the UPS-API
/// </summary>
public class SyncConstants
{
    /// <summary>
    ///     Named used to register and identify the sync http client.
    /// </summary>
    public const string SyncClient = "SyncHttpClient";

    /// <summary>
    ///     Section name in the UPS configuration where the UPS-Sync endpoint is located.
    /// </summary>
    public const string SyncConfigSection = "SyncProxyConfiguration";
}
