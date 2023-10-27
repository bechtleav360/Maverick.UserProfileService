using System;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using AggregateMember = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;

namespace UserProfileService.Adapter.Arango.V2.Extensions;

internal static class MapperExtensions
{
    internal static ISecondLevelProjectionProfile MapProfile(this IMapper mapper, IProfileEntityModel profile)
    {
        return profile switch
        {
            UserEntityModel => mapper.Map<SecondLevelProjectionUser>(profile),
            GroupEntityModel => mapper.Map<SecondLevelProjectionGroup>(profile),
            OrganizationEntityModel => mapper.Map<SecondLevelProjectionOrganization>(profile),
            _ => throw new NotSupportedException($"The type '{profile.GetType()}' is not supported by this method.")
        };
    }

    internal static IProfileEntityModel MapProfile(this IMapper mapper, ISecondLevelProjectionProfile profile)
    {
        return profile switch
        {
            SecondLevelProjectionUser => mapper.Map<UserEntityModel>(profile),
            SecondLevelProjectionGroup => mapper.Map<GroupEntityModel>(profile),
            SecondLevelProjectionOrganization => mapper.Map<OrganizationEntityModel>(profile),
            _ => throw new NotSupportedException($"The type '{profile.GetType()}' is not supported by this method.")
        };
    }

    internal static Member MapContainerTypeToAvMember(
        this IMapper mapper,
        ISecondLevelProjectionContainer container)
    {
        return container.ContainerType switch
        {
            ContainerType.Group => mapper.Map<Member>(container as SecondLevelProjectionGroup),
            ContainerType.Role => mapper.Map<Member>(container as SecondLevelProjectionRole),
            ContainerType.Function => mapper.Map<Member>(container as SecondLevelProjectionFunction),
            ContainerType.Organization => mapper.Map<Member>(container as SecondLevelProjectionOrganization),
            ContainerType.NotSpecified => throw new NotSupportedException(
                $"The type '{container.GetType()}' is not supported by this method."),
            _ => throw new NotSupportedException($"The type '{container.GetType()}' is not supported by this method.")
        };
    }

    internal static ILinkedObject MapContainerTypeToLinkedObject(
        this IMapper mapper,
        ISecondLevelProjectionContainer container)
    {
        return container.ContainerType switch
        {
            ContainerType.Role => mapper.Map<LinkedRoleObject>(container as SecondLevelProjectionRole),
            ContainerType.Function => mapper.Map<LinkedFunctionObject>(container as SecondLevelProjectionFunction),
            _ => throw new NotSupportedException($"The type '{container.GetType()}' is not supported by this method.")
        };
    }

    internal static AggregateMember MapContainerTypeToAggregateMember(
        this IMapper mapper,
        ISecondLevelProjectionContainer container)
    {
        return container.ContainerType switch
        {
            ContainerType.Group => mapper.Map<AggregateMember>(container as SecondLevelProjectionGroup),
            ContainerType.Role => mapper.Map<AggregateMember>(container as SecondLevelProjectionRole),
            ContainerType.Function => mapper.Map<AggregateMember>(container as SecondLevelProjectionFunction),
            ContainerType.Organization => mapper.Map<AggregateMember>(container as SecondLevelProjectionOrganization),
            ContainerType.NotSpecified => throw new NotSupportedException(
                $"The type '{container.GetType()}' is not supported by this method."),
            _ => throw new NotSupportedException($"The type '{container.GetType()}' is not supported by this method.")
        };
    }
}
