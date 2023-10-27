using System.Threading.Tasks;
using FluentAssertions;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;
using Xunit.Abstractions;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class TagReadTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;
        private readonly ITestOutputHelper _output;

        public TagReadTests(FirstLevelProjectionFixture fixture, ITestOutputHelper outputHelper)
        {
            _fixture = fixture;
            _output = outputHelper;
        }

        [Fact]
        public async Task Get_tag_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            FirstLevelProjectionTag tag = await repo.GetTagAsync(TagReadTestData.ReadTag.Id);

            tag.Should().BeEquivalentTo(TagReadTestData.ReadTag);
        }

        [Fact]
        public async Task Get_not_existing_tag_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.GetTagAsync("this-tag-does-not-exists"));
        }
    }
}
