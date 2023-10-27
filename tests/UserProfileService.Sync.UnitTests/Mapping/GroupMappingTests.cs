using System.Linq;
using AutoFixture.Xunit2;
using AutoMapper;
using Maverick.UserProfileService.Models.Models;
using FluentAssertions;
using Maverick.UserProfileService.AggregateEvents.Resolved.V1;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Utilities;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Mapping
{
    public class GroupMappingTests
    {
        [Theory]
        [AutoData]
        public void map_to_group_sync_should_work(GroupCreated groupCreated)
        {
            // arrange
            var mapper = new Mapper(new MapperConfiguration(conf => conf.AddProfile(new MappingProfiles())));

            // Act
            var resolvedGroup = mapper.Map<GroupSync>(groupCreated);

            // Assert
            resolvedGroup.Should().NotBeNull();

            resolvedGroup.Should()
                         .BeEquivalentTo(
                             new GroupSync
                                 {
                                     Id = groupCreated.Id,
                                     Name = groupCreated.Name,
                                     DisplayName = groupCreated.DisplayName,
                                     IsSystem = groupCreated.IsSystem,
                                     Source = groupCreated.Source,
                                     Weight = groupCreated.Weight,
                                     Tags = groupCreated.Tags.Select(t => new CalculatedTag
                                                                          {
                                                                              Id = t.TagDetails.Id,
                                                                              Name = t.TagDetails.Name,
                                                                          }).ToList(),
                                     ExternalIds = groupCreated.ExternalIds
                                                               .Select(
                                                                   e => new Abstraction.Models.KeyProperties(
                                                                       e.Id,
                                                                       e.Source,
                                                                       null,
                                                                       e.IsConverted))
                                                               .ToList()
                                 });

        }
    }
}
