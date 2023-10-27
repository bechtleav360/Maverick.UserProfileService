using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Contracts;

/// <summary>
///     Contains well-known constants for database implementations.
/// </summary>
public static class WellKnownDatabaseKeys
{
    /// <summary>
    ///     Defines the prefix of collection filled with entities of the sync.
    /// </summary>
    public const string CollectionPrefixSync = WellKnownDatabasePrefixes.Sync;

    /// <summary>
    ///     Defines the prefix of collection filled with entities of the user profile service.
    /// </summary>
    public const string CollectionPrefixUserProfileService = WellKnownDatabasePrefixes.ApiService;
}
