using System;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using AVMember = Maverick.UserProfileService.Models.Models.Member;
using AVTagType = Maverick.UserProfileService.Models.EnumModels.TagType;
using ExternalIdentifier = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using Group = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using Organization = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using TagType = Maverick.UserProfileService.AggregateEvents.Common.Enums.TagType;
using User = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.User;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Includes mapping profiles for <see cref="AutoMapper" /> related SecondLevel.
/// </summary>
public class MappingProfiles : Profile
{
    /// <summary>
    ///     Initializes a new instance of the mapper and creates a default mapping.
    /// </summary>
    public MappingProfiles()
    {
        CreateMap<UserCreated, UserBasic>();
        CreateMap<GroupCreated, GroupBasic>();
        CreateMap<OrganizationCreated, OrganizationBasic>();

        CreateMap<FunctionCreated, FunctionBasic>()
            .ForMember(
                f => f.Name,
                options
                    => options.MapFrom(
                        (created, model)
                            => model.Name = GenerateFunctionName(created)))
            // both types have a type property, but this is different.
            .ForMember(
                f => f.Type,
                options =>
                    options.MapFrom(_ => RoleType.Function))
            .ForMember(
                f => f.OrganizationId,
                options =>
                    options.MapFrom(
                        createdEvent =>
                            createdEvent.Organization != null
                                ? createdEvent.Organization.Id
                                : null))
            .ForMember(
                f => f.RoleId,
                options =>
                    options.MapFrom(
                        createdEvent =>
                            createdEvent.Role != null
                                ? createdEvent.Role.Id
                                : null));

        CreateMap<RoleCreated, RoleBasic>()
            // both types have a type property, but this is different.
            .ForMember(
                f => f.Type,
                options =>
                    options.MapFrom(_ => RoleType.Role));

        CreateMap<UserCreated, User>().ReverseMap();
        CreateMap<GroupCreated, Group>().ReverseMap();
        CreateMap<OrganizationCreated, Organization>().ReverseMap();

        CreateMap<FunctionCreated, Function>()
            .ForMember(
                f => f.OrganizationId,
                options =>
                    options.MapFrom(
                        createdEvent =>
                            createdEvent.Organization != null
                                ? createdEvent.Organization.Id
                                : null))
            .ForMember(
                f => f.RoleId,
                options =>
                    options.MapFrom(
                        createdEvent =>
                            createdEvent.Role != null
                                ? createdEvent.Role.Id
                                : null))
            .ReverseMap();

        CreateMap<SecondLevelProjectionUser, User>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, Group>().ReverseMap();
        CreateMap<SecondLevelProjectionOrganization, Organization>().ReverseMap();
        CreateMap<SecondLevelProjectionRole, Role>().ReverseMap();

        CreateMap<UserCreated, SecondLevelProjectionUser>().ReverseMap();
        CreateMap<GroupCreated, SecondLevelProjectionGroup>().ReverseMap();
        CreateMap<OrganizationCreated, SecondLevelProjectionOrganization>().ReverseMap();

        CreateMap<RoleCreated, SecondLevelProjectionRole>().ReverseMap();

        CreateMap<ISecondLevelProjectionProfile, IProfileEntityModel>().ReverseMap();
        CreateMap<SecondLevelProjectionUser, UserEntityModel>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, GroupEntityModel>().ReverseMap();
        CreateMap<SecondLevelProjectionFunction, FunctionObjectEntityModel>().ReverseMap();
        CreateMap<SecondLevelProjectionRole, RoleObjectEntityModel>().ReverseMap();
        CreateMap<SecondLevelProjectionOrganization, OrganizationEntityModel>().ReverseMap();
        CreateMap<Role, RoleBasic>().ReverseMap();

        CreateMap<ExternalIdentifier,
                Maverick.UserProfileService.Models.Models.ExternalIdentifier>()
            .ReverseMap();

        CreateMap<Member, AVMember>().ReverseMap();

        CreateMap<RangeCondition, Maverick.UserProfileService.Models.Models.RangeCondition>()
            .ReverseMap();

        CreateMap<Organization, OrganizationBasic>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, Member>().ReverseMap();
        CreateMap<SecondLevelProjectionFunction, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionFunction, Member>().ReverseMap();
        CreateMap<SecondLevelProjectionRole, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionRole, Member>().ReverseMap();
        CreateMap<SecondLevelProjectionOrganization, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionOrganization, Member>().ReverseMap();
        CreateMap<SecondLevelProjectionUser, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionUser, Member>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, AVMember>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, Member>().ReverseMap();
        CreateMap<Member, AVMember>();
        CreateMap<Member, IContainerProfile>().ReverseMap();
        CreateMap<Member, Organization>();
        CreateMap<Member, OrganizationBasic>();
        CreateMap<Member, OrganizationView>();
        CreateMap<Member, ConditionalOrganization>();
        CreateMap<SecondLevelProjectionRole, LinkedRoleObject>().ReverseMap();
        CreateMap<TagType, AVTagType>().ReverseMap();
        CreateMap<Tag, TagAssignment>();
    }

    private static string GenerateFunctionName(FunctionCreated functionCreated)
    {
        if (functionCreated == null)
        {
            throw new ArgumentNullException(nameof(functionCreated));
        }

        if (functionCreated.Role == null)
        {
            throw new ArgumentException(
                "The role in function created must not be null.",
                nameof(functionCreated));
        }

        if (functionCreated.Organization == null)
        {
            throw new ArgumentException(
                "The organization in function created must not be null.",
                nameof(functionCreated));
        }

        return $"{functionCreated.Organization.Name} {functionCreated.Role.Name}";
    }
}
