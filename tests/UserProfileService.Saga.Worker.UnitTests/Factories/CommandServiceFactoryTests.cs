using System;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using UserProfileService.Saga.Common;
using UserProfileService.Saga.Events.Contracts;
using UserProfileService.Saga.Validation.Abstractions;
using UserProfileService.StateMachine.Abstraction;
using UserProfileService.StateMachine.Factories;
using UserProfileService.StateMachine.Implementations;
using UserProfileService.StateMachine.Services;
using Xunit;

namespace UserProfileService.Saga.Worker.UnitTests.Factories
{
    public class CommandServiceFactoryTests
    {
        [Theory]
        [InlineData(CommandConstants.FunctionCreate, typeof(FunctionCreatedMessageService))]
        [InlineData(CommandConstants.FunctionDelete, typeof(FunctionDeletedMessageService))]
        [InlineData(CommandConstants.FunctionTagsAdded, typeof(FunctionTagsAddedMessageService))]
        [InlineData(CommandConstants.FunctionTagsRemoved, typeof(FunctionTagsRemovedMessageService))]
        [InlineData(CommandConstants.GroupCreate, typeof(GroupCreatedMessageService))]
        [InlineData(CommandConstants.ObjectAssignment, typeof(ObjectAssignmentMessageService))]
        [InlineData(CommandConstants.OrganizationCreate, typeof(OrganizationCreatedMessageService))]
        [InlineData(CommandConstants.ProfileClientSettingsDeleted, typeof(ProfileClientSettingsDeletedMessageService))]
        [InlineData(CommandConstants.ProfileClientSettingsSetBatch, typeof(ProfileClientSettingsSetBatchMessageService))]
        [InlineData(CommandConstants.ProfileClientSettingsSet, typeof(ProfileClientSettingsSetMessageService))]
        [InlineData(CommandConstants.ProfileClientSettingsUpdated, typeof(ProfileClientSettingsUpdatedMessageService))]
        [InlineData(CommandConstants.ProfileDelete, typeof(ProfileDeletedMessageService))]
        [InlineData(CommandConstants.ProfileChange, typeof(ProfilePropertiesChangedMessageService))]
        [InlineData(CommandConstants.ProfileTagsAdded,typeof(ProfileTagsAddedMessageService))]
        [InlineData(CommandConstants.ProfileTagsRemoved,typeof(ProfileTagsRemovedMessageService))]
        [InlineData(CommandConstants.RoleCreate,typeof(RoleCreatedMessageService))]
        [InlineData(CommandConstants.RoleDelete,typeof(RoleDeletedMessageService))]
        [InlineData(CommandConstants.RoleChange,typeof(RolePropertiesChangedMessageService))]
        [InlineData(CommandConstants.RoleTagsAdded,typeof(RoleTagsAddedMessageService))]
        [InlineData(CommandConstants.RoleTagsRemoved,typeof(RoleTagsRemovedMessageService))]
        [InlineData(CommandConstants.TagCreated,typeof(TagCreatedMessageService))]
        [InlineData(CommandConstants.TagDeleted,typeof(TagDeletedMessageService))]
        [InlineData(CommandConstants.UserCreate,typeof(UserCreatedMessageService))]
        [InlineData(CommandConstants.UserSettingObjectDeleted,typeof(UserSettingObjectDeletedMessageService))]
        [InlineData(CommandConstants.UserSettingObjectUpdated,typeof(UserSettingObjectUpdatedMessageService))]
        [InlineData(CommandConstants.UserSettingSectionDeleted,typeof(UserSettingSectionDeletedMessageService))]
        [InlineData(CommandConstants.UserSettingSectionCreated,typeof(UserSettingsSectionCreatedMessageService))]
        public void CreateCommandServiceSuccess(string command, Type type)
        {
            
            ServiceProvider serviceProvider = new ServiceCollection()
                .AddSingleton(new Mock<IProjectionReadService>().Object)
                .AddSingleton(new Mock<IValidationService>().Object)
                .AddSingleton<ISagaCommandFactory,DefaultSagaCommandFactory>()
                .AddLogging()
                .BuildServiceProvider();

            var factory = new CommandServiceFactory(serviceProvider);

            ICommandService service = factory.CreateCommandService(command);

            Assert.IsType(type, service);
        }
    }
}
