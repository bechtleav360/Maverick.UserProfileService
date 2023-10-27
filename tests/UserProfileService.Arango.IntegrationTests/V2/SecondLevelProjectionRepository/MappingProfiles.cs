using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository.Seeding.Models;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using Xunit.Sdk;
using AVMember = Maverick.UserProfileService.Models.Models.Member;
using ExternalIdentifier = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using Group = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using Organization = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using RangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using User = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.User;

namespace UserProfileService.Arango.IntegrationTests.V2.SecondLevelProjectionRepository
{
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

            CreateMap<SecondLevelProjectionFunction, LinkedFunctionObject>()
                .ForMember(
                    f => f.Name,
                    options
                        => options.MapFrom(
                            (function, model)
                                => model.Name = function.GenerateFunctionName()))
                .ReverseMap();
            
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
            CreateMap<SecondLevelProjectionUser, UserBasic>().ReverseMap();
            CreateMap<SecondLevelProjectionGroup, GroupBasic>().ReverseMap();
            CreateMap<SecondLevelProjectionOrganization, OrganizationBasic>().ReverseMap();
            CreateMap<SecondLevelProjectionRole, Role>().ReverseMap();
            CreateMap<SecondLevelProjectionFunction, Function>().ReverseMap();

            CreateMap<UserCreated, SecondLevelProjectionUser>().ReverseMap();
            CreateMap<GroupCreated, SecondLevelProjectionGroup>().ReverseMap();
            CreateMap<OrganizationCreated, SecondLevelProjectionOrganization>().ReverseMap();
            
            CreateMap<RoleCreated, SecondLevelProjectionRole>().ReverseMap();

            CreateMap<SecondLevelProjectionFunction, FunctionBasic>()
                .ForMember(
                    basic => basic.Name,
                    setup => setup.MapFrom(
                        (projection, _) => $"{projection.Organization?.Name} {projection.Role?.Name}"))
                .ForMember(
                    basic => basic.CreatedAt,
                    setup => setup.MapFrom((projection, _) => projection.CreatedAt))
                .ReverseMap();

            CreateMap<SecondLevelProjectionRole, RoleBasic>()
                .ForMember(
                    basic => basic.DeniedPermissions,
                    options =>
                        options.NullSubstitute(new List<string>()))
                .ForMember(
                    basic => basic.Permissions,
                    options =>
                        options.NullSubstitute(new List<string>()))
                .ReverseMap();

            CreateMap<FunctionView, FunctionObjectEntityModel>().ReverseMap();
            CreateMap<RoleView, RoleObjectEntityModel>().ReverseMap();
            CreateMap<OrganizationView, OrganizationEntityModel>().ReverseMap();

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
            CreateMap<SecondLevelProjectionRole, LinkedRoleObject>().ReverseMap();
            CreateMap<Member, IContainerProfile>().ReverseMap();
            CreateMap<Member, Organization>();
            CreateMap<Member, OrganizationBasic>();
            CreateMap<Member, OrganizationView>();
            CreateMap<Member, ConditionalOrganization>();
            CreateMap<CalculatedTag, Tag>();

            CreateMap<SingleAssignment, RangeCondition>();

            CreateMap<UserBasic, UserEntityModel>();

            CreateMap<UserView, UserEntityModel>()
                .IncludeBase<UserBasic, UserEntityModel>()
                .ForMember(
                    u => u.Functions,
                    options => options.Ignore())
                .ForMember(
                    u => u.MemberOf,
                    options => options.Ignore())
                .ForMember(
                    g => g.Tags,
                    options => options.Ignore())
                .ForMember(
                    g => g.SecurityAssignments,
                    options =>
                        options.MapFrom((_, model) => model.SecurityAssignments = new List<ILinkedObject>()));

            CreateMap<UserEntityModel, UserSync>()
                .ForMember(u => u.RelatedObjects, options => options.Ignore())
                .ForMember(u => u.Domain, options => options.Ignore())
                .ForMember(u => u.ExternalIds, m => m.MapFrom(
                                                   u => u.ExternalIds.Select(
                                                       r => new KeyProperties(
                                                           r.Id,
                                                           r.Source,
                                                           null,
                                                           r.IsConverted))));
            CreateMap<GroupEntityModel, GroupSync>()
                .ForMember(u => u.RelatedObjects, options => options.Ignore())
                .ForMember(u => u.ExternalIds, m => m.MapFrom(
                                                   u => u.ExternalIds.Select(
                                                       r => new KeyProperties(
                                                           r.Id,
                                                           r.Source,
                                                           null,
                                                           r.IsConverted))));

            CreateMap<RoleObjectEntityModel, RoleSync>()
                .ForMember(u => u.RelatedObjects, options => options.Ignore())
                .ForMember(u => u.ExternalIds, m => m.MapFrom(
                                                   u => u.ExternalIds.Select(
                                                       r => new KeyProperties(
                                                           r.Id,
                                                           r.Source,
                                                           null,
                                                           r.IsConverted))));

            CreateMap<FunctionObjectEntityModel, FunctionSync>()
                .ForMember(u => u.RelatedObjects, options => options.Ignore())
                .ForMember(u => u.ExternalIds, m => m.MapFrom(
                                                   u => u.ExternalIds.Select(
                                                       r => new KeyProperties(
                                                           r.Id,
                                                           r.Source,
                                                           null,
                                                           r.IsConverted))));

            CreateMap<OrganizationEntityModel, OrganizationSync>()
                .ForMember(u => u.RelatedObjects, options => options.Ignore())
                .ForMember(u => u.RelatedObjects, m => m.MapFrom(o => ExtractObjectRelations(o)))
                .ForMember(u => u.ExternalIds, m => m.MapFrom(
                                                   u => u.ExternalIds.Select(
                                                       r => new KeyProperties(
                                                           r.Id,
                                                           r.Source,
                                                           null,
                                                           r.IsConverted))));

            CreateMap<GroupBasic, GroupEntityModel>();

            CreateMap<GroupView, GroupEntityModel>()
                .IncludeBase<GroupBasic, GroupEntityModel>()
                .ForMember(
                    u => u.MemberOf,
                    options => options.Ignore())
                .ForMember(
                    u => u.Members,
                    options => options.Ignore())
                .ForMember(
                    g => g.Tags,
                    options => options.Ignore())
                .ForMember(
                    g => g.ChildrenCount,
                    options => options.MapFrom(
                        (_, model) => model.ChildrenCount = model.Members.Count(
                            m => m.Kind == ProfileKind.Group
                                || m.Kind == ProfileKind.User)))
                .ForMember(
                    g => g.HasChildren,
                    options => options.MapFrom(
                        (_, model) => model.HasChildren = model.Members.Any(
                            m => m.Kind == ProfileKind.Group
                                || m.Kind == ProfileKind.User)))
                .ForMember(
                    g => g.SecurityAssignments,
                    options =>
                        options.MapFrom((_, model) => model.SecurityAssignments = new List<ILinkedObject>()));

            CreateMap<OrganizationBasic, OrganizationEntityModel>();

            CreateMap<OrganizationView, OrganizationEntityModel>()
                .IncludeBase<OrganizationBasic, OrganizationEntityModel>()
                .ForMember(
                    u => u.MemberOf,
                    options => options.Ignore())
                .ForMember(
                    u => u.Members,
                    options => options.Ignore())
                .ForMember(
                    g => g.Tags,
                    options => options.Ignore())
                .ForMember(
                    g => g.HasChildren,
                    options => options.MapFrom(
                        (_, model) => model.HasChildren = model.Members.Any(m => m.Kind == ProfileKind.Organization)));

            CreateMap<IProfile, AVMember>();
            CreateMap<UserEntityModel, AVMember>();
            CreateMap<OrganizationEntityModel, AVMember>();
            CreateMap<GroupEntityModel, AVMember>();

            CreateMap<ISecondLevelProjectionProfile, IProfile>()
                .Include<SecondLevelProjectionUser, UserBasic>()
                .Include<SecondLevelProjectionGroup, GroupBasic>()
                .Include<SecondLevelProjectionOrganization, OrganizationBasic>()
                .ConstructUsing(
                    (source, context) =>
                    {
                        return source.Kind switch
                        {
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Unknown =>
                                throw NotSameException.ForSameValues(),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.User => context
                                                                             .Mapper.Map<UserBasic>(source),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Group => context
                                                                              .Mapper.Map<GroupBasic>(source),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Organization =>
                                context.Mapper.Map<OrganizationBasic>(source),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });

            CreateMap<ISecondLevelProjectionProfile, IProfileEntityModel>()
                .Include<SecondLevelProjectionUser, UserEntityModel>()
                .Include<SecondLevelProjectionGroup, GroupEntityModel>()
                .Include<SecondLevelProjectionOrganization, OrganizationEntityModel>()
                .ConstructUsing(
                    (source, context) =>
                    {
                        return source.Kind switch
                        {
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Unknown =>
                                throw NotSameException.ForSameValues(),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.User => context
                                                                             .Mapper.Map<UserEntityModel>(source),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Group => context
                                                                              .Mapper.Map<GroupEntityModel>(source),
                            Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind.Organization =>
                                context.Mapper.Map<OrganizationEntityModel>(source),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });

            CreateMap<FunctionObjectEntityModel, LinkedFunctionObject>()
                .ForMember(
                    funcObj => funcObj.Name,
                    setup => setup.MapFrom((entity, _) => $"{entity.Organization?.Name} {entity.Role?.Name}"));

            CreateMap<RoleObjectEntityModel, LinkedRoleObject>();

            CreateMap<IAssignmentObjectEntity, ILinkedObject>()
                .Include<FunctionObjectEntityModel, LinkedFunctionObject>()
                .Include<RoleObjectEntityModel, LinkedRoleObject>()
                .ConstructUsing(
                    (source, context) =>
                    {
                        return source.Type switch
                        {
                            RoleType.Function => context.Mapper.Map<LinkedFunctionObject>(source),
                            RoleType.Role => context.Mapper.Map<LinkedRoleObject>(source),
                            _ => throw new ArgumentOutOfRangeException()
                        };
                    });
            
            CreateMap<SecondLevelProjectionRole, LinkedRoleObject>();

            CreateMap<SecondLevelProjectionFunction, ILinkedObject>()
                .Include<SecondLevelProjectionFunction, LinkedFunctionObject>()
                .ForMember(
                    lo => lo.Type,
                    options
                        => options.MapFrom(function => function.ContainerType.ToString()));

            CreateMap<SecondLevelProjectionRole, ILinkedObject>()
                .Include<SecondLevelProjectionRole, LinkedRoleObject>()
                .ForMember(
                    lo => lo.Type,
                    options
                        => options.MapFrom(role => role.ContainerType.ToString()));
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

        private static IList<ObjectRelation> ExtractObjectRelations(OrganizationEntityModel organization)
        {
            var objectRelations = new List<ObjectRelation>();

            if (organization.Members != null)
            {
                List<ObjectRelation> relations = organization.Members
                                                             .Select(
                                                                 member => new ObjectRelation(
                                                                     AssignmentType.ChildrenToParent,
                                                                     new KeyProperties(member.ExternalIds.FirstOrDefault()?.Id, string.Empty),
                                                                     member.Id,
                                                                     member.Kind.ToObjectType()))
                                                             .ToList();

                objectRelations.AddRange(relations);
            }

            if (organization.MemberOf != null)
            {
                List<ObjectRelation> relations = organization.MemberOf
                                                             .Select(
                                                                 member => new ObjectRelation(
                                                                     AssignmentType.ParentsToChild,
                                                                     new KeyProperties(member.ExternalIds.FirstOrDefault()?.Id, string.Empty),
                                                                     member.Id,
                                                                     member.Kind.ToObjectType()))
                                                             .ToList();

                objectRelations.AddRange(relations);
            }

            return objectRelations;
        }
    }
}
