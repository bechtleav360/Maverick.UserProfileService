using System;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Microsoft.Extensions.DependencyInjection;
using UserProfileService.Projection.Common.UnitTests.Helpers;
using UserProfileService.Projection.Common.UnitTests.TestArguments;
using UserProfileService.Projection.Common.Utilities;
using Xunit;

namespace UserProfileService.Projection.Common.UnitTests
{
    public class MapperTests
    {
        private static IMapper GetMapper()
        {
            IServiceCollection services = new ServiceCollection()
                .AddAutoMapper(typeof(MappingProfiles).Assembly);

            return services.BuildServiceProvider().GetRequiredService<IMapper>();
        }

        [Theory]
        [MemberData(
            nameof(MapperTestArguments.GetCreateEventTestArguments),
            MemberType = typeof(MapperTestArguments))]
        public void Map_create_event_to_model_should_work(
            IUserProfileServiceEvent eventToConvert,
            Type entityTypeToBeCreated)
        {
            // arrange
            IMapper mapper = GetMapper();

            // act
            object model = mapper.Map(
                eventToConvert,
                eventToConvert.GetType(),
                entityTypeToBeCreated);

            // assert
            AssertionHelpers.AssertModelIsSimilarToCreatedEvent(model, eventToConvert);
        }
    }
}
