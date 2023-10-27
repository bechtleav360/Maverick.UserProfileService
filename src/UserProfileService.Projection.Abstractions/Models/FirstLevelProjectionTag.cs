using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Projection.Abstractions.Models;

/// <summary>
///     The tag that are used for the first level projection repository.
/// </summary>
public class FirstLevelProjectionTag : IFirstLevelProjectionSimplifier
{
    /// <summary>
    ///     The id representing the unique identifier of this tag.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    ///     The name that will be used to tag or to classify the related resource.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    ///     The type of the tag. For for information see <see cref="TagType" />.
    /// </summary>
    public TagType Type { get; set; } = TagType.Custom;
}
