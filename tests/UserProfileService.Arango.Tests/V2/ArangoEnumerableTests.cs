using System.Collections.Generic;
using System.Linq;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.EntityModels.QueryBuilding;
using UserProfileService.Adapter.Arango.V2.Extensions;
using UserProfileService.Adapter.Arango.V2.Helpers;
using UserProfileService.Arango.Tests.V2.Helpers;
using Xunit;

namespace UserProfileService.Arango.Tests.V2
{
    public class ArangoEnumerableTests
    {
        [Fact]
        public void Filter_by_conditional_count()
        {
            var startingPoint = new ArangoDbEnumerable<Organization>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Where(g => g.Members.Count(m => m.Kind == ProfileKind.User || m.Kind == ProfileKind.Organization) > 0)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+o0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s+o0\\.Kind\\s*==\\s*\"Organization\"\\s*"
                + "AND\\s*\\(COUNT\\(NOT_NULL\\(o0\\.Members,\\[\\]\\)\\[\\*\\s+FILTER\\s+\\(\\(CURRENT\\.Kind\\s*==\\s*\"User\"\\)\\s*OR\\s+\\(CURRENT\\.Kind\\s*==\\s*\"Organization\"\\)\\)\\]\\)\\s*>\\s*0\\)\\s*"
                + "RETURN\\s+o0",
                text);
        }

        [Fact]
        public void Filter_by_conditional_count_nested()
        {
            var startingPoint = new ArangoDbEnumerable<Organization>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var test = true;

            string text = startingPoint
                .Where(
                    g => g.Members.Count(
                            m => (m.Kind == ProfileKind.User || m.Kind == ProfileKind.Organization) && test)
                        > 0)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+o0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                + "FILTER\\s+o0\\.Kind\\s*==\\s*\"Organization\"\\s*"
                + "AND\\s*\\(COUNT\\(NOT_NULL\\(o0\\.Members,\\[\\]\\)\\[\\*\\s+"
                + "FILTER\\s+\\(\\(\\(CURRENT\\.Kind\\s*==\\s*\"User\"\\)\\s*OR\\s+\\(CURRENT\\.Kind\\s*==\\s*\"Organization\"\\)\\)\\s+"
                + "AND\\s+True\\)\\]\\)\\s*>\\s*0\\)\\s*"
                + "RETURN\\s+o0",
                text);
        }

        [Fact]
        public void Select_root_groups()
        {
            var startingPoint = new ArangoDbEnumerable<Group>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Where(g => ValidationHelper.IsNullOrEmpty(g.MemberOf))
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"FOR\\s+g0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+g0\\.Kind\\s*==\\s*\"Group\"\\s+AND\\s*\\(g0\\.MemberOf\\s*==\\s*null\\s+OR\\s+COUNT\\s*\\(\\s*g0\\.MemberOf\\s*\\)\\s*==\\s*0\\s*\\)\\s+RETURN\\s+g0",
                text);
        }

        [Fact]
        public void Select_users_and_groups()
        {
            var startingPoint = new ArangoDbEnumerable<User>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Select(u => u.Name)
                .Where(u => u.Name == "Test")
                .Combine<Group>()
                .Where(g => g.Name == "Grp")
                .ToQuery(CollectionScope.Query);

            Assert.Matches($"^FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}", text);

            Assert.Matches(
                "FILTER\\s*\\(\\s*u0.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\(\\s*u0\\.Name==\"Test\"\\s*\\)\\s*OR\\s+u0\\.Kind\\s*==\\s*\"Group\"\\s+AND \\s*\\(\\s*u0.Name==\"Grp\"\\s*\\)\\s*\\)",
                text);

            Assert.Matches("\\s+RETURN\\s+u0.Name$", text);
        }

        [Fact]
        public void Simple_get_all()
        {
            var startingPoint = new ArangoDbEnumerable<User>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Select(u => u)
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"^FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+u0.Kind\\s*==\\s*\"User\"\\s+RETURN\\s+u0$",
                text);
        }

        [Fact]
        public void Select_using_filter_object_to_sort_and_limit()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var opt = new QueryObjectList
            {
                Limit = 23,
                Offset = 1,
                ProfileKind = RequestedProfileKind.User,
                OrderedBy = nameof(IProfile.Name),
                SortOrder = SortOrder.Desc,
                Filter = "blabla",
                Tags = new List<string>
                {
                    "Development",
                    "my stuff",
                    "",
                    null
                }
            };

            string text = startingPoint
                .Where(p => p.Name.StartsWith("Test"))
                .UsingOptions(opt)
                .Select(p => p)
                .ToQuery(CollectionScope.Query);

            Assert.Matches($@"^FOR\s+i0\s+IN\s+{"profilesQuery".GetDefaultCollectionNameInTest()}", text);
            Assert.Matches(@"SORT\s+i0.Name\s+DESC\s+LIMIT\s+1\s*,\s*23", text);

            const string filterPattern =
                @"FILTER\(LIKE\(i0\.Name,""Test%"",true\)\s*AND\s*" + 
                @"\[""development"",""my\s+stuff""\]\s*ALL\s+IN\s+NOT_NULL\(i0\.Tags,\[\]\)" + 
                @"\[\*\s+RETURN\s+LOWER\(CURRENT\.Name\)\]\s*AND\s*\(i0\.Kind\s+==\s+""User""\s+" + 
                @"AND\s+\(\(\(\(\(LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\s+" + 
                @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\)\s+" + 
                @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\)\s+" + 
                @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\)\s+" + 
                @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\)\s+" + 
                @"OR\s+LIKE\(i0\.(?:[A-Za-z]*Name|Email),""%blabla%"",true\)\)\)";

            Assert.Matches(filterPattern, text);

            Assert.Matches("RETURN\\s+i0$", text);
        }

        [Fact]
        public void Get_single_profile()
        {
            var startingPoint =
                new ArangoDbEnumerable<IProfileEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Where(p => p.Id == "test#123")
                .Select(p => p)
                .ToQuery(CollectionScope.Query);

            Assert.Matches($"^FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}", text);
            Assert.Matches("FILTER\\s*\\(\\s*i0.Id\\s*==\\s*\"test#123\"\\s*\\)", text);
            Assert.Matches("RETURN\\s+i0$", text);
        }

        [Fact]
        public void Get_all_users_starts_with_an_a()
        {
            var startingPoint = new ArangoDbEnumerable<User>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Where(u => u.Name.StartsWith("A") && u.Name.EndsWith("Z"))
                .ToQuery(CollectionScope.Query);

            Assert.Matches(
                $"^FOR\\s+u0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+u0.Kind\\s*==\\s*\"User\"\\s+AND\\s*\\(\\s*LIKE\\s*\\(\\s*u0.Name,\"A%\",true\\s*\\)\\s*AND\\s+LIKE\\s*\\(\\s*u0.Name\\s*,\\s*\"%Z\"\\s*,\\s*true\\s*\\)\\s*\\s*\\)\\s*RETURN\\s+u0$",
                text);
        }

        [Fact]
        public void Get_children_of_system_group()
        {
            var startingPoint =
                new ArangoDbEnumerable<GroupEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .First(g => g.Id == "123")
                .Select(g => g.Members)
                .AsSubQueryIn<GroupEntityModel, IProfile>(
                    temp => temp
                        .Where(p => p.Name.StartsWith("A"))
                        .SortBy("DisplayName")
                        .Skip(0)
                        .Take(10))
                .ToQuery(CollectionScope.Query);

            string checkingPattern =
                $"FOR\\s+nested0\\s+IN\\s+FLATTEN\\s*\\(\\s*FOR\\s+g0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}"
                + "\\s+FILTER\\s+g0\\.Kind\\s*==\\s*\"Group\"\\s+AND\\s*\\(\\s*g0\\.Id\\s*==\\s*\"123\"\\s*\\)\\s+LIMIT"
                + "\\s+0\\s*,\\s*1\\s+RETURN\\s+g0\\.Members\\s*\\)\\s*FILTER\\s+LIKE\\s*\\(\\s*nested0\\.Name\\s*,"
                + "\\s*\"A%\"\\s*,\\s*true\\s*\\)\\s+SORT\\s+nested0\\.DisplayName\\s+Asc\\s+LIMIT\\s+0\\s*,"
                + "\\s*10\\s+RETURN\\s+nested0";

            Assert.Matches(checkingPattern, text);
        }

        [Fact]
        public void Get_role_and_resolve_linked_properties()
        {
            var startingPoint =
                new ArangoDbEnumerable<RoleObjectEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .First(r => r.Id == "22")
                .CastAndResolveProperties<RoleObjectEntityModel, RoleView>()
                .ToQuery(CollectionScope.Query);

            string checkingPattern =
                $"^FOR\\s+r0\\s+IN\\s+{"rolesFunctionsQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+r0\\.Type\\s*==\\s*\"Role\"\\s+AND\\s*\\"
                + "(\\s*r0\\.Id\\s*==\\s*\"22\"\\s*\\)\\s*LIMIT\\s+0,1\\s+RETURN\\s+r0$";

            Assert.Matches(checkingPattern, text);
        }

        [Fact]
        public void GetParentGroupsOfUserId()
        {
            var startingPoint =
                new ArangoDbEnumerable<GroupEntityModel>(DefaultModelConstellation.CreateNew().ModelsInfo);

            string text = startingPoint
                .Where(g => g.Members.Any(m => m.Id == "userid"))
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+g0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+g0\\.Kind\\s*==\\s*\"Group\"\\s+AND\\s*g0\\.Members"
                + "\\[\\*\\]\\.Id\\s+ANY\\s*==\\s*\"userid\"\\s*RETURN\\s+g0";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void GetActivityLogsOfGroups()
        {
            var startingPoint =
                new ArangoDbEnumerable<ActivityLogEntry>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var targetType = ObjectType.Group;

            string text = startingPoint
                .Where(a => a.Scope == targetType)
                .DistinctByKey(a => a.EventId)
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+a0\\s+IN\\s+{"activityLogs".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+\\(a0\\.Scope\\s*==\\s*\"Group\"\\s*\\)\\s*"
                + "LET\\s+value\\s*=\\s*FIRST\\(RETURN\\s+a0\\)\\s*COLLECT\\s+key\\s*=\\s*value\\.EventId\\s+INTO\\s+grouped\\s*=\\s*value\\s+"
                + "LET\\s+a0\\s*=\\s*FIRST\\(grouped\\)\\s+RETURN\\s+a0";

            Assert.Matches(pattern, text);
        }

        [Fact]
        public void GetActivityLogsOfGroupsSorted()
        {
            var startingPoint =
                new ArangoDbEnumerable<ActivityLogEntry>(DefaultModelConstellation.CreateNew().ModelsInfo);

            var targetType = ObjectType.Group;

            string text = startingPoint
                .Where(a => a.Scope == targetType)
                .SortBy(nameof(ActivityLogEntry.Timestamp), SortOrder.Desc)
                .DistinctByKey(a => a.EventId)
                .ToQuery(CollectionScope.Query);

            string pattern =
                $"FOR\\s+a0\\s+IN\\s+{"activityLogs".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+\\(a0\\.Scope\\s*==\\s*\"Group\"\\s*\\)\\s*"
                + "LET\\s+value\\s*=\\s*FIRST\\(RETURN\\s+a0\\)\\s*COLLECT\\s+key\\s*=\\s*value\\.EventId\\s+INTO\\s+grouped\\s*=\\s*value\\s+"
                + "LET\\s+a0\\s*=\\s*FIRST\\(grouped\\)\\s*SORT\\s+a0\\.Timestamp\\s+DESC\\s+RETURN\\s+a0";

            Assert.Matches(pattern, text);
        }
    }
}
