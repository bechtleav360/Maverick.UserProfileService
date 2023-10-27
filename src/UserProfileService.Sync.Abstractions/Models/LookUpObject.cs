using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Sync.Abstraction.Models;

/// <summary>
///     Implementation of <see cref="ILookUpObject" />
/// </summary>
public class LookUpObject : ILookUpObject
{
    /// <summary>
    ///     The external id of the object from source.
    /// </summary>
    public KeyProperties ExternalId { set; get; }

    /// <summary>
    ///     The created unique maverick id of the object.
    /// </summary>
    public string MaverickId { set; get; }

    /// <summary>
    ///     The type of the object that stored the external und internal ids.
    /// </summary>
    public ObjectType ObjectType { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Creates an instance of the object <see cref="LookUpObject" />.
    /// </summary>
    /// <param name="externalId">The external id of the object from source.</param>
    /// <param name="maverickId">The created unique maverick id of the object.</param>
    /// <param name="source">The source name where the entity was transferred from.</param>
    /// <param name="objectType">The type of the object that stored the external und internal ids.</param>
    public LookUpObject(KeyProperties externalId, string maverickId, string source, ObjectType objectType)
    {
        ExternalId = externalId;
        MaverickId = maverickId;
        Source = source;
        ObjectType = objectType;
    }
}
