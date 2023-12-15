using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.EventSourcing.Abstractions;
using UserProfileService.EventSourcing.Abstractions.Models;
using UserProfileService.Informer.Abstraction;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.SecondLevel.Abstractions;
using Group = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Group;
using Organization = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.Organization;
using User = Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models.User;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     Processes an <see cref="PropertiesChanged" /> event.
/// </summary>
internal class PropertiesChangedEventHandler : SecondLevelEventHandlerBase<PropertiesChanged>
{
    private readonly IServiceProvider _provider;

    /// <summary>
    ///     Initializes a new instance of an <see cref="PropertiesChangedEventHandler" />.
    /// </summary>
    /// <param name="repository">The repository to be used.</param>
    /// <param name="mapper"></param>
    /// <param name="streamNameResolver">The resolver that will convert from or to stream names.</param>
    /// <param name="messageInformer">
    ///     The message informer is used to notify consumer (if present) when
    ///     an event message type appears.
    /// </param>
    /// <param name="logger">the logger to be used.</param>
    /// <param name="provider">
    ///     Used to activate the custom handler, that will handle messages and resolve them dependent on
    ///     property name and entity type.
    /// </param>
    public PropertiesChangedEventHandler(
        ISecondLevelProjectionRepository repository,
        IMapper mapper,
        IStreamNameResolver streamNameResolver,
        // ReSharper disable once SuggestBaseTypeForParameterInConstructor
        IServiceProvider provider,
        IMessageInformer messageInformer, 
        ILogger<PropertiesChangedEventHandler> logger) : base(repository, mapper, streamNameResolver, messageInformer, logger)
    {
        _provider = provider;
    }

    // this method is using a helper class
    private async Task ApplyChangesOnRelationPropertiesAsync(
        PropertiesChanged domainEvent,
        ObjectIdent relatedEntityIdent,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        var relatePropertyHandler =
            ActivatorUtilities.CreateInstance<PropertiesChangedRelatedEntityEventHandler>(
                _provider,
                transaction,
                domainEvent,
                relatedEntityIdent);

        await relatePropertyHandler.HandleMessageAsync(cancellationToken);

        // to see the difference in log
        Logger.LogInfoMessage(
            "Changed model state, because of properties of related items have been changed.",
            LogHelpers.Arguments());

        Logger.ExitMethod();
    }

    private async Task ApplyChangesToObjectAsync(
        PropertiesChanged domainEvent,
        ISecondLevelProjectionRepository repository,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        Logger.EnterMethod();

        object toBeModified = await GetObjectAsync(
            domainEvent.ObjectType,
            domainEvent.Id,
            repository,
            transaction,
            cancellationToken);

        if (toBeModified == null)
        {
            throw new DatabaseException(
                "Did not get any response from database repository.",
                ExceptionSeverity.Error);
        }

        Logger.LogDebugMessage(
            "Current state of {entityTypeName} retrieved (id: {entityId})",
            LogHelpers.Arguments(GetLogName(toBeModified), GetId(toBeModified)));

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Current state (before updating) of {entityTypeName}: {oldEntityState}",
                LogHelpers.Arguments(GetLogName(toBeModified), toBeModified.ToLogString()));
        }

        object converted = GetAggregateFromSecondLevelModel(toBeModified);

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Converted second-level-projection-model to aggregate model (converted object type: {convertedObjectType}).",
                converted.GetType().FullName.AsArgumentList());
        }

        try
        {
            PatchObjectHelpers.ApplyPropertyChanges(
                converted,
                // to be sure, the keys are not compare case-sensitive
                domainEvent.Properties
                    .Where(kv => kv.Key != null) // to be on the safe side
                    .ToDictionary(
                        kv => kv.Key,
                        kv => kv.Value,
                        StringComparer.OrdinalIgnoreCase),
                domainEvent);
        }
        catch (NotValidException notValidException)
        {
            throw new InvalidDomainEventException(
                $"Could not change properties, because of an invalid event: {notValidException.Message}",
                notValidException);
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "{entityTypeName} changed in-memory. New data: {newEntityState}",
                LogHelpers.Arguments(
                    GetLogName(converted),
                    converted.ToLogString()));
        }

        object updatedSecondLevelModel = GetSecondLevelFromAggregateModel(converted);

        Logger.LogDebugMessage(
            "Converted to second level projection model to be persisted in database",
            LogHelpers.Arguments());

        Logger.LogDebugMessage(
            "Storing updated {entityTypeName} in database",
            GetLogName(updatedSecondLevelModel).AsArgumentList());

        await UpdateObjectAsync(
            updatedSecondLevelModel,
            repository,
            transaction,
            domainEvent.MetaData,
            cancellationToken);

        Logger.ExitMethod();
    }

    private object GetSecondLevelFromAggregateModel(object aggregateModel)
    {
        Logger.EnterMethod();

        // won't happen, but to be on the safe side
        if (aggregateModel == null)
        {
            throw new ArgumentNullException(nameof(aggregateModel));
        }

        object converted = aggregateModel switch
        {
            User user => Mapper.Map<SecondLevelProjectionUser>(user),
            Group group => Mapper.Map<SecondLevelProjectionGroup>(group),
            Organization organization => Mapper.Map<SecondLevelProjectionOrganization>(organization),
            Function function => Mapper.Map<SecondLevelProjectionFunction>(function),
            Role role => Mapper.Map<SecondLevelProjectionRole>(role),
            SecondLevelProjectionFunction secondFunction => secondFunction,
            SecondLevelProjectionRole secondRole => secondRole,
            SecondLevelProjectionUser secondUser => secondUser,
            SecondLevelProjectionGroup secondGroup => secondGroup,
            SecondLevelProjectionOrganization secondOrganization => secondOrganization,
            _ => throw new NotSupportedException(
                $"The type of the provided object (type: {aggregateModel.GetType().FullName}) is not supported by this method.")
        };

        return Logger.ExitMethod(converted);
    }

    private object GetAggregateFromSecondLevelModel(object secondLevelProjectionModel)
    {
        Logger.EnterMethod();

        // won't happen, but to be on the safe side
        if (secondLevelProjectionModel == null)
        {
            throw new ArgumentNullException(nameof(secondLevelProjectionModel));
        }

        object converted = secondLevelProjectionModel switch
        {
            User user => user,
            Group group => group,
            Organization organization => organization,
            Function function => function,
            Role role => role,
            SecondLevelProjectionFunction secondFunction => Mapper.Map<Function>(secondFunction),
            SecondLevelProjectionRole secondRole => Mapper.Map<Role>(secondRole),
            SecondLevelProjectionUser secondUser => Mapper.Map<User>(secondUser),
            SecondLevelProjectionGroup secondGroup => Mapper.Map<Group>(secondGroup),
            SecondLevelProjectionOrganization secondOrganization => Mapper.Map<Organization>(secondOrganization),
            _ => throw new NotSupportedException(
                $"The type of the provided object (type: {secondLevelProjectionModel.GetType().FullName}) is not supported by this method.")
        };

        return Logger.ExitMethod(converted);
    }

    private static string GetId(object o)
    {
        return o switch
        {
            IProfile profile => profile.Id,
            ISecondLevelProjectionProfile secondProfile => secondProfile.Id,
            User user => user.Id,
            Group group => group.Id,
            Organization organization => organization.Id,
            FunctionBasic functionBasic => functionBasic.Id,
            Function function => function.Id,
            SecondLevelProjectionFunction secondFunction => secondFunction.Id,
            RoleBasic roleBasic => roleBasic.Id,
            Role role => role.Id,
            SecondLevelProjectionRole secondRole => secondRole.Id,
            _ => string.Empty
        };
    }

    private static string GetLogName(object o)
    {
        switch (o)
        {
            case IProfile _:
            case ISecondLevelProjectionProfile _:
                return "profile";
            case FunctionBasic _:
            case Function _:
            case SecondLevelProjectionFunction _:
                return "function";
            case RoleBasic _:
            case Role _:
            case SecondLevelProjectionRole _:
                return "role";
            default:
                return "object";
        }
    }

    private static async Task<object> GetObjectAsync(
        ObjectType objectType,
        string id,
        ISecondLevelProjectionRepository repository,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        switch (objectType)
        {
            case ObjectType.Role:
                return await repository.GetRoleAsync(id, transaction, cancellationToken);
            case ObjectType.Function:
                return await repository.GetFunctionAsync(id, transaction, cancellationToken);
            case ObjectType.Profile:
            case ObjectType.Group:
            case ObjectType.User:
            case ObjectType.Organization:
                return await repository.GetProfileAsync(id, transaction, cancellationToken);
            case ObjectType.Tag:
            case ObjectType.Unknown:
                throw new NotSupportedException(
                    $"Requested object type '{objectType:G}' is not supported by event handler for 'PropertiesChanged'.");
            default:
                throw new ArgumentOutOfRangeException(nameof(objectType), objectType, null);
        }
    }

    private static Task UpdateObjectAsync(
        object updatedState,
        ISecondLevelProjectionRepository repository,
        IDatabaseTransaction transaction,
        EventMetaData eventMetadata,
        CancellationToken cancellationToken)
    {
        DateTime updateDate = eventMetadata.Timestamp;

        switch (updatedState)
        {
            case SecondLevelProjectionRole role:
                role.UpdatedAt = updateDate;
                return repository.UpdateRoleAsync(role, transaction, cancellationToken);
            case SecondLevelProjectionFunction function:
                function.UpdatedAt = updateDate;
                return repository.UpdateFunctionAsync(function, transaction, cancellationToken);
            case ISecondLevelProjectionProfile profile:
                profile.UpdatedAt = updateDate;
                return repository.UpdateProfileAsync(profile, transaction, cancellationToken);
            default:
                throw new NotSupportedException(
                    $"Updating requested object type '{updatedState.GetType().Name}' is not supported by event handler for 'PropertiesChanged'.");
        }
    }

    /// <inheritdoc />
    /// <exception cref="ArgumentException">
    ///     The dictionary in the domain event is empty or <c>null</c>.<br />-or-<br />
    ///     The object to be changed has no modifiable properties that suit to changed properties set.
    /// </exception>
    /// <exception cref="NotValidException">
    ///     Types of tne new value and one of the properties are not compatible and the patch
    ///     operation could not be done.
    /// </exception>
    /// <exception cref="DatabaseException">An error occurred in database repository</exception>
    protected override async Task HandleEventAsync(
        PropertiesChanged domainEvent,
        StreamedEventHeader eventHeader,
        ObjectIdent relatedEntityIdent,
        CancellationToken cancellationToken = default)
    {
        Logger.EnterMethod();

        if (domainEvent.Properties == null)
        {
            throw new InvalidDomainEventException(
                "Could not change properties: The domain event must contain a dictionary of property changes that is not empty.",
                domainEvent);
        }

        if (Logger.IsEnabledForTrace())
        {
            Logger.LogTraceMessage(
                "Input parameter domainEvent: {domainEvent}",
                LogHelpers.Arguments(domainEvent.ToLogString()));
        }

        if (string.IsNullOrWhiteSpace(domainEvent.Id))
        {
            throw new InvalidDomainEventException(
                "Could not change properties of resource: Resource id is missing.",
                domainEvent);
        }

        // the changes are related to the entity itself (i.e. changing weight on group)
        if (domainEvent.RelatedContext == PropertiesChangedContext.Self || relatedEntityIdent.Id == domainEvent.Id)
        {
            await ExecuteInsideTransactionAsync(
                (repo, t, ct)
                    => ApplyChangesToObjectAsync(domainEvent, repo, t, ct),
                eventHeader,
                cancellationToken);
        }
        else
        {
            await ExecuteInsideTransactionAsync(
                (_, t, ct)
                    => ApplyChangesOnRelationPropertiesAsync(domainEvent, relatedEntityIdent, t, ct),
                eventHeader,
                cancellationToken);
        }

        Logger.LogInfoMessage(
            "{entityTypeName} updated in database (id: {entityId})",
            LogHelpers.Arguments(
                relatedEntityIdent.Type.ToString("G"),
                relatedEntityIdent.Id));

        Logger.ExitMethod();
    }
}
