using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Abstractions;
using UserProfileService.Projection.FirstLevel.Abstractions;
using FunctionCreatedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.FunctionCreated;
using ResolvedInitiator = Maverick.UserProfileService.AggregateEvents.Common.EventInitiator;
using FunctionCreatedEventV3 = UserProfileService.Events.Implementation.V3.FunctionCreatedEvent;
using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using UserProfileService.Validation.Abstractions.Configuration;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Extensions;

namespace UserProfileService.Projection.FirstLevel.Handler.V3;

/// <summary>
///     This handler is used to process <see cref="FunctionCreatedEventV3" />.
/// </summary>
internal class FunctionCreatedFirstLevelEventHandler : FirstLevelEventHandlerTagsIncludedBase<FunctionCreatedEventV3>
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _propertyLocks = new();
    private readonly ValidationConfiguration _validationConfiguration;

    /// <summary>
    ///     Creates an instance of the object <see cref="FunctionCreatedFirstLevelEventHandler" />.
    /// </summary>
    /// <param name="mapper">The Mapper is used to map several existing events in new event with the right order.</param>
    /// <param name="logger">
    ///     The logger factory that is used to create a logger. The logger logs message for debugging
    ///     and control reasons.
    /// </param>
    /// <param name="repository">
    ///     The read service is used to read from the internal query storage to get all information to
    ///     generate all needed stream events.
    /// </param>
    /// <param name="validationConfiguration"> <see cref="ValidationConfiguration"/></param>
    /// <param name="sagaService">
    ///     The saga service is used to write all created <see cref="IUserProfileServiceEvent" /> to the
    ///     write stream.
    /// </param>
    /// <param name="creator">The creator is used to create <inheritdoc cref="EventTuple" /> from the given parameter.</param>
    public FunctionCreatedFirstLevelEventHandler(
        ILogger<FunctionCreatedFirstLevelEventHandler> logger,
        IFirstLevelProjectionRepository repository,
        ISagaService sagaService,
        IFirstLevelEventTupleCreator creator,
        IOptions<ValidationConfiguration> validationConfiguration,
        IMapper mapper) : base(
        logger,
        repository,
        sagaService,
        mapper,
        creator)
    {
        _validationConfiguration = validationConfiguration.Value;
    }

    protected override async Task HandleInternalAsync(
        FunctionCreatedEventV3 eventObject,
        StreamedEventHeader streamEvent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "@event: {event}.",
                LogHelpers.Arguments(eventObject.ToLogString()));
        }

        var function = Mapper.Map<FirstLevelProjectionFunction>(eventObject);

        function.Organization = await Repository.GetProfileAsync<FirstLevelProjectionOrganization>(
            eventObject.Payload.OrganizationId,
            transaction,
            cancellationToken);

        function.Role = await Repository.GetRoleAsync(
            eventObject.Payload.RoleId,
            transaction,
            cancellationToken);

        var functionCreatedResolvedEvent = Mapper.Map<FunctionCreatedResolvedEvent>(function);

        bool duplicateFunctionAllowed = _validationConfiguration.Internal.Function.DuplicateAllowed;

        string organizationId = function.Organization?.Id;
        string organizationExternalId = function.Organization?.ExternalIds.FirstOrDefaultUnconverted()?.Id;
        string roleExternalId = function.Role?.ExternalIds.FirstOrDefaultUnconverted()?.Id;
        string roleId = function.Role?.Id;
        string semaphoreKey = $"{organizationId}{roleId}";

        Logger.LogDebugMessage(
            "Trying to create function with roleId: {rId} and organizationId: {oId}",
            LogHelpers.Arguments(roleId, organizationId));

        SemaphoreSlim semaphore = null;

        if (!duplicateFunctionAllowed)
        {
            semaphore = _propertyLocks.GetOrAdd(
                semaphoreKey,
                _ => new SemaphoreSlim(1, 1));

            await semaphore.WaitAsync(cancellationToken);

            Logger.LogDebugMessage("Entered semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));

        }

        try
        {
            bool functionExist = await Repository.FunctionExistAsync(
               roleId,
               organizationId,
                roleExternalId,
               organizationExternalId,
                cancellationToken);

            if (functionExist && !duplicateFunctionAllowed)
            {
                Logger.LogWarnMessage(
                    "The function with the OrganizationId: {oId} and roleId: {rId} already exist and can not be created",
                    LogHelpers.Arguments(
                        organizationId,
                       roleId));

                throw new AlreadyExistsException(
                    $"The function with following properties: organizationId (external): {organizationExternalId}, organizationId: {organizationId}, roleId (external): {roleExternalId}, roleId:{roleId} already exist and can not be created");
            }
            else if (functionExist)
            {
                Logger.LogWarnMessage(
                    "The function with following properties: organizationId (external): {organizationExternalId}, organizationId: {organizationId}, roleId (external): {roleExternalId}, roleId:{roleId} already exist but will created again, DUPLICATED FUNCTIONS are allowed ",
                    LogHelpers.Arguments(
                        organizationExternalId,
                        organizationId,
                        roleExternalId,
                        roleId));
            }

            Guid batchSagaId = await SagaService.CreateBatchAsync(
                cancellationToken,
                Creator.CreateEvents(
                        new ObjectIdent(eventObject.Payload.Id, ObjectType.Function),
                        new List<IUserProfileServiceEvent>
                        {
                            functionCreatedResolvedEvent
                        },
                        eventObject)
                    .ToArray());

            await Repository.CreateFunctionAsync(function, transaction, cancellationToken);

            await CreateTagsAddedEvent(
                transaction,
                batchSagaId,
                eventObject.Payload.Tags.ToList(),
                async (repo, tag) =>
                    await repo.AddTagToFunctionAsync(
                        tag,
                        eventObject.Payload.Id,
                        transaction,
                        cancellationToken),
                eventObject,
                new ObjectIdent(eventObject.Payload.Id, ObjectType.Group),
                cancellationToken);

            await SagaService.ExecuteBatchAsync(batchSagaId, cancellationToken);

        }
        finally
        {
            if (!duplicateFunctionAllowed)
            {
                semaphore?.Release();
                Logger.LogDebugMessage("Left semaphore for key: {property}", LogHelpers.Arguments(semaphoreKey));
                _propertyLocks.Remove(semaphoreKey, out semaphore);
            }
        }

      
        Logger.ExitMethod();
    }
}
