using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Common.Tests.Utilities.Extensions;
using UserProfileService.Common.Tests.Utilities.Utilities;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;
using UserProfileService.Projection.SecondLevel.Assignments.Tests.Helpers;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests
{
    public class MainSecondLevelEventHandlerTests
    {
        private static IServiceProvider GetServiceProvider(Action<IServiceCollection> additionalAction = null)
        {
            var repoMock = new Mock<ISecondLevelAssignmentRepository>();

            IServiceCollection serviceCollection = new ServiceCollection()
                .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
                .AddAutoMapper(typeof(ServiceCollectionExtensionTests).Assembly)
                .AddSingleton(repoMock.Object)
                .AddDefaultMockStreamNameResolver();

            // Add handler to test, if they were created
            serviceCollection.AddAssignmentProjectionService(_ => { });

            additionalAction?.Invoke(serviceCollection);

            return serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task Event_handler_should_throw_on_null_event()
        {
            IServiceProvider services = GetServiceProvider();
            var sut = ActivatorUtilities.CreateInstance<MainSecondLevelAssignmentEventHandler>(services);

            await Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.HandleEventAsync(null, new StreamedEventHeader(), CancellationToken.None));
        }

        [Fact]
        public async Task Event_handler_should_throw_on_missing_event_id()
        {
            IServiceProvider services = GetServiceProvider();
            var sut = ActivatorUtilities.CreateInstance<MainSecondLevelAssignmentEventHandler>(services);

            var domainEvent = new Mock<IUserProfileServiceEvent>();
            domainEvent.SetupProperty(e => e.EventId, string.Empty);

            await Assert.ThrowsAsync<InvalidDomainEventException>(
                () => sut.HandleEventAsync(domainEvent.Object, new StreamedEventHeader(), CancellationToken.None));
        }

        [Fact]
        public async Task Event_handler_should_throw_on_missing_event_type()
        {
            IServiceProvider services = GetServiceProvider();
            var sut = ActivatorUtilities.CreateInstance<MainSecondLevelAssignmentEventHandler>(services);

            var domainEvent = new Mock<IUserProfileServiceEvent>();
            domainEvent.SetupAllProperties();
            domainEvent.SetupProperty(e => e.EventId, Guid.NewGuid().ToString("D"));

            await Assert.ThrowsAsync<InvalidDomainEventException>(
                () => sut.HandleEventAsync(domainEvent.Object, new StreamedEventHeader(), CancellationToken.None));
        }

        [Fact]
        public async Task Event_handler_should_invoke_valid_handler()
        {
            var ct = new CancellationToken();

            PropertiesChanged domainEvent = new PropertiesChanged
            {
                EventId = Guid.NewGuid().ToString("D"),
                Id = Guid.NewGuid().ToString("D"),
                Properties = new Dictionary<string, object>()
            }.AddDefaultMetadata(obj:null);

            var mockHandler = new Mock<ISecondLevelAssignmentEventHandler<PropertiesChanged>>();

            IServiceProvider services = GetServiceProvider(s => s.AddSingleton(mockHandler.Object));
            var sut = ActivatorUtilities.CreateInstance<MainSecondLevelAssignmentEventHandler>(services);

            await sut.HandleEventAsync(domainEvent, new StreamedEventHeader(), ct);

             mockHandler.Verify(
                h => h.HandleEventAsync(
                    ItShould.BeEquivalentTo(domainEvent, "The method will not be used."),
                    It.IsAny<StreamedEventHeader>(),
                    ItShould.BeEquivalentTo(ct)),
                Times.Never);
        }
    }
}
