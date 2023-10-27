using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using JsonSubTypes;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;

namespace UserProfileService.Arango.Tests.V2.Helpers
{
    public class JsonHelpers
    {
        public static JsonConverter GetContainerProfileConverter()
        {
            return JsonSubtypesConverterBuilder.Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
                .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
                .Build();
        }

        public static JsonConverter GetProfileEntityConverter()
        {
            return JsonSubtypesConverterBuilder
                .Of<IProfileEntityModel>(nameof(IProfileEntityModel.Kind))
                .RegisterSubtype<UserEntityModel>(ProfileKind.User)
                .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
                .RegisterSubtype<OrganizationEntityModel>(ProfileKind.Organization)
                .Build();
        }
    }
}
