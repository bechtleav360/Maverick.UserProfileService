using System;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Common.Utilities;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Helpers
{
    public static class HandlerTestsPreparationHelper
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

        public static IServiceProvider GetWithDefaultTestSetup(Action<IServiceCollection> additionalSetup)
        {
            IServiceCollection services = new ServiceCollection()
                .AddAutoMapper(typeof(MappingProfiles).Assembly)
                .AddAutoMapper(typeof(Utilities.MappingProfiles).Assembly)
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
    }
}
