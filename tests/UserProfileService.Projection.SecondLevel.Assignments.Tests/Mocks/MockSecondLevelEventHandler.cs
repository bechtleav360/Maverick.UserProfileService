using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;

namespace UserProfileService.Projection.SecondLevel.Assignments.Tests.Mocks
{
    public class MockSecondLevelEventHandler : SecondLevelAssignmentEventHandlerBase<MockUpsEvent>
    {
        /// <inheritdoc />
        public MockSecondLevelEventHandler(
            ISecondLevelAssignmentRepository repository,
            IMapper mapper,
            IStreamNameResolver streamNameResolver,
            ILogger<MockSecondLevelEventHandler> logger) : base(repository, mapper, streamNameResolver, logger)
        {
        }

        /// <inheritdoc />
        protected override Task HandleEventAsync(
            MockUpsEvent domainEvent,
            StreamedEventHeader eventHeader,
            ObjectIdent relatedEntityIdent,
            CancellationToken cancellationToken = default)
        {
            throw new NotValidException("Test");
        }
    }
}
