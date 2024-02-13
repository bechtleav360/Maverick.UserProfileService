using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Annotations;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     User profile model used in the second level projection.
/// </summary>
public class SecondLevelProjectionUser : ISecondLevelProjectionProfile
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
    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

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
    [Readonly]
    public IList<Member> MemberOf { get; set; }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    [Readonly]
    public List<string> Paths { get; set; }

    /// <inheritdoc />
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
