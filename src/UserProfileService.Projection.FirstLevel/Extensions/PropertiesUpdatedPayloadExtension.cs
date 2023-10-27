using System;
using System.Linq;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Common;
using UserProfileService.Events.Implementation.V2;
using UserProfileService.Events.Payloads.V2;
using UserProfileService.EventSourcing.Abstractions.Models;

namespace UserProfileService.Projection.FirstLevel.Extensions;

/// <summary>
///     Extensions that are used for <see cref="PropertiesUpdatedPayload" />.
/// </summary>
internal static class PropertiesUpdatedPayloadExtension
{
    /// <summary>
    ///     Checks if a member has to be updated.
    /// </summary>
    /// <param name="propertiesChangedEvent">Is needed to check if the changed properties are related  to an members changed.</param>
    /// <returns>True if the members have to be updated, otherwise false.</returns>
    /// <exception cref="ArgumentException">
    ///     Is thrown if the <paramref name="propertiesChangedEvent" /> is null the changed
    ///     object is empty.
    /// </exception>
    internal static bool MembersHasToBeUpdated(this PropertiesUpdatedPayload propertiesChangedEvent)
    {
        if (propertiesChangedEvent?.Properties == null
            || propertiesChangedEvent.Properties.Count == 0)
        {
            throw new ArgumentException();
        }

        string[] changedProperties = propertiesChangedEvent.Properties.Keys.ToArray();

        string[] propertiesMembersHasToChanged =
        {
            nameof(Member.Id), nameof(Member.Name), nameof(Member.DisplayName), nameof(Member.ExternalIds)
        };

        return changedProperties.Any(
            property => propertiesMembersHasToChanged.Contains(property, StringComparer.InvariantCulture));
    }

    /// <summary>
    ///     Creates out of the <see cref="PropertiesUpdatedPayload" /> an <see cref="FunctionPropertiesChangedEvent" />
    ///     that is needed to create a <see cref="EventTuple" />.
    /// </summary>
    /// <param name="propertiesUpdatePayload">
    ///     Contains the properties to changed, that are needed by the
    ///     <see cref="FunctionPropertiesChangedEvent" />.
    /// </param>
    /// <param name="eventObject">
    ///     The original event object that contains the needed properties for the to created a
    ///     <see cref="FunctionPropertiesChangedEvent" />.
    /// </param>
    /// <returns>A <see cref="FunctionPropertiesChangedEvent" /> that is constructed out of the given arguments.</returns>
    /// <exception cref="ArgumentNullException">Is thrown if a property is null.</exception>
    /// <exception cref="ArgumentException">Is thrown if a property is null or empty.</exception>
    internal static FunctionPropertiesChangedEvent CreateFunctionEventRelatedToPropertiesPayload(
        this PropertiesUpdatedPayload propertiesUpdatePayload,
        IDomainEvent eventObject)
    {
        if (propertiesUpdatePayload == null)
        {
            throw new ArgumentNullException(nameof(propertiesUpdatePayload));
        }

        if (eventObject == null)
        {
            throw new ArgumentNullException(nameof(eventObject));
        }

        if (propertiesUpdatePayload.Properties == null)
        {
            throw new ArgumentNullException(nameof(propertiesUpdatePayload.Properties));
        }

        if (propertiesUpdatePayload.Properties.ContainsKey(nameof(FunctionBasic.UpdatedAt)))
        {
            propertiesUpdatePayload.Properties[nameof(FunctionBasic.UpdatedAt)] = eventObject.Timestamp;
        }
        else
        {
            propertiesUpdatePayload.Properties.Add(nameof(FunctionBasic.UpdatedAt), eventObject.Timestamp);
        }

        return new FunctionPropertiesChangedEvent
        {
            MetaData = eventObject.MetaData,
            CorrelationId = eventObject.CorrelationId,
            EventId = Guid.NewGuid().ToString(),
            Initiator = eventObject.Initiator,
            Payload = new PropertiesUpdatedPayload
            {
                Properties = propertiesUpdatePayload.Properties,
                IsSynchronized = false,
                Id = propertiesUpdatePayload.Id
            },
            Timestamp = eventObject.Timestamp,
            RequestSagaId = eventObject.RequestSagaId
        };
    }
}
