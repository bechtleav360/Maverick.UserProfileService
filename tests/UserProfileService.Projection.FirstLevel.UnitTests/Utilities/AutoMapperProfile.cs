using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Utilities
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<EventMetaData, EventMetaData>();
        }
    }
}
