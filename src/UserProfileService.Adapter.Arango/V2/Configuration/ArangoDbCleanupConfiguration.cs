using System;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Configuration;

/// <summary>
///     Contains configuration information used for ArangoDB cleanup providers.
/// </summary>
public class ArangoDbCleanupConfiguration : ICleanupProviderConfiguration
{
    internal string ArangoDbClientName { get; set; } = ArangoConstants.ArangoClientName;

    internal string AssignmentCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.AssignmentProjection;
    internal string EventCollectorCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.EventCollector;
    internal string FirstLevelCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.FirstLevelProjection;
    internal string ServiceCollectionPrefix { get; set; } = WellKnownDatabasePrefixes.ApiService;

    /// <summary>
    ///     The maximum time period for which documents of assignment projections should remain stored.
    /// </summary>
    public TimeSpan? AssignmentProjectionCollection { get; set; } = null;

    /// <summary>
    ///     The maximum time period for which documents of event collectors should remain stored.
    /// </summary>
    public TimeSpan? EventCollectorCollections { get; set; } = TimeSpan.FromDays(2);
    
    /// <summary>
    ///     The maximum time period for which documents of first-level projections should remain stored.
    /// </summary>
    public TimeSpan? FirstLevelProjectionCollection { get; set; } = null;

    /// <summary>
    ///     The maximum time period for which documents of event collector should remain stored.
    /// </summary>
    public TimeSpan? ServiceProjectionCollection { get; set; } = null;

    /// <inheritdoc />
    public string ValidFor => "arango";
}
