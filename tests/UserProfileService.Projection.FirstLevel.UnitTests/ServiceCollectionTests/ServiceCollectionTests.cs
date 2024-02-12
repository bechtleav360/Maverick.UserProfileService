using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.DependencyInjection;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Utilities;
using Xunit;
using V3FunctionCreatedEvent = UserProfileService.Events.Implementation.V3.FunctionCreatedEvent;
using V3UserCreatedEvent = UserProfileService.Events.Implementation.V3.UserCreatedEvent;

namespace UserProfileService.Projection.FirstLevel.UnitTests.ServiceCollectionTests
{
    public class ServiceCollectionExtensionTests
    {
        private static IServiceProvider GetServiceProvider()
        {
            // Mock all dependencies for the handler
            var repo = new Mock<IFirstLevelProjectionRepository>();
            var saga = new Mock<ISagaService>();
            var tuple = new Mock<IFirstLevelEventTupleCreator>();
            var streamResolver = new Mock<IStreamNameResolver>();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging();
            serviceCollection.AddSingleton(repo.Object);
            serviceCollection.AddSingleton(saga.Object);
            serviceCollection.AddSingleton(tuple.Object);
            serviceCollection.AddSingleton(streamResolver.Object);
            serviceCollection.AddAutoMapper(typeof(FirstLevelProjectionMapper));

            // Added the handler that should be checked.
            serviceCollection.AddFirstLevelProjectionService(
                b => { b.AddFirstLevelEventHandlers().AddHandlerResolver(); });

            return serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_user_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<UserCreatedEvent>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_organization_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<OrganizationCreatedEvent>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_tag_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<TagCreatedEvent>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_group_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<GroupCreatedEvent>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_role_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<RoleCreatedEvent>>();

            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<FunctionCreatedEvent>>();
            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_v3_created_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<V3FunctionCreatedEvent>>();
            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_client_settings_updated_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileClientSettingsUpdatedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_tags_added_event_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileTagsAddedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_deleted_event_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileDeletedEvent>>();
            // assert ProfileClientSettingsSetEvent
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_clientSettings_set_event_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileClientSettingsSetEvent>>();
            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_client_settings_set_batch_event_handler()
        {
            // arrange
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileClientSettingsSetEvent>>();
            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_client_settings_deleted_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileClientSettingsSetEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_object_assignment_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ObjectAssignmentEvent>>();
            // assert FunctionDeletedEvent
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_deleted_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<FunctionDeletedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_tags_added_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<FunctionTagsAddedEvent>>();
            // assert  
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_properties_changed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<FunctionPropertiesChangedEvent>>();
            // assert ProfilePropertiesChangedEvent
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_properties_changed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfilePropertiesChangedEvent>>();
            // assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_retrieve_function_tags_removed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<FunctionTagsRemovedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }
        
        [Fact]
        public void Register_event_handler_types_and_retrieve_profile_tags_removed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act RoleDeletedEvent
            var handler = services.GetService<IFirstLevelProjectionEventHandler<ProfileTagsRemovedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_role_deleted_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act RoleDeletedEvent
            var handler = services.GetService<IFirstLevelProjectionEventHandler<RoleDeletedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_role_properties_changed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act RoleDeletedEvent
            var handler = services.GetService<IFirstLevelProjectionEventHandler<RolePropertiesChangedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_Role_tags_added_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<RoleTagsAddedEvent>>();
            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_Role_tags_removed_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<RoleTagsRemovedEvent>>();

            // assert 
            Assert.NotNull(handler);
        }

        [Fact]
        public void Register_event_handler_types_and_tag_deleted_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<TagDeletedEvent>>();

            // assert 
            Assert.NotNull(handler);
        }
        
        [Fact]
        public void Register_event_handler_types_and_user_created_v3_event_handler()
        {
            // arrange 
            IServiceProvider services = GetServiceProvider();

            // act 
            var handler = services.GetService<IFirstLevelProjectionEventHandler<V3UserCreatedEvent>>();

            // assert 
            Assert.NotNull(handler);
        }
    }
}
