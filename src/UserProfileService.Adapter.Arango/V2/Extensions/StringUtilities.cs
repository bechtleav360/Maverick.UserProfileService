using System;
using Maverick.UserProfileService.Models.EnumModels;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

// Mainly used in loggers
internal static class StringUtilities
{
    internal static string GetOutputString(this ProfileContainerType type)
    {
        return type == ProfileContainerType.NotSpecified ? "container profile" : type.ToString("G");
    }

    public static string GetOutputString(this RequestedProfileKind profileKind)
    {
        return profileKind switch
        {
            RequestedProfileKind.Group => "group profile",
            RequestedProfileKind.User => "user profile",
            RequestedProfileKind.All => "profile",
            RequestedProfileKind.Organization => "organizational unit profile",
            RequestedProfileKind.Undefined => "profile",
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };
    }
}
