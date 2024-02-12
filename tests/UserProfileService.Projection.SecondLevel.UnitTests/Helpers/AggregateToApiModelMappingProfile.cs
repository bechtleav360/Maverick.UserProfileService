using AutoMapper;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Projection.Abstractions;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

public class AggregateToApiModelMappingProfile : Profile
{
    public AggregateToApiModelMappingProfile()
    {
        CreateMap<ResolvedModels.Group, GroupBasic>().ReverseMap();
        CreateMap<ResolvedModels.Organization, OrganizationBasic>().ReverseMap();
        CreateMap<ResolvedModels.Role, RoleBasic>().ReverseMap();
        CreateMap<ResolvedModels.Function, FunctionBasic>().ReverseMap();

        CreateMap<AggregateModels.ExternalIdentifier, ExternalIdentifier>()
            .ReverseMap();

        CreateMap<ISecondLevelProjectionProfile, ResolvedModels.Member>().ReverseMap();
    }
}