using System;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

/// <summary>
///     Extension class for <see cref="IFirstLevelProjectionSimplifier"/>.
///     Provides extension methods for working with <see cref="ObjectIdent"/>s.
/// </summary>
public static class FirstLevelProjectionIdentifierExtensions
{
    /// <summary>
    ///     Returns an <see cref="ObjectIdent"/> with the appropriate id and <see cref="ObjectType"/>.
    /// </summary>
    /// <param name="o">Either a profile, container or tag object.</param>
    /// <returns>The <see cref="ObjectIdent"/> identifying <paramref name="o"/>.</returns>
    /// <exception cref="NotSupportedException">If the type <paramref name="o"/> was not supported.</exception>
    public static ObjectIdent ToObjectIdent(this IFirstLevelProjectionSimplifier o)
    {
        return o switch
        {
            IFirstLevelProjectionProfile p => new ObjectIdent(p.Id, p.Kind.ToObjectType()),
            IFirstLevelProjectionContainer c => new ObjectIdent(c.Id, c.ContainerType.ToObjectType()),
            FirstLevelProjectionTag tag => new ObjectIdent(tag.Id, ObjectType.Tag),
            _ => throw new NotSupportedException(
                "Neither was the object a profile, a container or a tag. So no conversion could be done.")
        };
    }

    internal static ObjectIdentPath ToObjectIdentPath(this IFirstLevelProjectionSimplifier o)
    {
        return o switch
        {
            IFirstLevelProjectionProfile p => new ObjectIdentPath(p.Id, p.Kind.ToObjectType()),
            IFirstLevelProjectionContainer c => new ObjectIdentPath(c.Id, c.ContainerType.ToObjectType()),
            FirstLevelProjectionTag tag => new ObjectIdentPath(tag.Id, ObjectType.Tag),
            _ => throw new NotSupportedException(
                "Neither was the object a profile, a container or a tag. So no conversion could be done.")
        };
    }
}
