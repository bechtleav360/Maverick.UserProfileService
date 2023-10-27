using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.BasicModels;
using Maverick.UserProfileService.Models.EnumModels;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using FluentAssertions;
using JsonSubTypes;
using Maverick.Client.ArangoDb.Public.Extensions;
using Maverick.UserProfileService.AggregateEvents.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using UserProfileService.Adapter.Arango.V2.Contracts;
using UserProfileService.Adapter.Arango.V2.EntityModels;
using UserProfileService.Adapter.Arango.V2.Implementations;
using UserProfileService.Arango.Tests.V2.Helpers;
using UserProfileService.Arango.Tests.V2.Mocks;
using UserProfileService.Common.Tests.Utilities.Comparers;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Group = Maverick.UserProfileService.Models.Models.Group;
using ObjectType = Maverick.UserProfileService.Models.EnumModels.ObjectType;
using ProfileKind = Maverick.UserProfileService.Models.EnumModels.ProfileKind;
using TagType = Maverick.UserProfileService.Models.EnumModels.TagType;

namespace UserProfileService.Arango.Tests.V2
{
    public class ReadServiceTests
    {
        private const string RegExDocumentCountValidation = "^RETURN\\s*{\\s*DocumentCount:";

        private static readonly ILoggerFactory _loggerFactory =
            LoggerFactory.Create(
                b => b.SetMinimumLevel(LogLevel.Debug).AddDebug().AddSimpleLogMessageCheckLogger(true));

        private readonly ITestOutputHelper _output;

        public ReadServiceTests(ITestOutputHelper output)
        {
            _output = output;
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
                    JsonSubtypesConverterBuilder
                        .Of<IProfileEntityModel>(nameof(IProfileEntityModel.Kind))
                        .RegisterSubtype<UserEntityModel>(ProfileKind.User)
                        .RegisterSubtype<GroupEntityModel>(ProfileKind.Group)
                        .Build());
        }

        private static string GetPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return
                string.Concat(
                    value[..1].ToUpperInvariant(),
                    value[1..]);
        }

        [Theory]
        [MemberData(nameof(GetUserSettingsTestArguments))]
        public async Task Get_user_settings(string profileId, ProfileKind kind, bool onlyGroups)
        {
            var callNo = 0;

            void Validation(string query)
            {
                if (callNo == 0)
                {
                    return;
                }

                Assert.Matches(
                    $"FILTER\\s*\\(\\(c0\\.ProfileId\\s*==\\s*\"{profileId}\"\\)\\s*AND\\s*\\(c0\\.SettingsKey\\s*==\\s*\"user-config\"\\)\\)",
                    query);
            }

            IEnumerable<object> GetResultObjects()
            {
                if (callNo == 0)
                {
                    callNo++;

                    return new List<string>
                    {
                        $"profiles/{profileId}"
                    };
                }

                callNo++;

                return SpecificTestData.ProfileSettings.GetSettingsEntities(onlyGroups);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(_output, Validation, e => exceptionList.Add(e));

            client.SetupAqlResult(GetResultObjects);

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            JObject response = await adapter.GetSettingsOfProfileAsync(
                profileId,
                kind,
                SpecificTestData.ProfileSettings.SettingsKey);

            exceptionList.ForEach(e => throw e);

            JObject reference = SpecificTestData.ProfileSettings
                                                .GetSettingsEntities(onlyGroups)
                                                .FirstOrDefault()
                                                ?
                                                .Value;

            Assert.Equal(reference, response);
        }

        [Fact]
        public async Task Try_to_get_profiles()
        {
            var callNo = 0;

            // AQL verification for this method
            void Validation(string query)
            {
                const string filterCheckPattern =
                    "FILTER\\s*\\(\\s*\\(\\s*u0.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\[\\s*\"A\"\\s*,\\s*\"B\"\\s*\\]\\s*ANY\\s*==\\s*u0.FirstName\\s*\\)\\s*AND\\s*\\(\\s*u0.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\[\\s*\"A\"\\s*,\\s*\"123cbd\"\\s*\\]\\s*ALL\\s*==\\s*u0.LastName\\s*\\)\\s*AND\\s*\\(\\s*u0.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\[\\s*\"test@email.com\"\\s*\\]\\s*ANY\\s*==\\s*u0.Email\\s*\\)\\s*\\)";

                switch (callNo)
                {
                    case 0:
                        Assert.Matches(RegExDocumentCountValidation, query);

                        break;
                    case 1:
                        Assert.Matches(
                            $"^FOR\\s+[A-Za-z0-9]0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER",
                            query);

                        Assert.Matches(
                            "SORT\\s+[A-Za-z0-9]0.Name\\s+DESC\\s+LIMIT\\s+2\\s*,5",
                            query);

                        Assert.Matches(filterCheckPattern, query);

                        Assert.Matches("\\s+RETURN [A-Za-z0-9]+$", query);

                        break;
                }

                callNo++;
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(_output, Validation, exc => exceptionList.Add(exc))
            {
                UseAlwaysSetResponse = true
            };

            client.SetupAqlResult(InstancesTestHelper.GetProfiles(5));

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            IPaginatedList<IProfile> profile = await adapter.GetProfilesAsync<User, Group, Organization>(
                RequestedProfileKind.User,
                new AssignmentQueryObject
                {
                    Limit = 5,
                    Offset = 2,
                    OrderedBy = nameof(IProfile.Name),
                    SortOrder = SortOrder.Desc,
                    Filter = new Filter
                    {
                        CombinedBy = BinaryOperator.And,
                        Definition = new List<Definitions>
                        {
                            new Definitions
                            {
                                FieldName = nameof(User.FirstName),
                                Values = new[] { "A", "B" },
                                BinaryOperator = BinaryOperator.Or
                            },
                            new Definitions
                            {
                                FieldName = nameof(User.LastName),
                                Values = new[] { "A", "123cbd" },
                                BinaryOperator = BinaryOperator.And
                            },
                            new Definitions
                            {
                                FieldName = nameof(User.Email),
                                Values = new[] { "test@email.com" }
                            }
                        }
                    }
                });

            exceptionList.ForEach(exc => throw exc);

            Assert.Equal(5, profile.Count);
        }

        [Fact]
        public async Task Get_members_of_group()
        {
            var callNo = 0;

            bool Validation(string s)
            {
                string existentProfileQuery =
                    $"FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s*\\(\\s*i0\\.Kind\\s*==\\s*\"Group\"\\s*"
                    + "AND\\s*\\(\\s*i0.Id\\s*==\\s*\"123-456\"\\s*\\)\\)\\s*LIMIT\\s+0,1\\s+";

                var innerQuery =
                    $"\\s*FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s*\\(\\(i0\\.Kind==\"User\"\\s*OR\\s+i0\\.Kind==\"Group\"\\)\\s*AND\\s+\\(COUNT\\(NOT_NULL\\(i0\\.MemberOf,s*\\[\\]\\)\\[\\*\\s+FILTER\\s+\\(\\(CURRENT\\.IsActive\\s+OR\\s+True\\)\\s*AND\\s*\\(CURRENT\\.Id\\s*==\\s*\"123-456\"\\s*\\)\\)\\]\\)\\s*>\\s*0\\s*\\)\\)\\s*RETURN\\s+i0";

                string countingQuery = "^RETURN\\s+\\{\\s*DocumentCount\\s*:\\s*LENGTH\\(\\s*"
                    + innerQuery
                    + "\\s*\\)\\s*\\}$";

                if (callNo == 0)
                {
                    Assert.Matches($"^{existentProfileQuery}RETURN\\s+i0.Id$", s);
                }

                if (callNo == 1)
                {
                    Assert.Matches(countingQuery, s);
                }

                if (callNo == 2)
                {
                    Assert.Matches($"^{innerQuery}$", s);
                }

                return true;
            }

            IEnumerable<object> GetResultObjects(List<IProfile> refVal)
            {
                if (callNo == 0)
                {
                    callNo++;

                    return new List<string>
                    {
                        "123-456"
                    };
                }

                callNo++;

                return refVal;
            }

            var client = new MockedArangoDb(_output, Validation);

            List<IProfile> referenceValues = SampleDataTestHelper.GenerateProfileDataUnrelated(2, 4, 1, 2);
            client.SetupAqlResult(() => GetResultObjects(referenceValues));

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            IPaginatedList<IProfile> children =
                await adapter.GetChildrenOfProfileAsync<UserBasic, GroupBasic, OrganizationBasic>(
                    "123-456",
                    ProfileContainerType.Group);

            Assert.Equal(
                referenceValues.ConvertToBasicTypeForTest(),
                children,
                new TestingEqualityComparerForProfiles(_output));
        }

        [Fact]
        public async Task Get_user_with_specified_id()
        {
            static void Validation(string s, string pId)
            {
                string pattern = $"^FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                    + "FILTER\\(i0\\.Kind\\s*==\\s*\"User\"\\s*AND\\s*\\(i0\\.Id\\s*=="
                    + $"\\s*\"{pId}\"\\)\\s*\\)\\s*"
                    + "LIMIT\\s+0,1\\s+RETURN\\s+i0$";

                Assert.Matches(pattern, s);
            }

            var exceptionList = new List<XunitException>();
            List<UserEntityModel> referenceValue = SampleDataTestHelper.GetUserFaker(false).Generate(1);
            string profileId = referenceValue.First().Id;

            var client = new MockedArangoDb(
                _output,
                query => Validation(query, profileId),
                exc => exceptionList.Add(exc))
            {
                UseAlwaysSetResponse = true
            };

            client.SetupAqlResult(referenceValue);

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            var user = await adapter.GetProfileAsync<UserBasic>(profileId, RequestedProfileKind.User);

            exceptionList.ForEach(exc => throw exc);

            Assert.Equal(
                referenceValue.ConvertToBasicTypeForTest().First(),
                user,
                new TestingEqualityComparerForProfiles(_output));
        }

        [Fact]
        public async Task Get_user_with_specified__group_id_shall_not_work()
        {
            static void Validation(string s)
            {
                string pattern = $"^FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+"
                    + "FILTER\\(i0\\.Kind\\s*==\\s*\"User\"\\)\\s*AND\\s*\\(\\(i0\\.Id\\s*=="
                    + "\\s*\"grp-1\"\\)\\s*\\)\\s*"
                    + "LIMIT\\s+0,1\\s+RETURN\\s+i0$";

                Assert.Matches(pattern, s);
            }

            const string profileId = "grp-1";

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            IServiceCollection serviceCollection =
                new ServiceCollection().AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            exceptionList.ForEach(exc => throw exc);

            await Assert.ThrowsAsync<InstanceNotFoundException>(
                () => adapter.GetProfileAsync<UserBasic>(profileId, RequestedProfileKind.User));
        }

        [Fact]
        public async Task Get_children_of_groups_filtered_by_tags()
        {
            var callNo = 0;
            string referenceId;

            void Validation(string s)
            {
                // first call will be check if profile exists
                if (callNo == 0)
                {
                    Assert.Matches(
                        $"FOR\\s+i0\\s+IN\\s+{"profilesQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s*\\(\\s*i0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\(i0\\.Id==\"{referenceId}\"\\)\\)\\s*LIMIT\\s+0,1\\s+RETURN\\s+i0\\.Id",
                        s);
                }
                else
                {
                    Assert.Matches(
                        $"FILTER\\s*\\(i0\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\(COUNT\\s*\\(NOT_NULL\\(i0\\.MemberOf,\\s*\\[\\]\\)\\[\\*\\s+FILTER\\s+\\(\\(CURRENT\\.IsActive\\s+OR\\s+True\\)\\s+AND\\s+\\(CURRENT\\.Id\\s*==\\s*\"{referenceId}\"\\)\\)\\]\\)\\s*>\\s*0\\)\\s*AND\\s*\\[\"organization\"\\]\\s*ALL\\s+IN\\s+NOT_NULL\\(i0\\.Tags,\\[\\]\\)\\[\\*\\s*RETURN\\s+LOWER\\(CURRENT\\.Name\\)\\]\\)",
                        s);
                }
            }

            IEnumerable<object> GetResultObjects(List<UserEntityModel> refVal)
            {
                if (callNo == 0)
                {
                    callNo++;

                    return new List<string>
                    {
                        referenceId
                    };
                }

                callNo++;

                return refVal;
            }

            List<UserEntityModel> referenceValue = SampleDataTestHelper.GetUserFaker(false).Generate(1);
            var exceptionList = new List<XunitException>();

            referenceId = referenceValue.First().Id;

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc))
            {
                UseAlwaysSetResponse = true
            };

            client.SetupAqlResult(() => GetResultObjects(referenceValue));

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            await adapter.GetChildrenOfProfileAsync<UserBasic, GroupView, OrganizationView>(
                referenceValue.First().Id,
                ProfileContainerType.Group,
                RequestedProfileKind.Group,
                new AssignmentQueryObject
                {
                    TagFilters = new List<string>
                    {
                        "Organization"
                    }
                });

            exceptionList.ForEach(exc => throw exc);
        }

        [Fact]
        public async Task Get_tagged_root_groups()
        {
            static void Validation(string query)
            {
                Assert.Matches(
                    "FILTER\\([a-z][0-9]\\.Kind\\s*==\\s*\"Group\"\\s*AND\\s*\\(COUNT\\(NOT_NULL\\(i0\\.MemberOf,\\[\\]\\)\\[\\* FILTER\\s+\\(\\(CURRENT\\.Kind\\s*==\\s*\"Group\"\\)\\s*OR\\s+\\(CURRENT\\.Kind\\s*==\\s*\"User\"\\)\\)\\]\\)==0\\)\\s*AND\\s*\\[\"test\",\"temp\"]\\s+ALL\\s+IN\\s+NOT_NULL\\([a-z][0-9]\\.Tags,\\[\\]\\)\\[\\*\\s+RETURN\\s+LOWER\\(CURRENT\\.Name\\)]\\)",
                    query);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            await adapter.GetRootProfilesAsync<Group, Organization>(
                RequestedProfileKind.Group,
                new AssignmentQueryObject
                {
                    TagFilters = new List<string>
                    {
                        "test",
                        "temp"
                    }
                });

            exceptionList.ForEach(exc => throw exc);
        }

        [Fact]
        public async Task Get_existent_tags()
        {
            static void Validation(string query)
            {
                Assert.Matches(
                    $"^FOR\\s+t0\\s+IN\\s+{"tagsQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s*\\[\"1\",\"2\",\"3\"\\]\\s*ANY\\s*==\\s*t0\\.Id\\s+RETURN\\s+t0\\.Id$",
                    query);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            client.SetupAqlResult(
                new List<string>
                {
                    "1",
                    "2"
                });

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            var tagList = new List<string>
            {
                "1",
                "2",
                "3"
            };

            IEnumerable<string> foundTags = await adapter.GetExistentTagsAsync(tagList);

            exceptionList.ForEach(exc => throw exc);

            Assert.Equal(
                new List<string>
                {
                    "1",
                    "2"
                },
                foundTags.ToList());
        }

        [Fact]
        public async Task Get_tag_with_id()
        {
            const string tagId = "tag_123";

            static void Validation(string query)
            {
                Assert.Matches(
                    $"FILTER\\s+\\(t0\\.Id\\s*==\\s*\"{tagId}\"\\)\\s*LIMIT\\s+0,1\\s+RETURN\\s+t0",
                    query);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            var reference = new Tag
            {
                Id = tagId,
                Type = TagType.Security,
                Name = "test#tag#1"
            };

            client.SetupAqlResult(new[] { reference });

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            Tag foundTag = await adapter.GetTagAsync(tagId);

            exceptionList.ForEach(exc => throw exc);

            Assert.Equal(reference, foundTag, new TestingEqualityComparerAdvancedForTag(_output));
        }

        [Theory]
        [InlineData(
            RequestedProfileKind.All,
            "name",
            SortOrder.Asc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.Undefined,
            "name",
            SortOrder.Desc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.Undefined,
            "Name",
            SortOrder.Asc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.All,
            "Name",
            SortOrder.Desc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.All,
            "whatever#1",
            SortOrder.Asc,
            null,
            false)]
        [InlineData(
            RequestedProfileKind.All,
            "Id",
            SortOrder.Asc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.Undefined,
            "Id",
            SortOrder.Desc,
            null,
            true)]
        [InlineData(
            RequestedProfileKind.Group,
            "name",
            SortOrder.Asc,
            "\\s+i0\\.Kind==\"Group\"",
            true)]
        [InlineData(
            RequestedProfileKind.Group,
            "name",
            SortOrder.Desc,
            "\\s+i0\\.Kind==\"Group\"",
            true)]
        [InlineData(
            RequestedProfileKind.Group,
            "Name",
            SortOrder.Asc,
            "\\s+i0\\.Kind==\"Group\"",
            true)]
        [InlineData(
            RequestedProfileKind.Group,
            "Name",
            SortOrder.Desc,
            "\\s+i0\\.Kind==\"Group\"",
            true)]
        [InlineData(
            RequestedProfileKind.User,
            "whatever#1",
            SortOrder.Asc,
            "\\s+i0\\.Kind==\"User\"",
            false)]
        [InlineData(
            RequestedProfileKind.User | RequestedProfileKind.Group,
            "Id",
            SortOrder.Asc,
            "\\s*\\(i0\\.Kind==\"User\"\\s*OR\\s+i0\\.Kind==\"Group\"\\)",
            true)]
        [InlineData(
            RequestedProfileKind.User | RequestedProfileKind.Group,
            "Id",
            SortOrder.Desc,
            "\\s*\\(i0\\.Kind==\"User\"\\s*OR\\s+i0\\.Kind==\"Group\"\\)",
            true)]
        public async Task Get_tags_of_profile(
            RequestedProfileKind kind,
            string sortProperty,
            SortOrder sortOrder,
            string filterPattern,
            bool sortActive)
        {
            void Validation(string query)
            {
                var s = "FOR\\s+i0\\s+IN\\s+Service_profilesQuery";

                if (!string.IsNullOrWhiteSpace(filterPattern))
                {
                    s += "\\s+FILTER" + filterPattern;
                }

                if (sortActive)
                {
                    s += $"\\s+SORT\\s+i0\\.{GetPascalCase(sortProperty)}\\s+({sortOrder:G}|{sortOrder.ToString("G").ToUpperInvariant()})";
                }

                s += "\\s+RETURN\\s+i0";

                Assert.Matches(s, query);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            await adapter.GetAllProfilesAsync(kind, sortProperty, sortOrder);

            exceptionList.ForEach(exc => throw exc);
        }

        [Fact]
        public async Task Get_Function_with_filled_linked_profiles()
        {
            const string functionId = "func_no_1";

            void Validation(string query)
            {
                Assert.Matches(
                    $"^FOR\\s+f0\\s+IN\\s+{"rolesFunctionsQuery".GetDefaultCollectionNameInTest()}\\s+FILTER\\s+f0\\.Type\\s*==\\s*\"Function\"\\s*"
                    + $"AND\\s*\\(f0\\.Id==\"{functionId}\"\\)\\s+"
                    + "LIMIT\\s+0,1\\s+RETURN\\s+f0$",
                    query);
            }

            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            FunctionObjectEntityModel referenceFunction = SampleDataTestHelper.GetSampleFunctionEntityModel(functionId);

            client.SetupAqlResult(referenceFunction.AsList());

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            FunctionView function = null;

            try
            {
                function = await adapter.GetFunctionAsync<FunctionView>(functionId);
            }
            catch (Exception e)
            {
                exceptionList.Add(
                    new XunitException(
                        $"Exception of type '{e.GetType()}' was thrown during GetFunctionAsync(). {e.Message}"));
            }

            exceptionList.ForEach(exc => throw exc);
            Assert.NotNull(function);
            var refFuncConv = SampleDataTestHelper.GetDefaultMapper().Map<FunctionView>(referenceFunction);

            Assert.Equal(
                refFuncConv,
                function,
                new TestingEqualityComparerForFunctionView(_output));
        }

        [Fact]
        public async Task Get_all_active_user_assignments_should_work()
        {
            var validationCallNumber = 0;

            void Validation(string query)
            {
                if (validationCallNumber++ == 0)
                {
                    return;
                }

                query.Should()
                     .NotBeNullOrWhiteSpace()
                     .And.MatchRegex(
                         new Regex(
                             "^FOR\\s+\\w\\d\\s+IN\\s+[\\w_]+\\s+FILTER\\s+\\(?\\s*[\\w\\.\\d]+\\s*==\\s*\"123\"\\s*\\)?\\s*LIMIT\\s+0,1\\s+RETURN \\w\\d\\.ActiveMemberships$"));
            }

            var creationCallNumber = 0;

            // arrange
            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            client.SetupAqlResult(_ => GetAssignmentsResponse(creationCallNumber++, o => o.ActiveMemberships));

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            // act
            IList<ObjectIdent> response =
                await adapter.GetAllAssignedIdsOfUserAsync("123", false, CancellationToken.None);

            // assert
            exceptionList.ForEach(e => throw e);

            response.Should().BeEquivalentTo(GetAssignments().First().ActiveMemberships);
        }
        
        [Fact]
        public async Task Get_all_user_assignments_including_inactive_should_work()
        {
            var validationCallNumber = 0;

            void Validation(string query)
            {
                if (validationCallNumber++ == 0)
                {
                    return;
                }

                query.Should()
                     .NotBeNullOrWhiteSpace()
                     .And.MatchRegex(
                         new Regex(
                             "^FOR\\s+\\w\\d\\s+IN\\s+[\\w_]+\\s+FILTER\\s+\\(?\\s*[\\w\\.\\d]+\\s*==\\s*\"123\"\\s*\\)?\\s*LIMIT\\s+0,1\\s+RETURN \\w\\d\\.Assignments"));
            }

            var creationCallNumber = 0;

            // arrange
            var exceptionList = new List<XunitException>();

            var client = new MockedArangoDb(
                _output,
                Validation,
                exc => exceptionList.Add(exc));

            client.SetupAqlResult(_ => GetAssignmentsResponse(creationCallNumber++, o => o.Assignments));

            IServiceCollection services = new ServiceCollection()
                .AddScoped(_ => GetClientFactory(client.HandleMessage, _output));

            ServiceProvider serviceProvider = services.BuildServiceProvider();

            IReadService adapter = new ArangoReadService(
                serviceProvider,
                new MockArangoDbInitializer(),
                _loggerFactory.CreateLogger<ArangoReadService>());

            // act
            IList<ObjectIdent> response =
                await adapter.GetAllAssignedIdsOfUserAsync("123", true, CancellationToken.None);

            // assert
            exceptionList.ForEach(e => throw e);

            response.Should().BeEquivalentTo(GetAssignments().First().Assignments.Select(a => a.Parent));
        }

        public static IEnumerable<object[]> GetUserSettingsTestArguments()
        {
            yield return new object[] { SpecificTestData.ProfileSettings.UserId, ProfileKind.User, true };
            yield return new object[] { SpecificTestData.ProfileSettings.UserId, ProfileKind.User, false };
        }

        private static IEnumerable<object> GetAssignmentsResponse(
            int callNumber,
            Func<SecondLevelProjectionAssignmentsUser, object> propertySelector)
        {
            return callNumber == 0
                ? new List<object> { "userId" }
                : GetAssignments().Select(propertySelector).ToList();
        }

        private static IList<SecondLevelProjectionAssignmentsUser> GetAssignments()
        {
            const string activeGroupId = "grp-456";
            const string userId = "123";
            const string inactiveGroupId = "grp-789";

            return new SecondLevelProjectionAssignmentsUser
                   {
                       ActiveMemberships = new ObjectIdent(activeGroupId, ObjectType.Group).AsList(),
                       ProfileId = userId,
                       Assignments = new List<SecondLevelProjectionAssignment>
                                     {
                                         new SecondLevelProjectionAssignment
                                         {
                                             Parent = new ObjectIdent(activeGroupId, ObjectType.Group),
                                             Profile = new ObjectIdent(userId, ObjectType.User),
                                             Conditions = EmptyConditions
                                         },
                                         new SecondLevelProjectionAssignment
                                         {
                                             Parent = new ObjectIdent(inactiveGroupId, ObjectType.Group),
                                             Profile = new ObjectIdent(userId, ObjectType.User),
                                             Conditions = new List<RangeCondition>
                                                          {
                                                              new RangeCondition(
                                                                  DateTime.UtcNow.AddMonths(3),
                                                                  null)
                                                          }
                                         }
                                     },
                       Containers = new List<ISecondLevelAssignmentContainer>
                                    {
                                        new SecondLevelAssignmentContainer
                                        {
                                            ContainerType = ContainerType.Group,
                                            Id = activeGroupId,
                                            Name = "active group"
                                        },
                                        new SecondLevelAssignmentContainer
                                        {
                                            ContainerType = ContainerType.Group,
                                            Id = inactiveGroupId,
                                            Name = "inactive group"
                                        }
                                    }
                   }
                .AsList();
        }

        private static List<RangeCondition> EmptyConditions =>
            new List<RangeCondition>
            {
                new RangeCondition(null, null)
            };
    }
}
