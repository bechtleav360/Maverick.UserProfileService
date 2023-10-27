using System;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

internal static class FirstLevelProjectionIdentifierExtensions
{
    internal static ObjectIdent ToObjectIdent(this IFirstLevelProjectionSimplifier o)
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
