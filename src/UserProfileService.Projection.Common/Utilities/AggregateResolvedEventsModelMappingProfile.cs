using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.BasicModels;
using ExternalIdentifierInApi = Maverick.UserProfileService.Models.Models.ExternalIdentifier;
using ExternalIdentifierResolved = Maverick.UserProfileService.AggregateEvents.Common.Models.ExternalIdentifier;

namespace UserProfileService.Projection.Common.Utilities;

/// <summary>
///     Contains mapping configuration of model classes of the aggregate resolved events project to their equivalent model
///     of UPS api
///     and repositories.
/// </summary>
public class AggregateResolvedEventsModelMappingProfile : Profile
{
    /// <summary>
    ///     The default constructor that generates the maps of this profile.
    /// </summary>
    public AggregateResolvedEventsModelMappingProfile()
    {
        CreateMap<Group, GroupBasic>().ReverseMap();
        CreateMap<Organization, OrganizationBasic>().ReverseMap();
        CreateMap<Role, RoleBasic>().ReverseMap();
        CreateMap<Function, FunctionBasic>().ReverseMap();
        CreateMap<ExternalIdentifierResolved, ExternalIdentifierInApi>().ReverseMap();
    }
}
