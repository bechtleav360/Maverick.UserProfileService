using System.Collections.Generic;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.ReadService
{
    [Collection(nameof(DatabaseCollection))]
    public class PermissionTests : ReadTestBase
    {
        public PermissionTests(
            DatabaseFixture fixture,
            ITestOutputHelper output) : base(fixture, output)
        {
        }
        //[Theory,
        // MemberData(nameof(GetPermissionOfUserTestArguments))]
        //public async Task GetPermissionOfUserShouldWork(string profileId,
        //                                                string secOId)
        //{
        //    IReadService service = await Fixture.GetReadServiceAsync();

        //    await Assert.ThrowsAsync<NotImplementedException>(
        //        () =>
        //            service.GetPermissionsOfSecOProfileAsync(profileId, new[] {secOId}));
        //}

        public static IEnumerable<object[]> GetPermissionOfUserTestArguments()
        {
            yield return new object[] { "id", "id" };
        }
    }
}
