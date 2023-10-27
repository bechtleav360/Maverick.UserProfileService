using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using ExternalIdentifierApi = Maverick.UserProfileService.Models.Models.ExternalIdentifier;
using ExternalIdentifierResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;
using TagResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.Tag;
using TagApi = Maverick.UserProfileService.Models.RequestModels.Tag;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.Common.Utilities;

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
                            => model.Name = created.GenerateFunctionName()))
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

        CreateMap<TagCreated, TagResolved>()
            .ForMember(
                t => t.Type,
                options => options
                    .MapFrom(t => t.TagType));

        CreateMap<TagCreated, TagApi>()
            .ForMember(
                t => t.Type,
                options => options
                    .MapFrom(t => t.TagType));

        CreateMap<TagResolved, TagApi>().ReverseMap();

        CreateMap<RoleBasic, ResolvedModels.Role>().ReverseMap();

        CreateMap<ExternalIdentifierResolved, ExternalIdentifierApi>().ReverseMap();

        CreateMap<UserCreated, ResolvedModels.User>().ReverseMap();
        CreateMap<GroupCreated, ResolvedModels.Group>().ReverseMap();
        CreateMap<OrganizationCreated, ResolvedModels.Organization>().ReverseMap();

        CreateMap<FunctionCreated, ResolvedModels.Function>()
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

        CreateMap<RoleCreated, ResolvedModels.Role>().ReverseMap();

        CreateMap<SecondLevelProjectionUser, ResolvedModels.User>().ReverseMap();
        CreateMap<SecondLevelProjectionGroup, ResolvedModels.Group>().ReverseMap();
        CreateMap<SecondLevelProjectionOrganization, ResolvedModels.Organization>().ReverseMap();
        CreateMap<SecondLevelProjectionRole, ResolvedModels.Role>().ReverseMap();
        CreateMap<SecondLevelProjectionFunction, ResolvedModels.Function>().ReverseMap();

        CreateMap<UserCreated, SecondLevelProjectionUser>().ReverseMap();
        CreateMap<GroupCreated, SecondLevelProjectionGroup>().ReverseMap();
        CreateMap<OrganizationCreated, SecondLevelProjectionOrganization>().ReverseMap();

        CreateMap<RoleCreated, SecondLevelProjectionRole>().ReverseMap();

        CreateMap<FunctionCreated, SecondLevelProjectionFunction>()
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

        CreateMap<SecondLevelProjectionFunction, LinkedFunctionObject>()
            .ForMember(
                f => f.Name,
                options
                    => options.MapFrom(
                        (function, model)
                            => model.Name = function.GenerateFunctionName()))
            .ReverseMap();

        // is needed, otherwise the automapper is mapping REFERENCES
        // this can cause bad SIDE-EFFECTS
        CreateMap<EventMetaData, EventMetaData>();
    }
}
