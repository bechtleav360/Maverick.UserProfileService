using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;

namespace UserProfileService.Events.Payloads.V2;

/// <summary>
///     Is a wrapper that contains all necessary information to created a tag object.
///     Be aware, the versioning is related to the Payload and not to the API.
/// </summary>
public class TagCreatedPayload : PayloadBase<TagCreatedPayload>, ICreateModelPayload
{
    /// <inheritdoc />
    public IList<ExternalIdentifier> ExternalIds { get; set; }

    /// <summary>
    ///     The id representing the unique identifier of this tag.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name that will be used to tag or to classify the related resource.
    ///     The name is only used if no reference to an object is specified.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     The source name where the entity was transferred from (i.e. API, active directory).
    /// </summary>
    public string Source { get; set; }

    /// <summary>
    ///     The type of the tag. For for information see <see cref="TagType" />.
    /// </summary>
    [Required]
    public TagType Type { get; set; } = TagType.Custom;
}
