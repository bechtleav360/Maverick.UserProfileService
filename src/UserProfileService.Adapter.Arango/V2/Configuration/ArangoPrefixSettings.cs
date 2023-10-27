using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Contains the configuration about collection name prefixes used by UPS in ArangoDb.
/// </summary>
public class ArangoPrefixSettings
{
    /// <summary>
    ///     The prefix of names of all assignment-projection collections.
    /// </summary>
    public string AssignmentsCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.AssignmentProjection;

    /// <summary>
    ///     The prefix of names of all event collector collections.
    /// </summary>
    public string EventCollectorCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.EventCollector;
    
    /// <summary>
    ///     The prefix of names of all first-level-projection collections.
    /// </summary>
    public string FirstLevelCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.FirstLevelProjection;

    /// <summary>
    ///     The prefix of names of all service related collections.
    /// </summary>
    public string ServiceCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.ApiService;
}
