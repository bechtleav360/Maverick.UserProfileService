using System.Collections.Generic;
using System.Linq;
using JsonSubTypes;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UserProfileService.Common.V2.Abstractions;

namespace UserProfileService.Common.Tests.Utilities.Serialization
{
    public class TestingEntityDetailsLevelJsonSerializerSettingsProvider : IJsonSerializerSettingsProvider
    {
        private readonly string _detailsLevel;

        public TestingEntityDetailsLevelJsonSerializerSettingsProvider(string detailsLevel = null)
        {
            _detailsLevel = detailsLevel;
        }

        private JsonSerializerSettings GetDefaultSettings()
        {
            return new JsonSerializerSettings
            {
                Converters = GetConverters().ToList(),
                TypeNameHandling = TypeNameHandling.None,
                ContractResolver = new DefaultContractResolver(),
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                DateFormatHandling = DateFormatHandling.IsoDateFormat
            };
        }

        private IEnumerable<JsonConverter> GetConverters()
        {
            if (string.IsNullOrWhiteSpace(_detailsLevel))
            {
                yield return JsonSubtypesConverterBuilder
                    .Of<IProfile>(nameof(IProfile.Kind))
                    .RegisterSubtype<User>(ProfileKind.User)
                    .RegisterSubtype<Group>(ProfileKind.Group)
                    .RegisterSubtype<Organization>(ProfileKind.Organization)
                    .Build();

                yield return JsonSubtypesConverterBuilder
                    .Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                    .RegisterSubtype<Group>(ProfileKind.Group)
                    .RegisterSubtype<Organization>(ProfileKind.Organization)
                    .Build();

                yield break;
            }

            if (_detailsLevel == "view")
            {
                yield return JsonSubtypesConverterBuilder
                    .Of<IProfile>(nameof(IProfile.Kind))
                    .RegisterSubtype<UserView>(ProfileKind.User)
                    .RegisterSubtype<GroupView>(ProfileKind.Group)
                    .RegisterSubtype<OrganizationView>(ProfileKind.Organization)
                    .Build();

                yield return JsonSubtypesConverterBuilder
                    .Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                    .RegisterSubtype<GroupView>(ProfileKind.Group)
                    .RegisterSubtype<OrganizationView>(ProfileKind.Organization)
                    .Build();

                yield return JsonSubtypesConverterBuilder
                    .Of<IAssignmentObject>(nameof(IAssignmentObject.Type))
                    .RegisterSubtype<FunctionView>(RoleType.Function)
                    .RegisterSubtype<RoleView>(RoleType.Role)
                    .Build();
            }

            if (_detailsLevel == "basic")
            {
                yield return JsonSubtypesConverterBuilder
                    .Of<IProfile>(nameof(IProfile.Kind))
                    .RegisterSubtype<UserBasic>(ProfileKind.User)
                    .RegisterSubtype<GroupBasic>(ProfileKind.Group)
                    .RegisterSubtype<OrganizationBasic>(ProfileKind.Organization)
                    .Build();

                yield return JsonSubtypesConverterBuilder
                    .Of<IContainerProfile>(nameof(IContainerProfile.Kind))
                    .RegisterSubtype<GroupView>(ProfileKind.Group)
                    .RegisterSubtype<OrganizationView>(ProfileKind.Organization)
                    .Build();

                yield return JsonSubtypesConverterBuilder
                    .Of<IAssignmentObject>(nameof(IAssignmentObject.Type))
                    .RegisterSubtype<FunctionBasic>(RoleType.Function)
                    .RegisterSubtype<RoleBasic>(RoleType.Role)
                    .Build();
            }
        }

        public JsonSerializerSettings GetNewtonsoftSettings()
        {
            return GetDefaultSettings();
        }
    }
}
