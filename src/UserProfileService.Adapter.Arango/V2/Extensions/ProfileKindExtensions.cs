using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

/// <summary>
///     Contains extension methods for profile kinds
///     (i.e. <see cref="ProfileKind" />, <see cref="ProfileContainerType" />, <see cref="RequestedProfileKind" />).
/// </summary>
internal static class ProfileKindExtensions
{
    /// <summary>
    ///     Gets an AQL query string as part of a filter expression.
    /// </summary>
    /// <param name="profileKind">The profile kind to filter for.</param>
    /// <param name="left">The string representing the iterating variable in the for-loop of the AQL.</param>
    /// <returns>The AQL query string.</returns>
    internal static string GetAqlFilterPart(
        this RequestedProfileKind profileKind,
        string left)
    {
        if (profileKind is RequestedProfileKind.User or RequestedProfileKind.Group or RequestedProfileKind.Organization)
        {
            return $"{left}=={profileKind.GetPropertyValueToFilterByKind()}";
        }

        return string.Empty;
    }

    /// <summary>
    ///     Gets an AQL query string as part of a filter expression.
    /// </summary>
    /// <param name="type">The assignment object type to filter for.</param>
    /// <param name="left">The string representing the iterating variable in the for-loop of the AQL.</param>
    /// <returns>The AQL query string.</returns>
    internal static string GetAqlFilterPart(
        this RequestedAssignmentObjectType type,
        string left)
    {
        if (type is RequestedAssignmentObjectType.Role or RequestedAssignmentObjectType.Function)
        {
            return $"{left}=={type.GetPropertyValueToFilterByType()}";
        }

        return string.Empty;
    }

    /// <summary>
    ///     Creates a <see cref="RawQueryExpression" /> to filter requested <paramref name="type" />.
    /// </summary>
    /// <param name="type">The profile to filter for.</param>
    /// <returns>The resulting <see cref="RawQueryExpression" />.</returns>
    internal static RawQueryExpression CreateRaqQueryExpression(this RequestedAssignmentObjectType type)
    {
        return RawQueryExpression.CreateInstance<IContainerProfileEntityModel, ProfileKind>(
            left =>
            {
                List<string> kindList = type.GetSingleFlagValues()
                    .Where(k => k != RequestedAssignmentObjectType.Undefined)
                    .Select(k => k.GetAqlFilterPart(left))
                    .ToList();

                return string.Concat(
                    kindList.Count > 1 ? "(" : string.Empty,
                    string.Join(
                        " OR ",
                        kindList),
                    kindList.Count > 1 ? ")" : string.Empty);
            },
            e => e.Kind);
    }

    /// <summary>
    ///     Creates a <see cref="RawQueryExpression" /> to filter requested <paramref name="profileKind" />.
    /// </summary>
    /// <param name="profileKind">The profile to filter for.</param>
    /// <returns>The resulting <see cref="RawQueryExpression" />.</returns>
    internal static RawQueryExpression CreateRaqQueryExpression(this RequestedProfileKind profileKind)
    {
        return RawQueryExpression.CreateInstance<IContainerProfileEntityModel, ProfileKind>(
            left =>
            {
                List<string> kindList = profileKind.GetSingleFlagValues()
                    .Where(k => k != RequestedProfileKind.Undefined)
                    .Select(k => k.GetAqlFilterPart(left))
                    .ToList();

                return string.Concat(
                    kindList.Count > 1 ? "(" : string.Empty,
                    string.Join(
                        " OR ",
                        kindList),
                    kindList.Count > 1 ? ")" : string.Empty);
            },
            e => e.Kind);
    }

    /// <summary>
    ///     Converts a profile kind to it's representation to be used in GET-PARENT requests in read service.
    /// </summary>
    /// <remarks>It won't handle combination of simple enum values although <see cref="RequestedProfileKind" /> is a flag.</remarks>
    /// <param name="profileKind">The profile kind to be converted.</param>
    /// <returns>A new <see cref="RequestedProfileKind" /> suitable for read operations to get all parents.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the value exceeds the <see cref="RequestedProfileKind" /> enum/flag.</exception>
    internal static RequestedProfileKind ConvertToParentRequestedProfileKind(this RequestedProfileKind profileKind)
    {
        return profileKind switch
        {
            RequestedProfileKind.User => RequestedProfileKind.Group,
            RequestedProfileKind.Organization => RequestedProfileKind.Organization,
            RequestedProfileKind.Group => RequestedProfileKind.Group,
            RequestedProfileKind.Undefined => RequestedProfileKind.Undefined,
            RequestedProfileKind.All => RequestedProfileKind.All,
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };
    }

    /// <summary>
    ///     Converts a container profile kind to it's representation to be used in GET-CHILDREN requests in read service.
    /// </summary>
    /// <param name="profileKind">The kind of the container profile to be converted.</param>
    /// <returns>A new <see cref="RequestedProfileKind" /> suitable for read operations to get all children.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the value exceeds the <see cref="ProfileContainerType" /> enum.</exception>
    internal static RequestedProfileKind ConvertToChildrenRequestedProfileKind(
        this ProfileContainerType profileKind)
    {
        return profileKind switch
        {
            ProfileContainerType.Organization => RequestedProfileKind.Organization,
            ProfileContainerType.Group => RequestedProfileKind.Group | RequestedProfileKind.User,
            ProfileContainerType.NotSpecified => RequestedProfileKind.Undefined,
            _ => throw new ArgumentOutOfRangeException(nameof(profileKind), profileKind, null)
        };
    }

    /// <summary>
    ///     Gets all type names of suitable entities of a flag in <paramref name="profileKind" />.
    /// </summary>
    /// <param name="profileKind">The profile kind to be converted.</param>
    /// <returns>A list of type names of all suitable entities of <paramref name="profileKind" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the value exceeds the <see cref="RequestedProfileKind" /> enum/flag.</exception>
    internal static List<string> GetEntityTypeNames(this RequestedProfileKind profileKind)
    {
        return profileKind.GetSingleFlagValues()
            .Where(s => s != RequestedProfileKind.Undefined)
            .Select(s => s.GetEntityTypeName())
            .ToList();
    }

    /// <summary>
    ///     Gets the type name of a suitable entity of a <paramref name="profileKind" />.
    /// </summary>
    /// <remarks>
    ///     This method does not support multiple flag values and will maybe throw an
    ///     <see cref="ArgumentOutOfRangeException" />.
    /// </remarks>
    /// <param name="profileKind">The profile kind to be converted.</param>
    /// <returns>A type name of the suitable entity of <paramref name="profileKind" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the value exceeds the <see cref="RequestedProfileKind" /> enum/flag.</exception>
    internal static string GetEntityTypeName(this RequestedProfileKind profileKind)
    {
        return profileKind switch
        {
            RequestedProfileKind.User => nameof(UserEntityModel),
            RequestedProfileKind.Group => nameof(GroupEntityModel),
            RequestedProfileKind.Organization => nameof(OrganizationEntityModel),
            _ => throw new ArgumentOutOfRangeException(
                nameof(profileKind),
                profileKind,
                "Only single values are allowed by this method.")
        };
    }

    /// <summary>
    ///     Gets the value of an enum to use in AQL filter strings.
    /// </summary>
    /// <param name="enum">The enum to be used in filter.</param>
    /// <returns>The string representation of <paramref name="enum" /> to be used in filter strings.</returns>
    internal static string GetPropertyValueToFilter(this Enum @enum)
    {
        return @enum switch
        {
            ProfileKind kind => kind.GetPropertyValueToFilterByKind(),
            RequestedProfileKind reqKind => reqKind.GetPropertyValueToFilterByKind(),
            RequestedAssignmentObjectType reqKind => reqKind.GetPropertyValueToFilterByType(),
            _ => $"\"{@enum:G}\""
        };
    }

    /// <summary>
    ///     Gets the value of <see cref="RequestedAssignmentObjectType" /> to use in AQL filter strings.
    /// </summary>
    /// <param name="type">The enum to be used in filter.</param>
    /// <returns>The string representation of <paramref name="type" /> to be used in filter strings.</returns>
    internal static string GetPropertyValueToFilterByType(this RequestedAssignmentObjectType type)
    {
        if (type is RequestedAssignmentObjectType.All or RequestedAssignmentObjectType.Undefined)
        {
            return string.Empty;
        }

        return $"\"{type:G}\"";
    }

    /// <summary>
    ///     Gets the value of <see cref="RequestedProfileKind" /> to use in AQL filter strings.
    /// </summary>
    /// <param name="profileKind">The enum to be used in filter.</param>
    /// <returns>The string representation of <paramref name="profileKind" /> to be used in filter strings.</returns>
    internal static string GetPropertyValueToFilterByKind(this RequestedProfileKind profileKind)
    {
        return profileKind.Convert().GetPropertyValueToFilterByKind();
    }

    /// <summary>
    ///     Gets the value of <see cref="ProfileKind" /> to use in AQL filter strings.
    /// </summary>
    /// <param name="profileKind">The enum to be used in filter.</param>
    /// <returns>The string representation of <paramref name="profileKind" /> to be used in filter strings.</returns>
    internal static string GetPropertyValueToFilterByKind(this ProfileKind profileKind)
    {
        return profileKind == ProfileKind.Unknown 
            ? string.Empty
            : $"\"{profileKind:G}\"";
    }
}
