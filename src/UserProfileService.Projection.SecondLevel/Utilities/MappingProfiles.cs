using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using UserProfileService.Projection.Abstractions;
using ResolvedModels = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using AggregateModels = Maverick.UserProfileService.AggregateEvents.Common.Models;

namespace UserProfileService.Projection.SecondLevel.Utilities;

internal class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<InitiatorType,
                Messaging.Abstractions.Models.InitiatorType>()
            .ReverseMap();

        CreateMap<ISecondLevelProjectionProfile, ResolvedModels.Member>().ReverseMap();
    }
}
