using System;
using System.Collections.Generic;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The user that is used for the first level projection
///     repository.
/// </summary>
public class FirstLevelProjectionUser : IFirstLevelProjectionProfile
{
    /// <inheritdoc />
    public DateTime CreatedAt { get; set; }

    /// <inheritdoc />
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
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     The first name of the user.
    /// </summary>
    public string FirstName { set; get; }

    /// <inheritdoc />
    public string Id { get; set; }

    /// <inheritdoc />
    public ProfileKind Kind => ProfileKind.User;

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    public string LastName { set; get; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <summary>
    ///     The source name where the entity was transferred to (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <inheritdoc />
    public DateTime? SynchronizedAt { get; set; }

    /// <inheritdoc />
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    ///     The name of the user.
    /// </summary>
    public string UserName { set; get; }

    /// <summary>
    ///     The image url of the group.
    /// </summary>
    public string UserStatus { set; get; }
}
