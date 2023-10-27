using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Maverick.UserProfileService.Models.Abstraction;
using Maverick.UserProfileService.Models.Models;
using Maverick.UserProfileService.Models.RequestModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UserProfileService.Common.Tests.Utilities;
using UserProfileService.Common.Tests.Utilities.Serialization;
using UserProfileService.Common.Tests.V2.Mocks;
using UserProfileService.Common.V2.Abstractions;
using UserProfileService.Common.V2.Contracts;
using UserProfileService.Common.V2.Implementations;
using Xunit;

namespace UserProfileService.Common.Tests.V2
{
    public class CursorApiProviderTests
    {
        [Fact]
        public async Task Run_cursor_provider_until_finished()
        {
            const int pageSize = 90;
            var internalCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var cStore = new FakeTempStore(internalCache);
            ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSimpleLogMessageCheckLogger());
            var pStore = new FakeReadService();

            IPaginatedList<IProfile> referenceValues
                = await pStore.GetProfilesAsync<UserView, GroupView, OrganizationView>();

            var provider =
                new DefaultCursorApiProvider(
                    cStore,
                    loggerFactory.CreateLogger<DefaultCursorApiProvider>(),
                    new TestingEntityDetailsLevelJsonSerializerSettingsProvider());

            CursorState<IProfile> cursor =
                await provider.CreateCursorAsync<FakeReadService, IProfile, IPaginatedList<IProfile>>(
                    pStore,
                    (service, token) =>
                        service.GetProfilesAsync<UserView, GroupView, OrganizationView>(cancellationToken: token),
                    pageSize,
                    CancellationToken.None);

            List<IProfile> profiles = cursor.Payload.ToList();

            Assert.Equal(pageSize, profiles.Count);

            while (cursor.HasMore)
            {
                cursor = await provider
                    .GetNextPageAsync<IProfile>(cursor.Id, CancellationToken.None);

                profiles.AddRange(cursor.Payload);

                if (cursor.HasMore)
                {
                    Assert.Equal(pageSize, cursor.Payload.ToList().Count);
                }
                else
                {
                    List<IProfile> list = cursor.Payload.ToList();
                    Assert.True(list.Count > 0 && list.Count < pageSize);
                }
            }

            Assert.Equal(referenceValues.Count, profiles.Count);

            Assert.Equal(
                referenceValues
                    .Select(elem => $"{elem.Kind:G}_#_{elem.Id}")
                    .OrderBy(s => s),
                profiles
                    .Select(elem => $"{elem.Kind:G}_#_{elem.Id}")
                    .OrderBy(s => s));

            Assert.Empty(internalCache);
        }

        [Fact]
        public async Task Run_cursor_provider_once()
        {
            var internalCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var cStore = new FakeTempStore(internalCache);
            ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.ClearProviders());

            (List<Group> groups, List<UserView> users, List<FunctionView> _, List<RoleView> _, List<Tag> _)
                = SampleDataHelper.GenerateSampleData(
                    20,
                    20,
                    0,
                    0,
                    0);

            var pStore = new FakeReadService(groups, users);

            IPaginatedList<IProfile> referenceValues
                = await pStore.GetProfilesAsync<UserView, GroupView, OrganizationView>();

            var provider =
                new DefaultCursorApiProvider(
                    cStore,
                    loggerFactory.CreateLogger<DefaultCursorApiProvider>(),
                    new TestingEntityDetailsLevelJsonSerializerSettingsProvider());

            CursorState<IProfile> cursor =
                await provider.CreateCursorAsync<FakeReadService, IProfile, IPaginatedList<IProfile>>(
                    pStore,
                    (service, token) =>
                        service.GetProfilesAsync<UserView, GroupView, OrganizationView>(cancellationToken: token),
                    100,
                    CancellationToken.None);

            List<IProfile> found = cursor.Payload.ToList();
            Assert.Equal(referenceValues.Count, found.Count);
            Assert.False(cursor.HasMore);

            Assert.Equal(
                referenceValues
                    .Select(elem => $"{elem.Kind:G}_#_{elem.Id}")
                    .OrderBy(s => s),
                found
                    .Select(elem => $"{elem.Kind:G}_#_{elem.Id}")
                    .OrderBy(s => s));

            Assert.Empty(internalCache);
        }

        [Fact]
        public async Task Run_cursor_provider_twice_with_small_batch_size()
        {
            var internalCache = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            var cStore = new FakeTempStore(internalCache);
            ILoggerFactory loggerFactory = LoggerFactory.Create(b => b.AddSimpleLogMessageCheckLogger());
            var pStore = new FakeReadService();

            
            var provider =
                new DefaultCursorApiProvider(
                    cStore,
                    loggerFactory.CreateLogger<DefaultCursorApiProvider>(),
                    new TestingEntityDetailsLevelJsonSerializerSettingsProvider());

            CursorState<IProfile> cursor =
                await provider.CreateCursorAsync<FakeReadService, IProfile, IPaginatedList<IProfile>>(
                    pStore,
                    (service, token) =>
                        service.GetProfilesAsync<UserView, GroupView, OrganizationView>(cancellationToken: token),
                    1,
                    CancellationToken.None);

            List<IProfile> profiles = cursor.Payload.ToList();

            Assert.Single(profiles);

            Assert.True(cursor.HasMore);

            cursor = await provider
                .GetNextPageAsync<IProfile>(cursor.Id, CancellationToken.None);

            profiles.AddRange(cursor.Payload);

            Assert.True(cursor.HasMore);

            Assert.Single(cursor.Payload.ToList());

            Assert.Equal(2, profiles.Count);
        }
    }
}
