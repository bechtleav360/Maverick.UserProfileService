using System;
using System.Threading.Tasks;
using FluentAssertions;
using Maverick.UserProfileService.Models.EnumModels;
using UserProfileService.Arango.IntegrationTests.V2.Abstractions;
using UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository.Data;
using UserProfileService.Arango.IntegrationTests.V2.Fixtures;
using UserProfileService.Common.V2.Exceptions;
using UserProfileService.Projection.Abstractions;
using UserProfileService.Projection.Abstractions.Models;
using Xunit;

namespace UserProfileService.Arango.IntegrationTests.V2.FirstLevelProjectionRepository
{
    [Collection(nameof(FirstLevelProjectionCollection))]
    public class TagWriteTests : ArangoFirstLevelRepoTestBase
    {
        private readonly FirstLevelProjectionFixture _fixture;

        public TagWriteTests(FirstLevelProjectionFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Create_tag_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            var tag = new FirstLevelProjectionTag
            {
                Id = Guid.NewGuid().ToString("D"),
                Name = "Create-Tag",
                Type = TagType.Custom
            };

            await repo.CreateTag(tag);

            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(tag.Id);
            dbTag.Should().BeEquivalentTo(tag);
        }

        [Fact]
        public async Task Delete_tag_should_work()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();
            await repo.DeleteTagAsync(TagWriteTestData.DeleteTag.TagId);

            var dbTag = await GetDocumentObjectAsync<FirstLevelProjectionTag>(
                RoleWriteTestData.DeleteRole.TargetRoleId,
                false);

            Assert.Null(dbTag);
            //TODO check edges
        }

        [Fact]
        public async Task Delete_not_existing_tag_should_throw()
        {
            IFirstLevelProjectionRepository repo = await _fixture.GetFirstLevelRepository();

            await Assert.ThrowsAsync<InstanceNotFoundException>(() => repo.DeleteTagAsync("this-tag-does-not-exist"));
        }
    }
}
