using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Newtonsoft.Json;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Arango.UnitTests.V2.Helpers;
using Xunit;

namespace UserProfileService.Arango.UnitTests.V2
{
    public class JsonConverterTests
    {
        private static string GroupsJsonString =>
            "[{"
            + "\"Id\":\"a5154a2a-5cad-48bb-adba-f94671d0eb4a\","
            + "\"Name\":\"San\","
            + "\"DisplayName\":\"Test\","
            + "\"ExternalId\":null,\"Kind\":2,"
            + "\"CreatedAt\":\"2020-12-03T10:56:09Z\","
            + "\"UpdatedAt\":\"2020-12-03T10:56:09Z\","
            + "\"IsSynchronized\":false,\"Weight\":0,"
            + "\"IsSystem\":false,"
            + "\"Tags\":[{\"Name\":\"Morelia spilotes variegata\",\"Inherited\":true}],"
            + "\"FunctionalAccessRights\":[{\"Name\":\"Morelia\",\"Inherited\":true}],"
            + "\"Members\":"
            + MembersJsonString
            + ","
            + "\"MemberOf\": ["
            + GroupBasicString
            + "]"
            + "}]";

        private static string MembersJsonString =>
            "[{\"CreatedAt\": \"2020-12-03T10:56:09Z\","
            + "\"DisplayName\": \"Langston Ivanichev\","
            + "\"Email\": \"livanichev0@msn.com\","
            + "\"ExternalId\": \"f7587a24-1790-4cce-ac00-e04fe3c6397e\","
            + "\"FirstName\": \"Langston\","
            + "\"Id\": \"d5f422bb-2b46-4112-94dc-19b3b2b106fa\","
            + "\"Kind\": 1,"
            + "\"LastName\": \"Ivanichev\","
            + "\"Name\": \"livanichev0\","
            + "\"UpdatedAt\": \"2020-06-30T16:21:04Z\","
            + "\"UserName\": \"livanichev0\","
            + "\"UserStatus\": null},"
            + GroupBasicString
            + "]";

        private static string GroupBasicString =>
            "{\"Id\":\"c5154a2a-5cad-48bb-adba-f94671d0eb4a\","
            + "\"Name\":\"Fix San\","
            + "\"DisplayName\":\"Pannier\","
            + "\"ExternalId\":null,\"Kind\":2,"
            + "\"CreatedAt\":\"2020-12-03T10:56:09Z\","
            + "\"UpdatedAt\":\"2020-12-03T10:56:09Z\","
            + "\"IsSynchronized\":false,"
            + "\"Weight\":0,"
            + "\"IsSystem\":false}";

        [Fact]
        public void GroupMembersConverterTestInsideNewtonProcessTest()
        {
            var groups = JsonConvert.DeserializeObject<IList<GroupEntityModel>>(
                GroupsJsonString,
                JsonHelpers.GetContainerProfileConverter());

            // checking, if correct values were returned
            Assert.NotNull(groups);
            Assert.Equal(1, groups.Count);
            Assert.Contains(groups, g => g.Id == "a5154a2a-5cad-48bb-adba-f94671d0eb4a");

            IList<Member> members = groups.First().Members;
            IList<Member> memberOf = groups.First().MemberOf;

            Assert.Equal(1, memberOf.Count);
            Assert.Equal(2, members.Count);

            Assert.Contains(
                members,
                p
                    => p.Id == "c5154a2a-5cad-48bb-adba-f94671d0eb4a"
                    && p.Kind == ProfileKind.Group);

            Assert.Contains(
                members,
                p
                    => p.Id == "d5f422bb-2b46-4112-94dc-19b3b2b106fa"
                    && p.Kind == ProfileKind.User);

            Assert.Contains(memberOf, m => m.Id == "c5154a2a-5cad-48bb-adba-f94671d0eb4a");
        }
    }
}
