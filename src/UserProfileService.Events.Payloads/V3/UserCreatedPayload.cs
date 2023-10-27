using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.Annotations;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;

namespace UserProfileService.Events.Payloads.V3;

/// <summary>
///     A model wrapping all properties required for creating a user.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class UserCreatedPayload : PayloadBase<UserCreatedPayload>, ICreateModelPayload
{
    /// <summary>
    ///     The name that is used for displaying.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string DisplayName { set; get; }

    /// <summary>
    ///     The domain of the user.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    ///     The email addresses of the user.
    /// </summary>
    [EmailAddress]
    [NotEmptyOrWhitespace]
    public string Email { set; get; }

    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     The first name of the user.
    /// </summary>
    public string FirstName { set; get; }

    /// <summary>
    ///     Used to identify the resource.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string Id { get; set; }

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    public string LastName { set; get; }

    /// <summary>
    ///     Defines the name of the resource.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     Tags to assign to group.
    /// </summary>
    public TagAssignment[] Tags { set; get; } = Array.Empty<TagAssignment>();

    /// <summary>
    ///     The name of the user.
    /// </summary>
    [NotEmptyOrWhitespace]
    public string UserName { set; get; }

    /// <summary>
    ///     The user status
    /// </summary>
    public string UserStatus { get; set; }
}
