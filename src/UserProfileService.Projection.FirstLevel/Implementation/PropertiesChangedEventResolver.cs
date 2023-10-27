using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1.Models;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Microsoft.Extensions.Logging;
using UserProfileService.Common;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.EnumModels;
using UserProfileService.Projection.Abstractions.Models;
using UserProfileService.Projection.FirstLevel.Abstractions;
using UserProfileService.Projection.FirstLevel.Extensions;
using UserProfileService.Projection.FirstLevel.Utilities;
using PropertiesChangedResolvedEvent = Maverick.UserProfileService.AggregateEvents.Resolved.V1.PropertiesChanged;

namespace UserProfileService.Projection.FirstLevel.Implementation;

/// <summary>
///     The resolver is used to create <see cref="EventTuple" /> related to entities that has
///     been changed and the needed events to updated the related entities that are affected by the changes.
/// </summary>
public class PropertiesChangedRelatedEventsResolver : IPropertiesChangedRelatedEventsResolver
{
    private readonly IFirstLevelEventTupleCreator _Creator;
    private readonly ILogger<ProfilePropertiesChangedEvent> _Logger;
    private readonly IMapper _Mapper;
    private readonly IFirstLevelProjectionRepository _Repository;

    /// <summary>
    ///     Creates an instance <see cref="PropertiesChangedRelatedEventsResolver" />.
    /// </summary>
    /// <param name="logger">The logger that is used for logging purposes.</param>
    /// <param name="mapper">The mapper transforms one model to another.</param>
    /// <param name="creator">The creator creates out of an event an <see cref="EventTuple" />.</param>
    /// <param name="repository">The repository is used to get and save object for the first level projection. </param>
    public PropertiesChangedRelatedEventsResolver(
        ILogger<ProfilePropertiesChangedEvent> logger,
        IMapper mapper,
        IFirstLevelEventTupleCreator creator,
        IFirstLevelProjectionRepository repository)
    {
        _Logger = logger;
        _Mapper = mapper;
        _Creator = creator;
        _Repository = repository;
    }

    /// <summary>
    ///     This method is used to run a method in a safe manner.
    ///     It has execution method and a compensate method that should be
    ///     executed, when the other method fails. Exception throwing can be
    ///     switch on/off.
    ///     The default value of the result item can be set.
    /// </summary>
    /// <typeparam name="TResult">The type of the resulting object of <paramref name="methodToExecute" /> and this method.</typeparam>
    /// <param name="methodToExecute">The method that should be executed in a safe manner.</param>
    /// <param name="defaultValue">If no exception should be thrown, but one has been caught, this value will be returned.</param>
    /// <param name="methodToCompensate">The method that should be executed, when the first method fails (optional).</param>
    /// <param name="throwException">Switch exception on/off.</param>
    /// <returns>A task that represent the asynchronous operation. It wraps the result item.</returns>
    /// <exception cref="ArgumentNullException">
    ///     This exception is thrown when the parameter <paramref name="methodToExecute" />
    ///     os null.
    /// </exception>
    private async Task<TResult> ExecuteSafelyAsync<TResult>(
        Func<Task<TResult>> methodToExecute,
        TResult defaultValue = default,
        Func<Task> methodToCompensate = null,
        bool throwException = false)
    {
        if (methodToExecute == null)
        {
            throw new ArgumentNullException(nameof(methodToExecute));
        }

        try
        {
            return await methodToExecute();
        }
        catch (Exception)
        {
            if (methodToCompensate != null)
            {
                await methodToCompensate();
            }

            if (throwException)
            {
                throw;
            }

            return defaultValue;
        }
    }

    /// <inheritdoc />
    public EventTuple CreateRelatedMemberEvent(
        ObjectIdent referenceEntity,
        ObjectIdent relatedEntity,
        PropertiesChangedRelation relationToChangedObject,
        ProfilePropertiesChangedEvent originalEvent)
    {
        _Logger.EnterMethod();

        if (referenceEntity == null)
        {
            throw new ArgumentNullException(nameof(referenceEntity));
        }

        if (relatedEntity == null)
        {
            throw new ArgumentNullException(nameof(relatedEntity));
        }

        if (originalEvent == null)
        {
            throw new ArgumentNullException(nameof(originalEvent));
        }

        if (relationToChangedObject == PropertiesChangedRelation.IndirectMember)
        {
            var indirectMemberPropertiesChanged = new PropertiesChangedResolvedEvent
            {
                Id = referenceEntity.Id,
                Properties = originalEvent.Payload.Properties,
                RelatedContext = PropertiesChangedContext.IndirectMember
            };

            return _Creator.CreateEvent(relatedEntity, indirectMemberPropertiesChanged, originalEvent);
        }

        _Logger.ExitMethod();

        return referenceEntity.Type switch
        {
            ObjectType.Group =>
                PropertiesChangedMembersEventHelper.HandleGroupAsReference(
                    referenceEntity.Id,
                    relatedEntity,
                    relationToChangedObject,
                    originalEvent,
                    _Creator),
            ObjectType.Organization =>
                PropertiesChangedMembersEventHelper.HandleOrganizationAsReference(
                    referenceEntity.Id,
                    relatedEntity,
                    relationToChangedObject,
                    originalEvent,
                    _Creator),
            ObjectType.User =>
                PropertiesChangedMembersEventHelper.HandleUserAsReference(
                    referenceEntity.Id,
                    relatedEntity,
                    originalEvent,
                    _Creator),
            _ => throw new NotSupportedException(
                $"Only groups, organization and user are accepted as type. The type {referenceEntity.Type} is not supported.")
        };
    }

    /// <inheritdoc />
    public async Task<List<EventTuple>> CreateFunctionPropertiesChangedEventsAsync(
        string functionId,
        FunctionPropertiesChangedEvent functionPropertiesChanged,
        PropertiesChangedContext context,
        IDatabaseTransaction transaction,
        CancellationToken cancellationToken)
    {
        if (functionPropertiesChanged == null)
        {
            throw new ArgumentNullException(nameof(functionPropertiesChanged));
        }

        if (string.IsNullOrWhiteSpace(functionId))
        {
            throw new ArgumentException(nameof(functionId));
        }

        if (transaction == null)
        {
            throw new ArgumentNullException(nameof(transaction));
        }

        var propertiesChangedEvent = _Mapper.Map<PropertiesChangedResolvedEvent>(functionPropertiesChanged);
        propertiesChangedEvent.RelatedContext = context;

        FirstLevelProjectionFunction function = await _Repository.GetFunctionAsync(
            functionId,
            transaction,
            cancellationToken);

        var eventTupleList = new List<EventTuple>();

        switch (context)
        {
            case PropertiesChangedContext.Role:
                function.Role.UpdateRoleWithPayload(
                    functionPropertiesChanged.Payload,
                    functionPropertiesChanged,
                    _Logger);

                break;
            case PropertiesChangedContext.Organization:
                function.Organization.UpdateProfileWithPayload(
                    functionPropertiesChanged.Payload,
                    functionPropertiesChanged,
                    _Logger);

                break;
            case PropertiesChangedContext.Self:
                function.UpdateFunctionWithPayload(
                    functionPropertiesChanged.Payload,
                    functionPropertiesChanged,
                    _Logger);

                break;

            default:
                throw new NotSupportedException(
                    $"Only Role, Organization or Self is possible as {nameof(context)}. The value for  {nameof(context)} is {context}");
        }

        eventTupleList.AddRange(
            _Creator.CreateEvents(
                function.ToObjectIdent(),
                new List<IUserProfileServiceEvent>
                {
                    propertiesChangedEvent
                },
                functionPropertiesChanged));

        IList<FirstLevelRelationProfile> relatedProfiles =
            await ExecuteSafelyAsync(
                () => _Repository.GetAllChildrenAsync(
                    function.ToObjectIdent(),
                    transaction,
                    cancellationToken),
                new List<FirstLevelRelationProfile>());

        foreach (FirstLevelRelationProfile relatedProfile in relatedProfiles)
        {
            eventTupleList.AddRange(
                _Creator.CreateEvents(
                    relatedProfile.Profile.ToObjectIdent(),
                    new[]
                    {
                        relatedProfile.Relation == FirstLevelMemberRelation.DirectMember
                            ? new FunctionChanged
                            {
                                Context = PropertiesChangedContext.SecurityAssignments,
                                Function = _Mapper.Map<Function>(function),
                                MetaData = new EventMetaData()
                            }
                            : new FunctionChanged
                            {
                                Context = PropertiesChangedContext.IndirectMember,
                                Function = _Mapper.Map<Function>(function),
                                MetaData = new EventMetaData()
                            }
                    },
                    functionPropertiesChanged));
        }

        await _Repository.UpdateFunctionAsync(function, transaction, cancellationToken);

        return eventTupleList;
    }
}
