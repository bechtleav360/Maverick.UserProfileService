using System.Collections.Generic;
using UserProfileService.Adapter.Arango.V2.Contracts;

namespace UserProfileService.Adapter.Arango.V2.Abstractions;

/// <summary>
///     Component that provides access to collections details that can be used
///     to create certain kinds of collections.
/// </summary>
public interface ICollectionDetailsProvider
{
    /// <summary>
    ///     Provides settings for <see cref="CollectionDetails" /> instances.
    /// </summary>
    /// <returns>A List of <see cref="CollectionDetails" />.</returns>
    IEnumerable<CollectionDetails> GetCollectionDetails();
}
