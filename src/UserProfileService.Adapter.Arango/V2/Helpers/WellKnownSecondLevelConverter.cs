using JsonSubTypes;
using Maverick.UserProfileService.AggregateEvents.Common;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.Extensions;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

/// <summary>
///     Contains Json converter for second level events
/// </summary>
public static class WellKnownSecondLevelConverter
{
    /// <summary>
    ///     Get all default json converters for second level events.
    /// </summary>
    /// <returns>   Generated <see cref="JsonConverter" /> for second level events.</returns>
    public static JsonConverter GetSecondLevelDefaultConverters()
    {
        return JsonSubtypesConverterBuilder
            .Of<IUserProfileServiceEvent>(nameof(IUserProfileServiceEvent.Type))
            .RegisterSubtypeByTypeName<AssignmentConditionTriggered>()
            .RegisterSubtypeByTypeName<ClientSettingsCalculated>()
            .RegisterSubtypeByTypeName<ClientSettingsInvalidated>()
            .RegisterSubtypeByTypeName<ContainerDeleted>()
            .RegisterSubtypeByTypeName<EntityDeleted>()
            .RegisterSubtypeByTypeName<FunctionChanged>()
            .RegisterSubtypeByTypeName<FunctionCreated>()
            .RegisterSubtypeByTypeName<GroupCreated>()
            .RegisterSubtypeByTypeName<MemberAdded>()
            .RegisterSubtypeByTypeName<MemberDeleted>()
            .RegisterSubtypeByTypeName<MemberRemoved>()
            .RegisterSubtypeByTypeName<OrganizationCreated>()
            .RegisterSubtypeByTypeName<ProfileClientSettingsSet>()
            .RegisterSubtypeByTypeName<ProfileClientSettingsUnset>()
            .RegisterSubtypeByTypeName<PropertiesChanged>()
            .RegisterSubtypeByTypeName<RoleChanged>()
            .RegisterSubtypeByTypeName<RoleCreated>()
            .RegisterSubtypeByTypeName<TagCreated>()
            .RegisterSubtypeByTypeName<TagDeleted>()
            .RegisterSubtypeByTypeName<TagsAdded>()
            .RegisterSubtypeByTypeName<TagsRemoved>()
            .RegisterSubtypeByTypeName<UserCreated>()
            .RegisterSubtypeByTypeName<WasAssignedToFunction>()
            .RegisterSubtypeByTypeName<WasAssignedToGroup>()
            .RegisterSubtypeByTypeName<WasAssignedToOrganization>()
            .RegisterSubtypeByTypeName<WasAssignedToRole>()
            .RegisterSubtypeByTypeName<WasUnassignedFrom>()
            .Build();
    }
}
