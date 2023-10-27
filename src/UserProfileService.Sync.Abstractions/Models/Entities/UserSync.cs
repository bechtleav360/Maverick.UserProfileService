using System;
using System.Collections.Generic;
using UserProfileService.Sync.Abstraction.Annotations;
using UserProfileService.Sync.Abstraction.Contracts;

namespace UserProfileService.Sync.Abstraction.Models.Entities;

/// <summary>
///     The implementation of <see cref="ISyncModel" /> for users.
/// </summary>
[Model(SyncConstants.Models.User)]
public class UserSync : ISyncProfile
{
    /// <summary>
    ///     Defines the display name of the user.
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    ///     The domain of the user.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    ///     The email addresses of the user.
    /// </summary>
    public string Email { set; get; }

    /// <inheritdoc />
    public IList<KeyProperties> ExternalIds { get; set; }

    /// <summary>
    ///     The first name of the user.
    /// </summary>
    public string FirstName { set; get; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc cref="ISyncProfile.Kind" />
    public ProfileKind Kind { get; set; } = ProfileKind.User;

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    public string LastName { set; get; }

    /// <summary>
    ///     Defines the name of the user.
    /// </summary>
    public string Name { get; set; }

    /// <inheritdoc />
    public List<ObjectRelation> RelatedObjects { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The time stamp when the object has been synchronized the last time.
    /// </summary>
    public DateTime? SynchronizedAt { set; get; }

    /// <summary>
    ///     The name of the user.
    /// </summary>
    public string UserName { set; get; }
}
