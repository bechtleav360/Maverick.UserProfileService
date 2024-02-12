using System;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Common.Utilities;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.UnitTests.Mocks;
using UserProfileService.Projection.FirstLevel.Utilities;

namespace UserProfileService.Projection.FirstLevel.UnitTests.Utilities
{
    public class FirstLevelHandlerTestsPreparationHelper
    {
        public static IServiceProvider Provider;

        public static IServiceProvider GetWithDefaultTestSetup(Action<IServiceCollection> additionalSetup)
        {
            Mock<IFirstLevelEventTupleCreator> tupleCreator =
                MockProvider.GetDefaultMock<IFirstLevelEventTupleCreator>();

            Mock<IStreamNameResolver> streamNameResolver = MockProvider.GetDefaultMock<IStreamNameResolver>();

            IServiceCollection services = new ServiceCollection()
                                          .AddAutoMapper(
                                              typeof(FirstLevelProjectionMapper).Assembly,
                                              typeof(MappingProfiles).Assembly)
                                          .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
                                          .AddSingleton(streamNameResolver.Object)
                                          .AddSingleton(tupleCreator.Object)
                                          .AddSingleton<ILoggerFactory>(new NullLoggerFactory())
                                          .AddLogging(c => c.AddDebug());

            additionalSetup?.Invoke(services);

            Provider = services.BuildServiceProvider();

            return Provider;
        }

        public static IMapper GetMapper()
        {
            var mappingConfig = new MapperConfiguration(mc => { mc.AddProfile<FirstLevelProjectionMapper>(); });

            return mappingConfig.CreateMapper();
        }

        public static IFirstLevelEventTupleCreator GetFirstLevelEventTupleCreator()
        {
            GetWithDefaultTestSetup(s => { });

            return Provider.GetRequiredService<IFirstLevelEventTupleCreator>();
        }

        public static IStreamNameResolver GetFirstLevelNameResolver()
        {
            GetWithDefaultTestSetup(s => { });

            return Provider.GetRequiredService<IStreamNameResolver>();
        }
    }
}
