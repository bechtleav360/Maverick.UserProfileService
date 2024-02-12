using System;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.DependencyInjection;
using UserProfileService.Projection.SecondLevel.Assignments.UnitTests.Helpers;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Assignments.UnitTests
{
    public class ServiceCollectionExtensionTests
    {
        private static IServiceProvider GetServiceProvider()
        {
            var repoMock = new Mock<ISecondLevelAssignmentRepository>();

            IServiceCollection serviceCollection = new ServiceCollection()
                .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
                .AddAutoMapper(typeof(ServiceCollectionExtensionTests).Assembly)
                .AddSingleton(repoMock.Object)
                .AddDefaultMockStreamNameResolver();

            // Add handler to test, if they were created
            serviceCollection.AddAssignmentProjectionService(second => second.AddSecondLevelEventHandlers());

            return serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_user_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<UserCreated>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_properties_changed_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<PropertiesChanged>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_was_assigned_to_function_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<WasAssignedToFunction>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_was_assigned_to_role_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<WasAssignedToRole>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_was_assigned_to_organization_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<WasAssignedToOrganization>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_was_assigned_to_group_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<WasAssignedToGroup>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_was_unassigned_from_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<WasUnassignedFrom>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_container_deleted_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<ContainerDeleted>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_entity_deleted_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<ISecondLevelAssignmentEventHandler<EntityDeleted>>();

            // assert
            Assert.NotNull(handler);
        }
    }
}
