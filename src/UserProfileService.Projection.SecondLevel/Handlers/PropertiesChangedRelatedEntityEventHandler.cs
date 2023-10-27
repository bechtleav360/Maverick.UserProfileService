using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Common.V2.Utilities;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.Common.Exceptions;
using AggregateEnums = Maverick.UserProfileService.AggregateEvents.Common.Enums;

namespace UserProfileService.Projection.SecondLevel.Handlers;

/// <summary>
///     It will update properties of an entity that are related to the property to be changed, but not this property
///     itself.
/// </summary>
internal sealed class PropertiesChangedRelatedEntityEventHandler
{
    private readonly PropertiesChanged _DomainEvent;
    private readonly ILogger<PropertiesChangedRelatedEntityEventHandler> _Logger;

    private readonly ObjectIdent _RelatedObjectIdent;
    private readonly ISecondLevelProjectionRepository _Repository;
    private readonly IDatabaseTransaction _Transaction;

    public PropertiesChangedRelatedEntityEventHandler(
        ILogger<PropertiesChangedRelatedEntityEventHandler> logger,
        ISecondLevelProjectionRepository repository,
        IDatabaseTransaction transaction,
        PropertiesChanged domainEvent,
        ObjectIdent relatedObjectIdent)
    {
        _Logger = logger;
        _Repository = repository;
        _Transaction = transaction;
        _DomainEvent = domainEvent;
        _RelatedObjectIdent = relatedObjectIdent;
    }

    private async Task UpdateLinkedProfileInsideFunctionOrRoleEntityAsync(
        ObjectIdent entityToModify,
        IDictionary<string, object> changeSet,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        await _Repository.TryUpdateLinkedProfileAsync(
            entityToModify.Id,
            _DomainEvent.Id,
            changeSet,
            _Transaction,
            cancellationToken);

        _Logger.ExitMethod();
    }

    private async Task UpdateMemberInsideContainerEntityAsync(
        ObjectIdent entityToModify,
        IDictionary<string, object> changeSet,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        await _Repository.TryUpdateMemberAsync(
            entityToModify.Id,
            _DomainEvent.Id,
            changeSet,
            _Transaction,
            cancellationToken);

        _Logger.ExitMethod();
    }

    private async Task UpdateMemberOfInsideProfileEntityAsync(
        ObjectIdent entityToModify,
        IDictionary<string, object> changeSet,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        await _Repository.TryUpdateMemberOfAsync(
            entityToModify.Id,
            _DomainEvent.Id,
            changeSet,
            _Transaction,
            cancellationToken);

        _Logger.ExitMethod();
    }

    private async Task UpdateOrganizationInfoInsideFunctionEntityAsync(
        ObjectIdent functionToBeUpdated,
        IDictionary<string, object> changeSet,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        _Logger.LogInfoMessage(
            "Properties of organization shall be changed inside a function. Related entity {entityId} (type: {entityType})",
            LogHelpers.Arguments(
                functionToBeUpdated.Id,
                functionToBeUpdated.Type.ToString("G")));

        // get the organization of the function
        SecondLevelProjectionFunction function;

        try
        {
            function = await _Repository.GetFunctionAsync(
                functionToBeUpdated.Id,
                _Transaction,
                cancellationToken);
        }
        catch (Exception exception)
        {
            throw new ProjectionRepositoryException(
                $"Could not retrieve function with id {functionToBeUpdated.Id}: {exception.Message}",
                exception);
        }

        if (function == null)
        {
            throw new ProjectionRepositoryException($"Could not retrieve function with id {functionToBeUpdated.Id}");
        }

        // update the organization property nested in the function instance
        PatchObjectHelpers.ApplyPropertyChanges(
            function.Organization,
            changeSet.ToDictionary(
                kv => kv.Key,
                kv => kv.Value,
                StringComparer.OrdinalIgnoreCase),
            _DomainEvent);

        // update organization name inside function entity
        await _Repository.UpdateFunctionAsync(
            function,
            _Transaction,
            cancellationToken);

        _Logger.ExitMethod();
    }

    private async Task UpdateRoleInfoInsideFunctionEntityAsync(
        ObjectIdent functionToBeUpdated,
        IDictionary<string, object> changeSet,
        CancellationToken cancellationToken)
    {
        _Logger.EnterMethod();

        _Logger.LogInfoMessage(
            "Properties of role shall be changed inside a function. Related entity {entityId} (type: {entityType})",
            LogHelpers.Arguments(
                functionToBeUpdated.Id,
                functionToBeUpdated.Type.ToString("G")));

        // get the organization of the function
        SecondLevelProjectionFunction function = await _Repository.GetFunctionAsync(
            functionToBeUpdated.Id,
            _Transaction,
            cancellationToken);

        try
        {
            // update the organization property nested in the function instance
            PatchObjectHelpers.ApplyPropertyChanges(
                function.Role,
                changeSet.ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value,
                    StringComparer.OrdinalIgnoreCase),
                _DomainEvent);
        }
        catch (NotValidException notValidException)
        {
            throw new InvalidDomainEventException(
                $"Could not change properties, because of an invalid event: {notValidException.Message}",
                notValidException);
        }

        // update organization name inside function entity
        await _Repository.UpdateFunctionAsync(
            function,
            _Transaction,
            cancellationToken);

        _Logger.ExitMethod();
    }

    public async Task HandleMessageAsync(
        CancellationToken cancellationToken = default)
    {
        _Logger.EnterMethod();

        if (_DomainEvent.Id == _RelatedObjectIdent.Id)
        {
            _Logger.LogDebugMessage(
                "Properties changed refers to the same entity like the input stream. No further actions necessary.",
                LogHelpers.Arguments());

            return;
        }

        // the organization was changed in the function
        if (_RelatedObjectIdent.Type == ObjectType.Function
            && _DomainEvent.RelatedContext == PropertiesChangedContext.Organization)
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            await UpdateOrganizationInfoInsideFunctionEntityAsync(
                _RelatedObjectIdent,
                _DomainEvent.Properties,
                cancellationToken);

            _Logger.ExitMethod();

            return;
        }

        // The role was changed in the function
        if (_RelatedObjectIdent.Type == ObjectType.Function
            && _DomainEvent.RelatedContext == PropertiesChangedContext.Role)
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            await UpdateRoleInfoInsideFunctionEntityAsync(
                _RelatedObjectIdent,
                _DomainEvent.Properties,
                cancellationToken);

            _Logger.ExitMethod();

            return;
        }

        if (_DomainEvent.RelatedContext == PropertiesChangedContext.LinkedProfiles
            && (_DomainEvent.ObjectType == AggregateEnums.ObjectType.User
                || _DomainEvent.ObjectType == AggregateEnums.ObjectType.Group))
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            await UpdateLinkedProfileInsideFunctionOrRoleEntityAsync(
                _RelatedObjectIdent,
                _DomainEvent.Properties,
                cancellationToken);

            _Logger.ExitMethod();

            return;
        }

        if ((_RelatedObjectIdent.Type == ObjectType.Group
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Group)
            || (_RelatedObjectIdent.Type == ObjectType.Organization
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Organization))
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            if (_DomainEvent.RelatedContext == PropertiesChangedContext.Members)
            {
                await UpdateMemberInsideContainerEntityAsync(
                    _RelatedObjectIdent,
                    _DomainEvent.Properties,
                    cancellationToken);
            }

            if (_DomainEvent.RelatedContext == PropertiesChangedContext.MemberOf)
            {
                await UpdateMemberOfInsideProfileEntityAsync(
                    _RelatedObjectIdent,
                    _DomainEvent.Properties,
                    cancellationToken);
            }

            _Logger.ExitMethod();

            return;
        }

        if (_RelatedObjectIdent.Type == ObjectType.Group
            && _DomainEvent.ObjectType == AggregateEnums.ObjectType.User
            && _DomainEvent.RelatedContext == PropertiesChangedContext.Members)
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            await UpdateMemberInsideContainerEntityAsync(
                _RelatedObjectIdent,
                _DomainEvent.Properties,
                cancellationToken);

            _Logger.ExitMethod();

            return;
        }

        if (_RelatedObjectIdent.Type == ObjectType.User
            && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Group
            && _DomainEvent.RelatedContext == PropertiesChangedContext.MemberOf
           )
        {
            _Logger.LogDebugMessage(
                "Further handling must be done, because of relations that shall be modified as well.",
                LogHelpers.Arguments());

            await UpdateMemberOfInsideProfileEntityAsync(
                _RelatedObjectIdent,
                _DomainEvent.Properties,
                cancellationToken);

            _Logger.ExitMethod();

            return;
        }

        if ((_RelatedObjectIdent.Type == ObjectType.Organization
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.User)
            || (_RelatedObjectIdent.Type == ObjectType.Organization
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Group)
            || (_RelatedObjectIdent.Type == ObjectType.Group
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Organization)
           )
        {
            _Logger.LogWarnMessage(
                "Type {memberType} cannot be a member of {parentType}",
                LogHelpers.Arguments(
                    _DomainEvent.ObjectType.ToString("G"),
                    _RelatedObjectIdent.Type.ToString("G")));

            _Logger.ExitMethod();

            return;
        }

        if ((_RelatedObjectIdent.Type == ObjectType.User
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Organization)
            || (_RelatedObjectIdent.Type == ObjectType.Group
                && _DomainEvent.ObjectType == AggregateEnums.ObjectType.Organization)
           )
        {
            _Logger.LogWarnMessage(
                "Type {parentType} cannot be a member-of/parent of {memberType}",
                LogHelpers.Arguments(
                    _DomainEvent.ObjectType.ToString("G"),
                    _RelatedObjectIdent.Type.ToString("G")));

            _Logger.ExitMethod();

            return;
        }

        _Logger.ExitMethod();
    }
}
