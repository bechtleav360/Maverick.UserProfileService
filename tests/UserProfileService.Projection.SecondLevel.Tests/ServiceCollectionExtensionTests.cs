using System;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Abstractions;
using UserProfileService.Projection.SecondLevel.DependencyInjection;
using UserProfileService.Projection.SecondLevel.Tests.Helpers;
using Xunit;

namespace UserProfileService.Projection.SecondLevel.Tests;

public class ServiceCollectionExtensionTests
{
    private static IServiceProvider GetServiceProvider()
    {
        var repoMock = new Mock<ISecondLevelProjectionRepository>();

        IServiceCollection serviceCollection = new ServiceCollection()
            .AddLogging(b => b.AddSimpleLogMessageCheckLogger())
            .AddAutoMapper(typeof(ServiceCollectionExtensionTests).Assembly)
            .AddSingleton(repoMock.Object)
            .AddDefaultMockStreamNameResolver()
            .AddDefaultMockMessageInformerResolver();

        // Add handler to test, if they were created
        serviceCollection.AddSecondLevelProjectionService(second => second.AddSecondLevelEventHandlers());

        return serviceCollection.BuildServiceProvider();
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_user_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<UserCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_group_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<GroupCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_function_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<FunctionCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_organization_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<OrganizationCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_role_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<RoleCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_tag_created_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<TagCreated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_properties_changed_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<PropertiesChanged>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_was_assigned_to_function_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<WasAssignedToFunction>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_was_assigned_to_role_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<WasAssignedToRole>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_was_assigned_to_organization_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<WasAssignedToOrganization>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_was_assigned_to_group_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<WasAssignedToOrganization>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_was_unassigned_from_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<WasUnassignedFrom>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_profile_client_settings_set_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<ProfileClientSettingsSet>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_profile_client_settings_unset_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<ProfileClientSettingsUnset>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_profile_client_settings_calculated_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<ClientSettingsCalculated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_profile_client_settings_invalidated_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<ClientSettingsInvalidated>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_container_deleted_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<ContainerDeleted>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_entity_deleted_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<EntityDeleted>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_member_added_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<MemberAdded>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_member_deleted_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<MemberDeleted>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_member_removed_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<MemberRemoved>>();

        // assert
        Assert.NotNull(handler);
    }
    
    [Fact]
    public void Register_event_handler_types_and_retrieve_tags_added_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<TagsAdded>>();

        // assert
        Assert.NotNull(handler);
    }

    [Fact]
    public void Register_event_handler_types_and_retrieve_tags_removed_handler()
    {
        // arrange
        IServiceProvider services = GetServiceProvider();

        // act
        var handler = services.GetService<ISecondLevelEventHandler<TagsRemoved>>();

        // assert
        Assert.NotNull(handler);
    }
}