using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Models;

namespace UserProfileService.Sync.Abstractions;

/// <summary>
///     Describes a service to compare objects of <see cref="ISyncModel" />
/// </summary>
public interface ISyncModelComparer<in TSyncModel> where TSyncModel : ISyncModel
{
    /// <summary>
    ///     Compares two objects of the same type considering the sync configuration.
    /// </summary>
    /// <param name="source">Source object with which the target object is to be compared.</param>
    /// <param name="target">
    ///     Target object which contains the modified properties and should be compared with the source
    ///     object.
    /// </param>
    /// <param name="modifiedProperties">All changed properties under consideration of the sync configuration.</param>
    /// <returns>True if the objects are equal, false otherwise.</returns>
    public bool CompareObject(
        TSyncModel source,
        TSyncModel target,
        out IDictionary<string, object> modifiedProperties);
}
