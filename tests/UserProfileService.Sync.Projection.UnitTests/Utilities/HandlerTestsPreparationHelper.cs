using System;
using System.Threading;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Sync.Projection.Abstractions;
using UserProfileService.Sync.Utilities;

namespace UserProfileService.Sync.Projection.UnitTests.Utilities
{
    internal static class HandlerTestsPreparationHelper
    {

        private static ObjectIdent GetIdent(string streamName)
        {
            string[] parts = streamName?.Split("#");

            if (parts?.Length != 2)
            {
                return default;
            }

            return new ObjectIdent(
                parts[1],
                Enum.Parse<ObjectType>(parts[0], true));
        }


        public static string GetRelatedProfileId(string profileIdWithEntityName)
        {
            if (string.IsNullOrWhiteSpace(profileIdWithEntityName))
            {
                throw new ArgumentNullException(nameof(profileIdWithEntityName));
            }

            try
            {
                return profileIdWithEntityName.Split('#')[1];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static IServiceProvider GetWithDefaultTestSetup(Action<IServiceCollection> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection()
                                          .AddAutoMapper(
                                              typeof(MappingProfiles).Assembly)
                                          .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
                                          .AddDefaultMockStreamNameResolver();

            additionalSetup?.Invoke(services);

            return services.BuildServiceProvider();
        }

        /// <summary>
        ///     Adds a stream name resolver mock, that will do nothing.
        /// </summary>
        public static IServiceCollection AddDefaultMockStreamNameResolver(this IServiceCollection services)
        {
            var mock = new Mock<IStreamNameResolver>();

            mock.Setup(m => m.GetStreamName(It.IsAny<ObjectIdent>()))
                .Returns((ObjectIdent ident) => $"{ident.Type:G}#{ident.Id}");

            mock.Setup(m => m.GetObjectIdentUsingStreamName(It.IsAny<string>()))
                .Returns((string s) => GetIdent(s));

            services.TryAddSingleton(mock.Object);

            return services;
        }

        /// <summary>
        ///     Creates a mock of <see cref="IProfileService"/> with basic configuration
        /// </summary>
        /// <returns> returns <see cref="Mock{IProfileService}"/></returns>
        public static Mock<IProfileService> GetProfileServiceMock()
        {
            var mock = new Mock<IProfileService>();

            mock
                .Setup(
                    p => p.TrySaveProjectionStateAsync(
                        It.IsAny<ProjectionState>(),
                        It.IsAny<IDatabaseTransaction>(),
                        It.IsAny<ILogger>(),
                        It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            return mock;
        }
    }
}
