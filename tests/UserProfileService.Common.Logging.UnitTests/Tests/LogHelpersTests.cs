using UserProfileService.Common.Logging.UnitTests.TestData;
using Xunit;

namespace UserProfileService.Common.Logging.UnitTests.Tests
{
    public class LogHelpersTests
    {
        [Theory]
        [ClassData(typeof(LogHelpersTestData.LogStringTests))]
        public void ToLogStringTests_shouldWork(object input, string expected)
        {
            Assert.Equal(expected, input.ToLogString());
        }

        [Theory]
        [ClassData(typeof(LogHelpersTestData.AsArgumentListTests))]
        public void AsArgumentListTests_shouldWork(object input)
        {
            Assert.Equal(new[] { input }, input.AsArgumentList());
        }
    }
}
