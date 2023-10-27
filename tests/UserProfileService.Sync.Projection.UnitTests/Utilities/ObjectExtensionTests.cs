using System.Collections.Generic;
using AutoFixture.Xunit2;
using FluentAssertions;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Common.Extensions;
using Xunit;

namespace UserProfileService.Sync.Projection.UnitTests.Utilities
{
    public class ObjectExtensionTests
    {
        [Theory]
        [AutoData]
        public void UpdatePropertiesShouldWork(string domainName, UserSync user)
        {
            var propertiesChanged = new Dictionary<string,object>(){{nameof(UserSync.Domain),domainName}};
            user.UpdateProperties(propertiesChanged);

            user.Domain.Should().BeEquivalentTo(domainName);
        }
    }
}
