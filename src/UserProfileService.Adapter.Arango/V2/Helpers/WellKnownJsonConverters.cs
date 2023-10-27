using System.Collections.Generic;
using JsonSubTypes;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Common.V2.TicketStore.Abstractions;
using UserProfileService.Common.V2.TicketStore.Models;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;

namespace UserProfileService.Adapter.Arango.V2.Helpers;

internal static class WellKnownJsonConverters
{
    internal static IEnumerable<JsonConverter> GetDefaultProfileConverters()
    {
        yield return JsonSubtypesConverterBuilder.Of<IProfileEntityModel>(nameof(IProfileEntityModel.Kind))
            .RegisterSubtype<UserEntityModel>(ProfileKind.User)
            .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
            .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
            .Build();

        yield return JsonSubtypesConverterBuilder
            .Of<IContainerProfileEntityModel>(nameof(IContainerProfileEntityModel.Kind))
            .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
            .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
            .Build();

        yield return JsonSubtypesConverterBuilder.Of<IProfile>(nameof(IProfile.Kind))
            .RegisterSubtype<UserEntityModel>(ProfileKind.User)
            .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
            .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
            .Build();

        yield return JsonSubtypesConverterBuilder.Of<IAssignmentObjectEntity>(nameof(IAssignmentObjectEntity.Type))
            .RegisterSubtype<FunctionObjectEntityModel>(RoleType.Function)
            .RegisterSubtype<RoleObjectEntityModel>(RoleType.Role)
            .Build();

        yield return JsonSubtypesConverterBuilder.Of<ILinkedObject>(nameof(ILinkedObject.Type))
            .RegisterSubtype<LinkedFunctionObject>(RoleType.Function.ToString())
            .RegisterSubtype<LinkedRoleObject>(RoleType.Role.ToString())
            .Build();

        yield return new StringEnumConverter();

        yield return GetAssignmentSubtypeConverter();
    }

    internal static IEnumerable<JsonConverter> GetDefaultTicketStoreConverters()
    {
        yield return JsonSubtypesConverterBuilder.Of<TicketBase>(nameof(TicketBase.Type))
            .RegisterSubtype<UserProfileOperationTicket>(UserProfileOperationTicket.TicketType)
            .Build();
    }

    internal static IEnumerable<JsonConverter> GetDefaultFirstLevelProjectionConverters()
    {
        yield return JsonSubtypesConverterBuilder
            .Of<IFirstLevelProjectionProfile>(nameof(IFirstLevelProjectionProfile.Kind))
            .RegisterSubtype<FirstLevelProjectionUser>(ProfileKind.User)
            .RegisterSubtype<FirstLevelProjectionGroup>(ProfileKind.Group)
            .RegisterSubtype<FirstLevelProjectionOrganization>(ProfileKind.Organization)
            .Build();

        yield return JsonSubtypesConverterBuilder
            .Of<IFirstLevelProjectionContainer>(nameof(IFirstLevelProjectionContainer.ContainerType))
            .RegisterSubtype<FirstLevelProjectionRole>(ContainerType.Role)
            .RegisterSubtype<FirstLevelProjectionFunction>(ContainerType.Function)
            .RegisterSubtype<FirstLevelProjectionGroup>(ContainerType.Group)
            .RegisterSubtype<FirstLevelProjectionOrganization>(ContainerType.Organization)
            .Build();

        yield return new StringEnumConverter();
    }

    internal static IEnumerable<JsonConverter> GetDefaultSecondLevelAssignmentProjectionConverters()
    {
        yield return GetAssignmentSubtypeConverter();
        yield return new StringEnumConverter();
    }

    internal static JsonConverter GetAssignmentSubtypeConverter()
    {
        return JsonSubtypesConverterBuilder
            .Of<ISecondLevelAssignmentContainer>(nameof(ISecondLevelAssignmentContainer.ContainerType))
            .RegisterSubtype<SecondLevelAssignmentContainer>(ContainerType.Role)
            .RegisterSubtype<SecondLevelAssignmentContainer>(ContainerType.Group)
            .RegisterSubtype<SecondLevelAssignmentContainer>(ContainerType.Organization)
            .RegisterSubtype<SecondLevelAssignmentFunction>(ContainerType.Function)
            .Build();
    }
}
