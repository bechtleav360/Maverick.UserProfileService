using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Models;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Sync.Abstraction.Contracts;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Extensions;
using UserProfileService.Sync.Models;
using UserProfileService.Sync.Models.Views;
using UserProfileService.Sync.States;
using AutoMapperProfile = AutoMapper.Profile;
using ExternalIdentifier = Maverick.UserProfileService.Models.Models.ExternalIdentifier;
using Member = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Member;
using ProfileKind = Maverick.UserProfileService.AggregateEvents.Common.Enums.ProfileKind;

namespace UserProfileService.Sync.Utilities;

/// <summary>
///     Mapping configuration for sync service.
/// </summary>
public class MappingProfiles : AutoMapperProfile
{
    /// <summary>
    ///     Create an instance of <see cref="MappingProfiles" /> and initialize all mapping profiles for <see cref="IMapper" />
    /// </summary>
    public MappingProfiles()
    {
        #region Group

        CreateMap<Group, GroupSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (group, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(group.ExternalIds)))
            .ForMember(t => t.RelatedObjects, t => t.MapFrom(m => m.ExtractObjectRelations()));

        CreateMap<GroupSync, GroupCreatedMessage>()
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<KeyProperties, ExternalIdentifier>().ReverseMap();

        CreateMap<KeyProperties, Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier>()
            .ReverseMap();

        CreateMap<GroupCreatedPayload, GroupSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (payload, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(payload.ExternalIds)));

        CreateMap<GroupSync, ProfilePropertiesChangedMessage>()
            .ForMember(t => t.ProfileKind, t => t.MapFrom(m => ProfileKind.Group))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.Properties, t => t.MapFrom(m => m.GetTargetPropertiesAsDictionary<GroupBasic>()))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<GroupSync, GroupBasic>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (group, _, _, context) =>
                        context.Mapper.Map<IList<KeyProperties>, IList<ExternalIdentifier>>(group.ExternalIds)))
            .ForMember(t => t.Kind, t => t.MapFrom(m => ProfileKind.Group))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id));

        CreateMap<GroupCreated, GroupBasic>()
            .ForMember(t => t.ExternalIds, t => t.MapFrom(m => m.ExternalIds))
            .ForMember(t => t.Kind, t => t.MapFrom(m => ProfileKind.Group))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id));

        CreateMap<GroupCreated, GroupSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (payload, _, _, context) =>
                        context.Mapper
                            .Map<IList<Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier>,
                                IList<KeyProperties>>(payload.ExternalIds)))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.Tags, t => t.MapFrom(m => GetTags(m.Tags)));

        #endregion

        #region Organization

        CreateMap<Organization, OrganizationSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (organization, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(organization.ExternalIds)))
            .ForMember(t => t.RelatedObjects, t => t.MapFrom(m => m.ExtractObjectRelations()));

        CreateMap<OrganizationSync, OrganizationCreatedMessage>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (organizationSync, _, _, context) =>
                        context.Mapper.Map<IList<KeyProperties>, IList<ExternalIdentifier>>(
                            organizationSync.ExternalIds)))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<OrganizationSync, ProfilePropertiesChangedMessage>()
            .ForMember(t => t.ProfileKind, t => t.MapFrom(m => ProfileKind.Organization))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true))
            .ForMember(
                t => t.Properties,
                t => t.MapFrom(m => m.GetTargetPropertiesAsDictionary<OrganizationBasic>()));

        CreateMap<OrganizationCreated, OrganizationSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(m => m.ExternalIds.Select(x => new KeyProperties(x.Id, x.Source, null, false)).ToList()))
            .ForMember(
                t => t.Tags,
                t => t.MapFrom(
                    m => m.Tags != null
                        ? m.Tags.Where(tg => tg.TagDetails != null)
                            .Select(
                                c => new CalculatedTag
                                {
                                    Id = c.TagDetails.Id,
                                    Name = c.TagDetails.Name
                                })
                            .ToList()
                        : new List<CalculatedTag>()));

        #endregion

        #region Role

        CreateMap<RoleView, RoleSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (roleView, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(roleView.ExternalIds)))
            .ForMember(t => t.RelatedObjects, t => t.MapFrom(m => m.ExtractObjectRelations()))
            .ReverseMap();

        CreateMap<RoleSync, RoleCreatedMessage>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (roleSync, _, _, context) =>
                        context.Mapper.Map<IList<KeyProperties>, IList<ExternalIdentifier>>(roleSync.ExternalIds)))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<RoleCreated, RoleSync>()
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.DeniedPermissions, t => t.MapFrom(m => m.DeniedPermissions))
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    m => m.ExternalIds != null
                        ? m.ExternalIds.Select(x => new KeyProperties(x.Id, x.Source, null, x.IsConverted)).ToList()
                        : new List<KeyProperties>()));

        CreateMap<RoleSync, RolePropertiesChangedMessage>()
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true))
            .ForMember(t => t.Properties, t => t.MapFrom(m => m.GetTargetPropertiesAsDictionary<RoleBasic>()));

        CreateMap<RoleSync, RoleDeletedMessage>()
            .ForMember(r => r.IsSynchronized, t => t.MapFrom(m => true));

        #endregion

        #region User

        CreateMap<User, UserSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (user, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(user.ExternalIds)))
            .ForMember(t => t.RelatedObjects, t => t.MapFrom(m => m.ExtractObjectRelations()));

        CreateMap<UserSync, UserCreatedMessage>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (userSync, _, _, context) =>
                        context.Mapper.Map<IList<KeyProperties>, IList<ExternalIdentifier>>(userSync.ExternalIds)))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<UserCreated, UserBasic>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (userCreated, _, _, context) =>
                        context.Mapper
                            .Map<IList<Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier>,
                                IList<ExternalIdentifier>>(userCreated.ExternalIds)))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.Name, t => t.MapFrom(m => m.Name))
            .ForMember(t => t.FirstName, t => t.MapFrom(m => m.FirstName))
            .ForMember(t => t.LastName, t => t.MapFrom(m => m.LastName))
            .ForMember(t => t.Kind, t => t.MapFrom(m => ProfileKind.User))
            .ForMember(t => t.LastName, t => t.MapFrom(m => m.LastName))
            .ForMember(t => t.Source, t => t.MapFrom(m => m.Source));

        CreateMap<UserSync, ProfilePropertiesChangedMessage>()
            .ForMember(t => t.ProfileKind, t => t.MapFrom(m => ProfileKind.Group))
            .ForMember(t => t.Id, t => t.MapFrom(m => m.Id))
            .ForMember(t => t.Properties, t => t.MapFrom(m => m.GetTargetPropertiesAsDictionary<GroupBasic>()))
            .ForMember(t => t.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<UserCreated, UserSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    m => m.ExternalIds.Select(x => new KeyProperties(x.Id, x.Source, null, x.IsConverted)).ToList()))
            .ForMember(t => t.Domain, t => t.MapFrom(m => m.Domain))
            .ForMember(t => t.Kind, t => t.MapFrom(m => Abstraction.Models.ProfileKind.User))
            .ForMember(t => t.LastName, t => t.MapFrom(m => m.LastName))
            .ForMember(t => t.Name, t => t.MapFrom(m => m.Name));

        #endregion

        #region Functions

        CreateMap<FunctionBasic, FunctionSync>()
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    (fb, _, _, context) =>
                        context.Mapper.Map<IList<ExternalIdentifier>, IList<KeyProperties>>(fb.ExternalIds)))
            .ReverseMap();

        CreateMap<FunctionSync, FunctionDeletedMessage>();

        CreateMap<FunctionCreated, FunctionSync>()
            .ForMember(f => f.Id, t => t.MapFrom(m => m.Id))
            .ForMember(f => f.OrganizationId, t => t.MapFrom(m => m.Organization.Id))
            .ForMember(
                f => f.Name,
                t => t.MapFrom(m => m.GenerateFunctionName()))
            .ForMember(f => f.Source, t => t.MapFrom(m => m.Source))
            .ForMember(f => f.Role, t => t.MapFrom(m => m.Role.Id))
            .ForMember(
                t => t.ExternalIds,
                t => t.MapFrom(
                    m => m.ExternalIds != null
                        ? m.ExternalIds.Select(x => new KeyProperties(x.Id, x.Source, null, x.IsConverted)).ToList()
                        : new List<KeyProperties>()));

        #endregion

        CreateMap<ISyncModel, ProfileDeletedMessage>()
            .ForMember(r => r.IsSynchronized, t => t.MapFrom(m => true))
            .ReverseMap();

        CreateMap<ILookUpObject, ProfileDeletedMessage>()
            .ForMember(t => t.Id, t => t.MapFrom(m => m.MaverickId))
            .ForMember(r => r.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<ILookUpObject, RoleDeletedMessage>()
            .ForMember(t => t.Id, t => t.MapFrom(m => m.MaverickId))
            .ForMember(r => r.IsSynchronized, t => t.MapFrom(m => true));

        CreateMap<ProcessState, ProcessView>()
            .ForMember(d => d.CorrelationId, m => m.MapFrom(v => v.CorrelationId))
            .ForMember(d => d.Initiator, m => m.MapFrom(v => v.Initiator))
            .ForMember(d => d.Status, m => m.MapFrom(v => v.Process.Status))
            .ForMember(d => d.FinishedAt, m => m.MapFrom(v => v.Process.FinishedAt))
            .ForMember(d => d.LastActivity, m => m.MapFrom(v => v.Process.UpdatedAt))
            .ForMember(d => d.StartedAt, m => m.MapFrom(v => v.Process.StartedAt))
            .ForMember(
                d => d.SyncOperations,
                m => m.MapFrom(v => GenerateOperations(v)))
            .ReverseMap();

        CreateMap<ProcessState, ProcessDetail>()
            .ForMember(d => d.Initiator, m => m.MapFrom(v => v.Initiator))
            .ForMember(d => d.Status, m => m.MapFrom(v => v.Process.Status))
            .ForMember(d => d.FinishedAt, m => m.MapFrom(v => v.Process.FinishedAt))
            .ForMember(d => d.UpdatedAt, m => m.MapFrom(v => v.Process.UpdatedAt))
            .ForMember(d => d.StartedAt, m => m.MapFrom(v => v.Process.StartedAt))
            .ForMember(d => d.Systems, m => m.MapFrom(v => GetSystemViews(v.Process.Systems)))
            .ReverseMap();

        CreateMap<Member, ObjectRelation>()
            .ForMember(d => d.MaverickId, m => m.MapFrom(o => o.Id))
            .ForMember(d => d.Conditions, m => m.MapFrom(o => o.Conditions))
            .ForMember(d => d.ObjectType, m => m.MapFrom(o => ConvertProfileKindToObjectType(o.Kind)))
            .ForMember(d => d.ExternalId, m => m.MapFrom(o => o.GetKeyProperties()));
    }

    private static ObjectType ConvertProfileKindToObjectType(ProfileKind profileKind)
    {
        return profileKind switch
        {
            ProfileKind.Group => ObjectType.Group,
            ProfileKind.Organization => ObjectType.Organization,
            ProfileKind.User => ObjectType.User,
            ProfileKind.Unknown => ObjectType.Unknown,
            _ => ObjectType.Unknown
        };
    }

    private static IDictionary<string, SystemView> GetSystemViews(IDictionary<string, Models.State.System> systems)
    {
        if (systems == null)
        {
            return null;
        }

        if (systems.Count == 0)
        {
            return new Dictionary<string, SystemView>();
        }

        return systems.ToDictionary(
            keyValuePair => keyValuePair.Key,
            keyValuePair => new SystemView
            {
                IsCompleted = keyValuePair.Value.IsCompleted,
                Steps = keyValuePair.Value.Steps.Select(
                        s => new KeyValuePair<string, StepView>(
                            s.Key,
                            new StepView
                            {
                                Final = s.Value?.Final,
                                Handled = s.Value?.Handled,
                                Temporary = s.Value?.Temporary,
                                Status = s.Value?.Status
                            }))
                    .ToDictionary(o => o.Key, o => o.Value)
            });
    }

    private Operations GenerateOperations(ProcessState state)
    {
        var operations = new Operations
        {
            Groups = state.Process.GetHandledNumberOfStep(SyncConstants.SagaStep.GroupStep),
            Users = state.Process.GetHandledNumberOfStep(SyncConstants.SagaStep.UserStep),
            Organizations =
                state.Process.GetHandledNumberOfStep(SyncConstants.SagaStep.OrgUnitStep),
            Roles = state.Process.GetHandledNumberOfStep(SyncConstants.SagaStep.RoleStep)
        };

        return operations;
    }

    private List<CalculatedTag> GetTags(TagAssignment[] tags)
    {
        if (tags == null || !tags.Any())
        {
            return new List<CalculatedTag>();
        }

        return tags.Where(t => t.TagDetails != null)
            .Select(
                t => new CalculatedTag
                {
                    Id = t.TagDetails.Id,
                    Name = t.TagDetails.Name
                })
            .ToList();
    }
}
