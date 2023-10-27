using System;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Contains extension methods for tag type
/// </summary>
internal static class TagTypeExtensions
{
    internal static TagType? ConvertToTagType(this RequestedTagType tagType)
    {
        return tagType switch
        {
            RequestedTagType.Security => TagType.Security,
            RequestedTagType.Custom => TagType.Custom,
            RequestedTagType.FunctionalAccessRights => TagType.FunctionalAccessRights,
            RequestedTagType.Color => TagType.Color,
            RequestedTagType.All => null,
            _ => throw new ArgumentOutOfRangeException(nameof(tagType), tagType, null)
        };
    }
}
