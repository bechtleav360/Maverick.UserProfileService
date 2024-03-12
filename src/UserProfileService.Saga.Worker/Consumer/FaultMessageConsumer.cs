using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.Messaging;
using UserProfileService.Saga.Events.Messages;
using UserProfileService.Validation.Abstractions.Message;

namespace UserProfileService.Saga.Worker.Consumer
{
    /// <summary>
    ///     Implementation of <see cref="FaultMessageConsumerBase"/> to consume fault messages.
    /// </summary>
    public class FaultMessageConsumer : FaultMessageConsumerBase,
        IConsumer<Fault<FunctionCreatedMessage>>, IConsumer<Fault<FunctionDeletedMessage>>, IConsumer<Fault<FunctionTagsAddedMessage>>,
        IConsumer<Fault<FunctionTagsRemovedMessage>>, IConsumer<Fault<GroupCreatedMessage>>, IConsumer<Fault<ObjectAssignmentMessage>>, IConsumer<Fault<OrganizationCreatedMessage>>,
        IConsumer<Fault<ProfileClientSettingsDeletedMessage>>, IConsumer<Fault<ProfileClientSettingsSetBatchMessage>>, IConsumer<Fault<ProfileClientSettingsSetMessage>>,
        IConsumer<Fault<ProfileClientSettingsUpdatedMessage>>, IConsumer<Fault<ProfileDeletedMessage>>, IConsumer<Fault<ProfilePropertiesChangedMessage>>, IConsumer<Fault<ProfileTagsAddedMessage>>,
        IConsumer<Fault<ProfileTagsRemovedMessage>>, IConsumer<Fault<RoleCreatedMessage>>, IConsumer<Fault<RoleDeletedMessage>>, IConsumer<Fault<RolePropertiesChangedMessage>>,
        IConsumer<Fault<RoleTagsAddedMessage>>, IConsumer<Fault<RoleTagsRemovedMessage>>, IConsumer<Fault<TagCreatedMessage>>, IConsumer<Fault<TagDeletedMessage>>, IConsumer<Fault<UserCreatedMessage>>,
        IConsumer<Fault<UserSettingObjectDeletedMessage>>, IConsumer<Fault<UserSettingsSectionCreatedMessage>>,
        IConsumer<Fault<SubmitCommand>>, IConsumer<Fault<ValidateCommand>>, IConsumer<Fault<ValidationCompositeResponse>>, IConsumer<Fault<SubmitCommandSuccess>>,
        IConsumer<Fault<SubmitCommandFailure>>, IConsumer<Fault<SubmitCommandResponseMessage>>, IConsumer<Fault<CommandProjectionFailure>>, IConsumer<Fault<CommandProjectionSuccess>> 
    {
        /// <inheritdoc />
        public FaultMessageConsumer(ILogger<FaultMessageConsumer> logger) : base(logger)
        {
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SubmitCommandResponseMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<CommandProjectionFailure>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<CommandProjectionSuccess>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SubmitCommandFailure>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SubmitCommandSuccess>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ValidationCompositeResponse>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ValidateCommand>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SubmitCommand>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<UserSettingsSectionCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FunctionCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FunctionDeletedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FunctionTagsAddedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FunctionTagsRemovedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<GroupCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ObjectAssignmentMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<OrganizationCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileClientSettingsDeletedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileClientSettingsSetBatchMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileClientSettingsSetMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileClientSettingsUpdatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileDeletedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfilePropertiesChangedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileTagsAddedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<ProfileTagsRemovedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RoleCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RoleDeletedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RolePropertiesChangedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RoleTagsAddedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RoleTagsRemovedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<TagCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<TagDeletedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<UserCreatedMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<UserSettingObjectDeletedMessage>> context)
        {
            await Consume(context.Message);
        }
    }
}
