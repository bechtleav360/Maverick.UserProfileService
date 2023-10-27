using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.Tests.V2.Helpers;
using UserProfileService.Arango.Tests.V2.Mocks;
using UserProfileService.Common.V2.Abstractions;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace UserProfileService.Arango.Tests.V2
{
    public class ReadServiceSearchTests
    {
        private static readonly ILoggerFactory _loggerFactory =
            LoggerFactory.Create(b => b.SetMinimumLevel(LogLevel.Debug).AddDebug());

        private readonly ITestOutputHelper _output;

        public ReadServiceSearchTests(ITestOutputHelper output)
        {
            _output = output;
        }

        private IReadService GetArangoReadService<TEntity>(
            IEnumerable<TEntity> elements,
            Action<string> validationFunction,
            Action<XunitException> storeExceptionFunction = null)
        {
            var client = new MockedArangoDb(_output, validationFunction, storeExceptionFunction);

            client.SetupAqlResult(elements);

            IServiceCollection serviceCollection = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            return adapter;
        }

        private async Task<IPaginatedList<TEntity>> ExecuteSearchAsync<TEntity>(
            QueryObject queryObject,
            IEnumerable<TEntity> elements,
            Func<string, bool> validationFunction)
            where TEntity : class
        {
            var client = new MockedArangoDb(_output, validationFunction);

            client.SetupAqlResult(elements);

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            return await adapter.SearchAsync<TEntity>(queryObject);
        }

        private static IHttpClientFactory GetHttpClientFactory(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> messageHandler,
            ITestOutputHelper output)
        {
            return new MockHttpClientFactory(
                $"^{ArangoConstants.DatabaseClientNameUserProfileStorage}$",
                () => new HttpClient(HttpMockHelpers.GetHttpMessageHandlerMock(messageHandler).Object),
                output);
        }

        private static IArangoDbClientFactory GetClientFactory(
            Func<HttpRequestMessage, Task<HttpResponseMessage>> messageHandler,
            ITestOutputHelper output)
        {
            return new SingletonArangoDbClientFactory(GetHttpClientFactory(messageHandler, output))
                .AddClient(
                    ArangoConstants.DatabaseClientNameUserProfileStorage,
                    JsonHelpers.GetProfileEntityConverter(),
                    JsonHelpers.GetContainerProfileConverter());
        }

        [Fact]
        public async Task SearchForGroupsUsingContainFilter()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                SampleDataTestHelper.GetTestGroupEntities()
                    .Take(2),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(GroupEntityModel.Name),
                            Operator = FilterOperator.Contains,
                            BinaryOperator = BinaryOperator.Or,
                            Values = new[] { "must", "shall" }
                        }
                    }
                }
            };

            IPaginatedList<IProfile> groups = await adapter.GetProfilesAsync<User, Group, Organization>(
                RequestedProfileKind.Group,
                options);

            exceptionList.ForEach(e => throw e);

            Assert.NotEmpty(groups);
            Assert.Equal(2, groups.Count);
            Assert.Contains(groups, g => g is Group);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*g0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\[\\s*\"must\"\\s*,\\s*\"shall\"\\s*\\]\\[\\*\\s+RETURN\\s+LIKE\\(g0\\.Name\\s*,\\s*CONCAT\\(\"%\",CURRENT,\"%\"\\)\\s*,\\s*true\\)]\\s*ANY\\s*==\\s*true\\s*\\)\\)",
                    message);
            }
        }

        [Fact]
        public async Task GetGroupsFilteredBySynchronizedAtTime()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                SampleDataTestHelper.GetTestGroupEntities()
                    .Take(2),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    CombinedBy = BinaryOperator.And,
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            Operator = FilterOperator.GreaterThan,
                            Values = new[]
                            {
                                new DateTime(2020, 03, 15)
                                    .ToString("O")
                            }, // Iso-String
                            BinaryOperator = BinaryOperator.And,
                            FieldName = nameof(Group.SynchronizedAt)
                        }
                    }
                }
            };

            IPaginatedList<IProfile> groups = await adapter.GetProfilesAsync<User, Group, Organization>(
                RequestedProfileKind.Group,
                options);

            exceptionList.ForEach(e => throw e);

            Assert.NotEmpty(groups);
            Assert.Equal(2, groups.Count);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*g0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\[\"2020\\-03\\-15T00\\:00\\:00\\.0000000\"\\]\\s*ALL\\s*<g0\\.SynchronizedAt\\s*\\)\\)",
                    message);
            }
        }

        [Fact]
        public async Task SearchForGroupsUsingContainFilterWithWeight()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<IProfile>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(GroupEntityModel.Weight),
                            Operator = FilterOperator.Contains,
                            BinaryOperator = BinaryOperator.And,
                            Values = new[] { "2", "23" }
                        }
                    }
                }
            };

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.Group,
                options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*g0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\[2.0,23.0\\]\\s*ALL\\s*==\\s*g0\\.Weight\\s*\\)\\)",
                    message);
            }
        }

        [Fact]
        public async Task SearchForGroupsUsingContainFilterRole()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<FunctionBasic>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = "Role.Name",
                            Operator = FilterOperator.Contains,
                            BinaryOperator = BinaryOperator.And,
                            Values = new[] { "Test" }
                        }
                    }
                }
            };

            await adapter.GetFunctionsAsync<FunctionBasic>(options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER \\(f0\\.Type == \"Function\" AND \\[\"Test\"\\]\\[\\* RETURN LIKE\\(f0\\.Role\\.Name,CONCAT\\(\"%\",CURRENT,\"%\"\\),true\\)\\]ALL==true\\)",
                    message);
            }
        }

        [Fact]
        public async Task SearchForFunctionsUsingContainFilterWithLinkedProfiles()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<FunctionView>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = $"{nameof(FunctionView.LinkedProfiles)}.{nameof(Member.Name)}",
                            Operator = FilterOperator.Contains,
                            BinaryOperator = BinaryOperator.Or,
                            Values = new[] { "rex", "cody" }
                        }
                    }
                }
            };

            await adapter.GetFunctionsAsync<FunctionView>(options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(f0\\.Type\\s*==\\s*\"Function\"\\s*AND\\s*\\(FOR\\s+insideProperty\\s+IN\\s+NOT_NULL\\(f0\\.LinkedProfiles,\\[\\]\\)\\s*RETURN\\s*\\[\"rex\",\"cody\"\\]\\[\\*\\s+RETURN\\s+LIKE\\(insideProperty\\.Name,CONCAT\\(\"%\",CURRENT,\"%\"\\),true\\)\\]\\s*ANY\\s*==\\s*true\\)\\s*ANY\\s*==\\s*true\\)",
                    message);
            }
        }

        [Fact]
        public async Task SearchForUsersUsingContainFilterWithTags()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                SampleDataTestHelper.GetTestGroupEntities()
                    .Take(2),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(UserEntityModel.Tags),
                            Operator = FilterOperator.Contains,
                            BinaryOperator = BinaryOperator.And,
                            Values = new[] { "tag no. _1", "tag no. %2", "tag no.  3" }
                        }
                    }
                }
            };

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                RequestedProfileKind.User,
                options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*u0\\.Kind\\s+==\\s+\"User\"\\s+AND\\s+\\(\\s*FOR\\s+insideProperty\\s+IN\\s+NOT_NULL\\(u0\\.Tags,\\[\\]\\)\\s*RETURN\\s*\\[\"tag no\\. \\\\\\\\_1\",\"tag no\\. \\\\\\\\%2\",\"tag no\\.  3\"\\]\\[\\*\\s+RETURN\\s+LIKE\\(insideProperty\\.Name,CONCAT\\(\"%\",CURRENT,\"%\"\\),true\\)]\\s*ALL\\s*==\\s*true\\)\\s+ANY\\s*==\\s*true\\s*\\)\\)",
                    message);
            }
        }

        [Fact]
        public async Task SearchForUsersButQueryObjectIsNull()
        {
            await Assert.ThrowsAsync<ValidationException>(
                () =>
                    ExecuteSearchAsync(
                        null,
                        Enumerable.Empty<IProfileEntityModel>(),
                        _ => true));
        }

        [Fact]
        public async Task SearchForUsersButQueryObjectIsEmpty()
        {
            await ExecuteSearchAsync(
                new QueryObject(),
                Enumerable.Empty<IProfileEntityModel>(),
                _ => true);
        }

        [Fact]
        public async Task GetProfilesUsingSearchProperty()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<IProfileEntityModel>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Search = "term"
            };

            await adapter.GetProfilesAsync<UserView, GroupView, OrganizationView>(RequestedProfileKind.All, options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                const string pattern =
                    @"FILTER\s*\(\(\s*u0\.Kind\s*==\s*""User""\s*AND\s*" + 
                    @"\(\s*\(\s*\(\s*\(\s*\(\s*LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*" +
                    @"OR\s*LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*" + 
                    @"OR\s+LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*" +
                    @"OR\s+LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*" +
                    @"OR\s+LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*" +
                    @"OR\s+LIKE\s*\(\s*u0\.(?:[A-Za-z]*Name|Email)\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*\)\s*" +
                    @"OR\s*\(\s*u0\.Kind\s*==\s*""Group""\s*AND\s*" +
                    @"\(\s*LIKE\s*\(\s*u0\.(?:Display)?Name\s*,\s*""%term%""\s*,\s*true\s*\)\s*" +
                    @"OR\s+LIKE\s*\(\s*u0\.(?:Display)?Name\s*,\s*""%term%""\s*,\s*true\s*\)\s*\)\s*\)\s*" +
                    @"OR\s*\(\s*u0\.Kind\s*==\s*""Organization""\s*AND\s*" +
                    @"\(\s*LIKE\s*\(\s*u0\.(?:Display)?Name\s*,\s*""%term%""\s*,\s*true\s*\)\s*" +
                    @"OR\s+LIKE\s*\(\s*u0\.(?:Display)?Name\s*,\s*""%term%""\s*,\s*true\s*\)\)\)\)";

                Assert.Matches(pattern, message);
            }
        }

        [Fact]
        public async Task GetGroupProfilesUsingSearchProperty()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<GroupEntityModel>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Search = "Martin"
            };

            await adapter.GetProfilesAsync<UserView, GroupView, OrganizationView>(RequestedProfileKind.Group, options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                const string pattern =
                    @"FILTER\s*\(\s*g0\.Kind\s*==\s*""Group""\s*" + 
                    @"AND\s*\(\s*LIKE\s*\(\s*g0\.(?:Display)?Name\s*,\s*""%Martin%""\s*,\s*true\s*\)\s*" + 
                    @"OR\s+LIKE\s*\(\s*g0\.(?:Display)?Name\s*,\s*""%Martin%""\s*,\s*true\s*\)\s*\)\s*\)";

                Assert.Matches(pattern, message);
            }
        }

        [Fact]
        public async Task GetUserProfilesUsingSearchProperty()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                new List<IProfile>(),
                Validate,
                exc => exceptionList.Add(exc));

            var options = new AssignmentQueryObject
            {
                Search = "Freeman"
            };

            await adapter.GetProfilesAsync<UserView, GroupView, OrganizationView>(RequestedProfileKind.User, options);

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string message)
            {
                const string pattern =
                    @"FILTER\s*\(\s*u0\.Kind\s+==\s+""User""\s+AND\s+" +
                    @"\(\(\(\(\(LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\s+" +
                    @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\)\s+" +
                    @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\)\s+" +
                    @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\)\s+" +
                    @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\)\s+" +
                    @"OR\s+LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%Freeman%"",true\)\)\)";

                Assert.Matches(pattern, message);
            }
        }

        [Fact]
        public async Task GetGroupProfileWithDifferentValuesInOneDefinitionFilter()
        {
            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*g0.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\[\\s*\"test1\"\\s*,\\s*\"test2\"\\s*,\\s*\"test3\"\\s*]\\s*ANY\\s*==\\s*g0.DisplayName\\s*\\)\\)",
                    message);
            }

            var exceptionList = new List<XunitException>();

            var queryOptions = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(IProfile.DisplayName),
                            Values = new[] { "test1", "test2", "test3" },
                            BinaryOperator = BinaryOperator.Or,
                            Operator = FilterOperator.Equals
                        }
                    }
                },
                Limit = 5,
                Offset = 0,
                OrderedBy = nameof(IProfile.CreatedAt),
                SortOrder = SortOrder.Desc
            };

            IReadService adapter = GetArangoReadService(
                new List<IProfile>(),
                Validate,
                exc => exceptionList.Add(exc));

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationView>(
                RequestedProfileKind.Group,
                queryOptions);

            exceptionList.ForEach(e => throw e);
        }

        [Fact]
        public async Task GetUserProfileWithDifferentValuesInOneDefinitionCombinedByAndFilter()
        {
            static void Validate(string message)
            {
                Assert.Matches(
                    "FILTER\\s*\\(\\(\\s*u0.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\[\\s*\"test1\"\\s*,\\s*\"test2\"\\s*,\\s*\"test3\"\\s*]\\s*ALL\\s*==\\s*u0.LastName\\s*\\)\\)",
                    message);
            }

            var exceptionList = new List<XunitException>();

            var queryOptions = new AssignmentQueryObject
            {
                Filter = new Filter
                {
                    Definition = new List<Definitions>
                    {
                        new Definitions
                        {
                            FieldName = nameof(UserBasic.LastName),
                            Values = new[] { "test1", "test2", "test3" },
                            BinaryOperator = BinaryOperator.And,
                            Operator = FilterOperator.Equals
                        }
                    }
                },
                Limit = 5,
                Offset = 0,
                OrderedBy = nameof(IProfile.CreatedAt),
                SortOrder = SortOrder.Asc
            };

            IReadService adapter = GetArangoReadService(
                new List<IProfile>(),
                Validate,
                exc => exceptionList.Add(exc));

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationView>(
                RequestedProfileKind.User,
                queryOptions);

            exceptionList.ForEach(e => throw e);
        }

        [Fact]
        public async Task SearchForTaggedProfiles()
        {
            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                Enumerable.Empty<Group>(),
                Validate,
                exc => exceptionList.Add(exc));

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationView>(
                options: new AssignmentQueryObject
                {
                    TagFilters = new List<string>
                    {
                        "yellow"
                    },
                    Search = "gelb"
                });

            exceptionList.ForEach(e => throw e);

            return;

            static void Validate(string query)
            {
                const string pattern =
                    @"FILTER\(\(\(u0\.Kind\s*==\s*""User""\s*AND\s*" +
                    @"\(\(\(\(\(LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:[A-Za-z]*Name|Email),""%gelb%"",true\)\)\)\s*OR\s*" +
                    @"\(u0\.Kind\s*==\s*""Group""\s*AND\s*\(LIKE\(u0\.(?:Display)?Name,""%gelb%"",true\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:Display)?Name,""%gelb%"",true\)\)\)\s*OR\s*\(u0\.Kind\s*==\s*""Organization""\s*" +
                    @"AND\s*\(LIKE\(u0\.(?:Display)?Name,""%gelb%"",true\)\s*OR\s+" +
                    @"LIKE\(u0\.(?:Display)?Name,""%gelb%"",true\)\)\)\)\s*AND\s*" + 
                    @"\[""yellow""\]\s*ALL\s+IN\s+NOT_NULL\(u0\.Tags,\[\]\)\[\*\s+RETURN\s+LOWER\(CURRENT\.Name\)\]\)";

                Assert.Matches(pattern, query);
            }
        }

        [Fact]
        public async Task GetTaggedProfilesFilteredByCreationDate()
        {
            static void Validate(string query)
            {
                Assert.Matches(
                    "FILTER\\(\\(\\(\\(u0\\.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\[\"2020\\-03\\-15T00:00:00\"\\]\\s*ANY\\s*<u0\\.CreatedAt\\)\\s*OR\\s*\\(u0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\[\"2020\\-03\\-15T00:00:00\"\\]\\s*ANY\\s*<u0\\.CreatedAt\\)\\s*OR\\s*\\(u0\\.Kind\\s*==\\s*\"Organization\"\\s*AND\\s*\\[\"2020\\-03\\-15T00:00:00\"\\]\\s*ANY\\s*<u0\\.CreatedAt\\)\\)\\)\\s*AND\\s*\\[\"yellow\"\\]\\s*ALL\\s+IN\\s+NOT_NULL\\(u0\\.Tags,\\[\\]\\)\\[\\*\\s+RETURN\\s+LOWER\\(CURRENT\\.Name\\)\\]\\)",
                    query);
            }

            var exceptionList = new List<XunitException>();

            IReadService adapter = GetArangoReadService(
                Enumerable.Empty<Group>(),
                Validate,
                exc => exceptionList.Add(exc));

            await adapter.GetProfilesAsync<UserBasic, GroupBasic, OrganizationBasic>(
                options: new AssignmentQueryObject
                {
                    TagFilters = new List<string>
                    {
                        "yellow"
                    },
                    Filter = new Filter
                    {
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = nameof(IProfile.CreatedAt),
                                Operator = FilterOperator.GreaterThan,
                                Values = new[] { "2020-03-15" }
                            }
                        }
                    }
                });

            exceptionList.ForEach(e => throw e);
        }
    }
}
