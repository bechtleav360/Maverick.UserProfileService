using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using UserProfileService.Common.V2.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using ExternalIdentifierResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using ExternalIdentifierApi = Maverick.UserProfileService.Models.Models.ExternalIdentifier;
using TagResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.Tag;
using TagAssignmentsResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.TagAssignment;
using OrganizationCreatedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.OrganizationCreated;
using GroupResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using FunctionResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Function;
using RoleResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Role;
using OrganizationResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using V3FunctionCreatedPayload = UserProfileService.Events.Payloads.V3.FunctionCreatedPayload;
using V3UserCreatedPayload = UserProfileService.Events.Payloads.V3.UserCreatedPayload;
using ResolvedRangeCondition = Maverick.UserProfileService.AggregateEvents.Common.Models.RangeCondition;
using ResolvedEventInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using MemberResolved = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;

namespace UserProfileService.Projection.FirstLevel.Utilities;

internal class FirstLevelProjectionMapper : Profile
{
    /// <summary>
    ///     Creates an instance of the object <see cref="FirstLevelProjectionMapper" />.
    /// </summary>
    public FirstLevelProjectionMapper()
    {
        #region GeneralMappings

        // map same model for resolved and normal object
        CreateMap<EventInitiator, ResolvedEventInitiator>().ReverseMap();
        CreateMap<ExternalIdentifierResolved, ExternalIdentifierApi>().ReverseMap();
        CreateMap<RangeCondition, ResolvedRangeCondition>().ReverseMap();
        CreateMap<IFirstLevelProjectionProfile, MemberResolved>().ReverseMap();
        CreateMap<TagResolved, FirstLevelProjectionTag>().ReverseMap();
        CreateMap<FirstLevelProjectionTagAssignment, TagAssignmentsResolved>();
        CreateMap<IList<FirstLevelProjectionTagAssignment>, List<TagAssignmentsResolved>>();

        CreateMap<FirstLevelProjectionTagAssignment, TagAssignmentsResolved>()
            .ForMember(
                tagAss => tagAss.TagDetails,
                opt => opt.MapFrom(
                    src => new TagResolved
                           {
                               Name = string.Empty,
                               Id = src.TagId
                           })).ReverseMap();
        
        CreateMap<OrganizationCreatedResolvedEvent, OrganizationCreatedPayload>()
            .ReverseMap();

        CreateMap<FunctionCreated, V3FunctionCreatedPayload>()
            .ReverseMap();

        CreateMap<FunctionCreatedPayload, V3FunctionCreatedPayload>()
            .ForMember(t => t.OrganizationId, t => t.MapFrom(m => m.TagFilters.FirstOrDefault()));

        CreateMap<FunctionCreatedEvent, Events.Implementation.V3.FunctionCreatedEvent>()
            .ForMember(t => t.VersionInformation, t => t.Ignore());

        CreateMap<TagAssignmentsResolved, TagAssignment>().ReverseMap();

        #endregion

        #region CreatedEventHandler

        #region OrganizationCreatedEventHandler

        CreateMap<OrganizationCreatedPayload, OrganizationCreatedResolvedEvent>().ReverseMap();
        CreateMap<OrganizationBasic, FirstLevelProjectionOrganization>().ReverseMap();
        CreateMap<OrganizationResolved, FirstLevelProjectionOrganization>().ReverseMap();
        CreateMap<OrganizationResolved, OrganizationCreatedPayload>().ReverseMap();

        CreateMap<OrganizationCreatedResolvedEvent, FirstLevelProjectionOrganization>().ReverseMap();

        CreateMap<OrganizationCreatedEvent, FirstLevelProjectionOrganization>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        CreateMap<OrganizationCreatedEvent, OrganizationCreatedResolvedEvent>()
            .ForMember(gr => gr.CreatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .ForMember(gr => gr.UpdatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .IncludeMembers(gr => gr.Payload)
            .ReverseMap();

        CreateMap<OrganizationCreatedResolvedEvent, OrganizationCreatedPayload>().ReverseMap();
        CreateMap<OrganizationCreatedPayload, FirstLevelProjectionOrganization>().ReverseMap();

        #endregion

        #region RoleCreatedEventHandler

        CreateMap<RoleCreatedPayload, RoleCreated>().ReverseMap();

        CreateMap<RoleCreatedEvent, FirstLevelProjectionRole>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        CreateMap<RoleCreatedPayload, FirstLevelProjectionRole>().ReverseMap();
        CreateMap<RoleBasic, FirstLevelProjectionRole>();
        CreateMap<FirstLevelProjectionRole, RoleResolved>();

        #endregion

        #region FunctionCreatedEventHandler

        // V2
        CreateMap<FunctionCreatedPayload, FunctionCreated>().ReverseMap();

        CreateMap<FirstLevelProjectionFunction, FunctionResolved>().ReverseMap();

        CreateMap<FunctionCreatedPayload, V3FunctionCreatedPayload>()
            .ForMember(t => t.OrganizationId, t => t.MapFrom(m => m.TagFilters.FirstOrDefault()));

        CreateMap<FunctionCreatedEvent, Events.Implementation.V3.FunctionCreatedEvent>()
            .ForMember(t => t.VersionInformation, t => t.Ignore());

        CreateMap<FirstLevelProjectionFunction, Events.Implementation.V3.FunctionCreatedEvent>()
            .ForMember(t => t.VersionInformation, t => t.Ignore());

        CreateMap<FirstLevelProjectionFunction, FunctionCreatedPayload>()
            .ForMember(t => t.TagFilters, t => t.MapFrom(m => new[] { m.Organization.Id }));

        CreateMap<FunctionCreatedPayload, FirstLevelProjectionFunction>()
            .ForMember(
                t => t.Organization,
                t => t.MapFrom(
                    m => new FirstLevelProjectionOrganization
                    {
                        Id = m.TagFilters.First()
                    }));

        // V3
        CreateMap<FirstLevelProjectionFunction, V3FunctionCreatedPayload>().ReverseMap();

        CreateMap<V3FunctionCreatedPayload, FunctionCreated>().ReverseMap();

        CreateMap<FunctionCreated, FirstLevelProjectionFunction>().ReverseMap();

        CreateMap<FunctionCreated, V3FunctionCreatedPayload>()
            .ReverseMap();

        CreateMap<Events.Implementation.V3.FunctionCreatedEvent, FirstLevelProjectionFunction>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        CreateMap<FunctionView, FirstLevelProjectionFunction>()
            .ForMember(x => x.Organization, m => m.MapFrom(y => y.Organization))
            .ForMember(x => x.Role, m => m.MapFrom(y => y.Role));

        #endregion

        #region GroupCreatedEventHandler

        CreateMap<GroupCreatedPayload, GroupCreated>().ReverseMap();

        CreateMap<OrganizationResolved, FirstLevelProjectionGroup>().ReverseMap();

        CreateMap<GroupCreatedEvent, FirstLevelProjectionGroup>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        CreateMap<GroupCreatedEvent, GroupCreated>()
            .ForMember(gr => gr.CreatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .ForMember(gr => gr.UpdatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .IncludeMembers(gr => gr.Payload);

        CreateMap<OrganizationCreatedEvent, OrganizationResolved>()
            .ForMember(gr => gr.CreatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .ForMember(gr => gr.UpdatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .IncludeMembers(gr => gr.Payload);

        // map payloads
        CreateMap<UserCreatedPayload, FirstLevelProjectionUser>().ReverseMap();
        CreateMap<GroupCreatedPayload, FirstLevelProjectionGroup>().ReverseMap();
        CreateMap<FirstLevelProjectionGroup, GroupResolved>().ReverseMap();
        CreateMap<GroupCreatedPayload, GroupResolved>();
        CreateMap<GroupCreated, FirstLevelProjectionGroup>().ReverseMap();
        CreateMap<GroupCreatedPayload, GroupCreated>().ReverseMap();

        #endregion

        #region UserCreatedEventHandler

        // V2
        CreateMap<UserCreatedPayload, UserCreated>().ReverseMap();
        CreateMap<UserBasic, FirstLevelProjectionUser>().ReverseMap();
        CreateMap<UserCreatedPayload, FirstLevelProjectionUser>().ReverseMap();

        CreateMap<UserCreatedEvent, FirstLevelProjectionUser>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        CreateMap<UserCreatedEvent, UserCreated>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload)
            .ReverseMap();

        // V3
        CreateMap<FirstLevelProjectionUser, V3UserCreatedPayload>().ReverseMap();

        CreateMap<UserCreated, V3UserCreatedPayload>()
            .ReverseMap();

        CreateMap<UserCreatedPayload, V3UserCreatedPayload>().ReverseMap();

        CreateMap<UserCreatedEvent, Events.Implementation.V3.UserCreatedEvent>()
            .ForMember(t => t.VersionInformation, t => t.Ignore());

        CreateMap<Events.Implementation.V3.UserCreatedEvent, UserCreated>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload)
            .ReverseMap();

        CreateMap<Events.Implementation.V3.UserCreatedEvent, FirstLevelProjectionUser>()
            .ForMember(x => x.CreatedAt, x => x.MapFrom(o => o.Timestamp))
            .ForMember(x => x.UpdatedAt, x => x.MapFrom(o => o.Timestamp))
            .IncludeMembers(x => x.Payload);

        #endregion

        #region TagCreatedEventHandler

        CreateMap<TagCreatedPayload, TagCreated>()
            .ForMember(
                t => t.TagType,
                options => options
                    .MapFrom(t => t.Type));

        CreateMap<TagType, Maverick.UserProfileService.Models.EnumModels.TagType>().ReverseMap();
        CreateMap<TagCreatedPayload, FirstLevelProjectionTag>().ReverseMap();

        #endregion

        #endregion

        #region DeletedEventHandler

        #region RoleDeletedEventHandler

        CreateMap<RoleDeletedEvent, FirstLevelProjectionRole>()
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        CreateMap<RoleDeletedEvent, EntityDeleted>().ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        CreateMap<FirstLevelProjectionRole, IdentifierPayload>().ReverseMap();

        CreateMap<RoleCreatedEvent, RoleCreated>()
            .ForMember(gr => gr.CreatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .ForMember(gr => gr.UpdatedAt, gr => gr.MapFrom(o => o.Timestamp))
            .IncludeMembers(gr => gr.Payload);

        #endregion

        #region FunctionDeletedEventHandler

        CreateMap<FunctionDeletedEvent, EntityDeleted>()
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        CreateMap<FunctionDeletedEvent, ContainerDeleted>()
            .ForMember(x => x.ContainerId, m => m.MapFrom(y => y.Payload.Id))
            .ForMember(x => x.ContainerType, m => m.MapFrom(y => ContainerType.Function));

        CreateMap<FirstLevelProjectionFunction, IdentifierPayload>().ReverseMap();

        #endregion

        #region ProfileDeletedEventHandler

        CreateMap<ProfileDeletedEvent, EntityDeleted>()
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        CreateMap<ProfileDeletedEvent, ContainerDeleted>()
            .ForMember(x => x.ContainerId, m => m.MapFrom(y => y.Payload.Id))
            .ForMember(x => x.ContainerType, m => m.MapFrom(y => y.Payload.ProfileKind.ToObjectType()));

        CreateMap<IFirstLevelProjectionProfile, ProfileIdentifierPayload>();

        #endregion

        #endregion

        #region PropertiesChangedEventHandler

        #region FunctionPropertiesChangedEventHandler

        CreateMap<FunctionPropertiesChangedEvent, PropertiesChanged>()
            .ForMember(x => x.ObjectType, m => m.MapFrom(y => ObjectType.Function))
            .ForMember(x => x.Properties, m => m.MapFrom(y => y.Payload.Properties))
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        #endregion

        #region RolePropertiesChangedEventHandler

        CreateMap<RolePropertiesChangedEvent, PropertiesChanged>()
            .ForMember(x => x.ObjectType, m => m.MapFrom(y => ObjectType.Role))
            .ForMember(x => x.Properties, m => m.MapFrom(y => y.Payload.Properties))
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        #endregion

        #region ProfilePropertiesChangedHandler

        CreateMap<ProfilePropertiesChangedEvent, PropertiesChanged>()
            .ForMember(x => x.ObjectType, m => m.MapFrom(y => y.ProfileKind.ToObjectType()))
            .ForMember(x => x.Properties, m => m.MapFrom(y => y.Payload.Properties))
            .ForMember(x => x.Id, m => m.MapFrom(y => y.Payload.Id));

        CreateMap<FirstLevelProjectionUser, UserBasic>();
        CreateMap<FirstLevelProjectionGroup, GroupBasic>();
        CreateMap<FirstLevelProjectionOrganization, OrganizationBasic>();

        #endregion

        #endregion

        #region ObjectAssingmentEventHandler

        CreateMap<FirstLevelProjectionTreeEdgeRelation, WasAssignedToGroup>()
            .ForMember(
                dest => dest.ProfileId,
                opt =>
                    opt.MapFrom(src => src.Child.Id))
            .ForMember(
                dest => dest.Conditions,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<ResolvedRangeCondition[]>(entity.Conditions)))
            .ForMember(
                dest => dest.Target,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<GroupResolved>((FirstLevelProjectionGroup)entity.Parent)));

        CreateMap<FirstLevelProjectionTreeEdgeRelation, WasAssignedToOrganization>()
            .ForMember(
                dest => dest.ProfileId,
                opt =>
                    opt.MapFrom(src => src.Child.Id))
            .ForMember(
                dest => dest.Conditions,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<ResolvedRangeCondition[]>(entity.Conditions)))
            .ForMember(
                dest => dest.Target,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<OrganizationResolved>((FirstLevelProjectionOrganization)entity.Parent)));

        CreateMap<FirstLevelProjectionTreeEdgeRelation, WasAssignedToRole>()
            .ForMember(
                dest => dest.ProfileId,
                opt =>
                    opt.MapFrom(src => src.Child.Id))
            .ForMember(
                dest => dest.Conditions,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<ResolvedRangeCondition[]>(entity.Conditions)))
            .ForMember(
                dest => dest.Target,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<RoleResolved>((FirstLevelProjectionRole)entity.Parent)));

        CreateMap<FirstLevelProjectionTreeEdgeRelation, WasAssignedToFunction>()
            .ForMember(
                dest => dest.ProfileId,
                opt =>
                    opt.MapFrom(src => src.Child.Id))
            .ForMember(
                dest => dest.Conditions,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<ResolvedRangeCondition[]>(entity.Conditions)))
            .ForMember(
                dest => dest.Target,
                expression =>
                    expression.MapFrom(
                        (entity, grp, _, context) =>
                            context.Mapper.Map<FunctionResolved>((FirstLevelProjectionFunction)entity.Parent)));

        #endregion
    }
}
