using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     The LookUp stored the source id and the maverick id.
///     The external id can be assembled from more than one properties.
/// </summary>
public interface ILookUpObject
{
    /// <summary>
    ///     External id of the object from source.
    /// </summary>
    KeyProperties ExternalId { set; get; }

    /// <summary>
    ///     The created unique maverick id of the object.
    /// </summary>
    string MaverickId { set; get; }

    /// <summary>
    ///     The type of the object that stored the external und internal ids.
    /// </summary>
    ObjectType ObjectType { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    string Source { get; set; }
}
