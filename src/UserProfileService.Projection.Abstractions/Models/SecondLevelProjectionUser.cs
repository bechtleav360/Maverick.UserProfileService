using System;
using System.Collections.Generic;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using UserProfileService.Projection.Abstractions.Annotations;

namespace UserProfileService.Projection.Abstractions.Models;

public class SecondLevelProjectionUser : ISecondLevelProjectionProfile
{
    public DateTime CreatedAt { get; set; }
    public string DisplayName { get; set; }

    /// <summary>
    ///     The domain of the user.
    /// </summary>
    public string Domain { get; set; }

    /// <summary>
    ///     The email addresses of the user.
    /// </summary>
    public string Email { set; get; }

    public IList<ExternalIdentifier> ExternalIds { get; set; } = new List<ExternalIdentifier>();

    /// <summary>
    ///     The first name of the user.
    /// </summary>

    public string FirstName { set; get; }

    public string Id { get; set; }
    public ProfileKind Kind => ProfileKind.User;

    /// <summary>
    ///     The last name of the user.
    /// </summary>
    public string LastName { set; get; }

    [Readonly]
    public IList<Member> MemberOf { get; set; }

    public string Name { get; set; }

    [Readonly]
    public List<string> Paths { get; set; }

    public string Source { get; set; }
    public DateTime? SynchronizedAt { get; set; }
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
