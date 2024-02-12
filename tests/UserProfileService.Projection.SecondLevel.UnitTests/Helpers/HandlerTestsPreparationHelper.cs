using System;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Informer.Abstraction;
using MappingProfiles = UserProfileService.Projection.Common.Utilities.MappingProfiles;

namespace UserProfileService.Projection.SecondLevel.UnitTests.Helpers;

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
            .AddAutoMapper(
                typeof(MappingProfiles).Assembly,
                typeof(AggregateToApiModelMappingProfile).Assembly)
            .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
            .AddDefaultMockStreamNameResolver()
            .AddDefaultMockMessageInformerResolver();

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

    public static IServiceCollection AddDefaultMockMessageInformerResolver(this IServiceCollection services)
    {
        var mock = new Mock<IMessageInformer>();

        mock.Setup(m => m.NotifyEventOccurredAsync(It.IsAny<IUserProfileServiceEvent>(), It.IsAny<INotifyContext>()))
            .Returns(() => Task.CompletedTask);

        services.TryAddSingleton(mock.Object);

        return services;
    }
}