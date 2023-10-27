using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Extensions;
using UserProfileService.Projection.SecondLevel.Assignments.Abstractions;
using UserProfileService.Projection.SecondLevel.Handlers;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;

namespace UserProfileService.Projection.SecondLevel.Assignments.Handler;

/// <summary>
///     Handles <see cref="FunctionChangedHandler" /> regarding assignments.
/// </summary>
public class FunctionChangedHandler : SecondLevelAssignmentEventHandlerBase<FunctionChanged>
{
    /// <summary>
    ///     Initializes a new instance of an <see cref="FunctionChangedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper">The mapper that is used to map object to other objects.</param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="logger">The logger to be used.</param>
    public FunctionChangedHandler(
        ISecondLevelAssignmentRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        ILogger<FunctionChangedEventHandler> logger) :
        base(repository, mapper, streamNameResolver, logger)
    {
    }

    private void UpdateFunctionName(
        Function updatedFunction,
        SecondLevelAssignmentFunction secondLevelAssignmentFunction,
        ObjectIdent relatedEntityId)
    {
        Logger.EnterMethod();

        if (updatedFunction.Role == null || updatedFunction.Organization == null)
        {
            Logger.LogWarnMessage(
                "Unable to resolve all dependencies for function {functionId} role {roleId} organization {organizationId} for relates user {profile}",
                LogHelpers.Arguments(
                    updatedFunction.Id,
                    updatedFunction.RoleId,
                    updatedFunction?.OrganizationId,
                    relatedEntityId.ToLogString()));

            return;
        }

        secondLevelAssignmentFunction.Name = updatedFunction.GenerateFunctionName();
    }

    /// <inheritdoc />
    protected override async Task HandleEventAsync(
        FunctionChanged domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (relatedEntityIdent.Type != ObjectType.User)
        {
            Logger.LogInfoMessage(
                "Ignoring assignment condition triggered event for object {objectType} as it is no user",
                LogHelpers.Arguments(relatedEntityIdent.Type));

            Logger.ExitMethod();

            return;
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        await ExecuteInsideTransactionAsync(
            (repo, transaction, ct)
                => HandleInternalAsync(
                    repo,
                    transaction,
                    relatedEntityIdent,
                    domainEvent.Function.Id,
                    domainEvent.Function,
                    ct),
            eventHeader,
            cancellationToken);

        Logger.ExitMethod();
    }

    protected async Task HandleInternalAsync(
        ISecondLevelAssignmentRepository repo,
        IDatabaseTransaction transaction,
        ObjectIdent relatedEntityId,
        string changedEntityId,
        Function functionChanged,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        Logger.LogInfoMessage(
            "Updating name of {changedEntityId} for assignment user {userId}",
            LogHelpers.Arguments(changedEntityId, relatedEntityId.Id));

        cancellationToken.ThrowIfCancellationRequested();

        SecondLevelProjectionAssignmentsUser currentState =
            await repo.GetAssignmentUserAsync(relatedEntityId.Id, transaction, cancellationToken);

        // Get all container for function (role, organization)
        ISecondLevelAssignmentContainer functionContainer =
            currentState.Containers.FirstOrDefault(c => c.Id == changedEntityId);

        ISecondLevelAssignmentContainer roleContainer =
            currentState.Containers.FirstOrDefault(c => c.Id == functionChanged.RoleId);

        ISecondLevelAssignmentContainer organizationContainer =
            currentState.Containers.FirstOrDefault(c => c.Id == functionChanged.OrganizationId);

        // change role and organization
        if (roleContainer != null)
        {
            roleContainer.Name = functionChanged.Role.Name;
        }

        if (organizationContainer != null)
        {
            organizationContainer.Name = functionChanged.Organization.Name;
        }

        if (functionContainer == null)
        {
            Logger.LogWarnMessage(
                "Unable to update the name of the specified container {containerId} in assignments of {userId}",
                LogHelpers.Arguments(changedEntityId, relatedEntityId.Id));

            Logger.ExitMethod();

            return;
        }

        if (functionContainer.ContainerType != ContainerType.Function)
        {
            Logger.LogWarnMessage(
                "Wrong container type for {changedEntityId}. It should be a function. But container type has the value: {containerType}",
                LogHelpers.Arguments(changedEntityId.ToLogString(), functionContainer.ContainerType.ToLogString()));

            Logger.ExitMethod();

            return;
        }

        SecondLevelAssignmentFunction function =
            currentState.Containers
                .Where(c => c is SecondLevelAssignmentFunction)
                .Cast<SecondLevelAssignmentFunction>()
                .First(c => c.Id == changedEntityId);

        UpdateFunctionName(functionChanged, function, relatedEntityId);

        cancellationToken.ThrowIfCancellationRequested();
        Logger.LogDebugMessage("Storing the new assignment user.", LogHelpers.Arguments());
        await repo.SaveAssignmentUserAsync(currentState, transaction, cancellationToken);

        Logger.ExitMethod();
    }
}
