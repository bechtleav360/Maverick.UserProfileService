using System.Threading.Tasks;
using MassTransit;
using Microsoft.Extensions.Logging;
using UserProfileService.Commands;
using UserProfileService.EventCollector.Abstractions.Messages.Responses;
using UserProfileService.Sync.Messages.Commands;
using UserProfileService.Sync.States.Messages;

namespace UserProfileService.Sync.Messages
{
    /// <summary>
    ///     Implementation of <see cref="FaultMessageConsumerBase"/> to consume fault messages.
    /// </summary>
    public class SyncFaultMessageConsumer : FaultMessageConsumerBase, IConsumer<Fault<StartSyncCommand>>, IConsumer<Fault<SetNextStepMessage>>, IConsumer<Fault<SkipStepMessage>>,
        IConsumer<Fault<AddedRelationSyncMessage>>, IConsumer<Fault<CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>>>,
        IConsumer<Fault<CollectingItemsStatus>>, IConsumer<Fault<DeletedRelationSyncMessage>>, IConsumer<Fault<FinalizeSyncMessage>>,
        IConsumer<Fault<WaitingForResponseMessage>>, IConsumer<Fault<UpdateProcessMessage>>, IConsumer<Fault<GetSyncStatus>>, IConsumer<Fault<GetSyncScheduleCommand>>,
        IConsumer<Fault<FunctionSyncMessage>>,IConsumer<Fault<GroupSyncMessage>>, IConsumer<Fault<OrganizationSyncMessage>>, IConsumer<Fault<RoleSyncMessage>>,IConsumer<Fault<UserSyncMessage>>,
        IConsumer<Fault<SetSyncScheduleCommand>>


    {
        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<StartSyncCommand>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SetNextStepMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SkipStepMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<AddedRelationSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<CollectingItemsResponse<SubmitCommandSuccess, SubmitCommandFailure>>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<CollectingItemsStatus>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<DeletedRelationSyncMessage>> context)
        {
           await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FinalizeSyncMessage>> context)
        {
           await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<WaitingForResponseMessage>> context)
        {
           await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<UpdateProcessMessage>> context)
        {
           await Consume(context.Message);
        }

        /// <inheritdoc />
        public SyncFaultMessageConsumer(ILogger<SyncFaultMessageConsumer> logger) : base(logger)
        {
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<GetSyncStatus>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<GetSyncScheduleCommand>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<FunctionSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<GroupSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<OrganizationSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<RoleSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<UserSyncMessage>> context)
        {
            await Consume(context.Message);
        }

        /// <inheritdoc />
        public async Task Consume(ConsumeContext<Fault<SetSyncScheduleCommand>> context)
        {
            await Consume(context.Message);
        }
    }
}
