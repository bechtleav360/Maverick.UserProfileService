using System;
using System.Linq;
using FluentAssertions;
using UserProfileService.Sync.Abstraction.Models;
using UserProfileService.Sync.Abstraction.Models.Entities;
using UserProfileService.Sync.Converter;
using Xunit;

namespace UserProfileService.Sync.UnitTests.Converter
{
    public class PrefixConverterTests
    {
        [Fact]
        public void ConvertOperationTest()
        {
            var model = new UserSync
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Tester",
                Source = "Tests",
                ExternalIds = new[]
                {
                    new KeyProperties(
                        Guid.NewGuid().ToString(),
                        "Tests")
                }.ToList()
            };

            const string prefix = "KK_";
            string expectedExternalId = prefix + model.ExternalIds.FirstOrDefault()?.Id;
            string expectedSource = model.ExternalIds.FirstOrDefault()?.Source;
            model.ExternalIds.Add(new KeyProperties(expectedExternalId, expectedSource));

            var expected = new UserSync
            {
                Id = model.Id,
                Name = model.Name,
                Source = model.Source,
                ExternalIds = model.ExternalIds
            };

            var converter = new PrefixConverter<ISyncModel>(prefix);

            ISyncModel result = converter.Convert(model);
            result.Should().NotBeNull().And.BeEquivalentTo(model);
        }
    }
}
