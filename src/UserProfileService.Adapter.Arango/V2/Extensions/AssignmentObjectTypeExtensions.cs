using System;
using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class AssignmentObjectTypeExtensions
{
    /// <summary>
    ///     Gets all type names of suitable entities of a flag in <paramref name="type" />.
    /// </summary>
    /// <param name="type">The assignment object type to be converted.</param>
    /// <returns>A list of type names of all suitable entities of <paramref name="type" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the value exceeds the <see cref="RequestedAssignmentObjectType" />
    ///     enum/flag.
    /// </exception>
    internal static List<string> GetEntityTypeNames(this RequestedAssignmentObjectType type)
    {
        return type.GetSingleFlagValues()
            .Where(s => s != RequestedAssignmentObjectType.Undefined)
            .Select(s => s.GetEntityTypeName())
            .ToList();
    }

    /// <summary>
    ///     Gets the type name of a suitable entity of a <paramref name="type" />.
    /// </summary>
    /// <remarks>
    ///     This method does not support multiple flag values and will maybe throw an
    ///     <see cref="ArgumentOutOfRangeException" />.
    /// </remarks>
    /// <param name="type">The assignment object type to be converted.</param>
    /// <returns>A type name of the suitable entity of <paramref name="type" />.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    ///     If the value exceeds the <see cref="RequestedAssignmentObjectType" />
    ///     enum/flag.
    /// </exception>
    internal static string GetEntityTypeName(this RequestedAssignmentObjectType type)
    {
        return type switch
        {
            RequestedAssignmentObjectType.Function => nameof(FunctionObjectEntityModel),
            RequestedAssignmentObjectType.Role => nameof(RoleObjectEntityModel),
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Only single values are allowed by this method.")
        };
    }
}
