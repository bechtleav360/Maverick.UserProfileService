using Microsoft.Extensions.Logging;
using UserProfileService.Common.Logging.Extensions;
using UserProfileService.Common.Logging.UnitTests.TestData;
using Xunit;

namespace UserProfileService.Common.Logging.UnitTests.Tests
{
    public class LoggingExtensionsTests
    {
        [Theory]
        [ClassData(typeof(LoggingExtensionsTestData.IsEnabledTests))]
        public void IsEnabledFor_shouldWork(
            ILogger logger,
            LogLevel logLevel,
            bool enabledExpected)
        {
            Assert.Equal(enabledExpected, logger.IsEnabledFor(logLevel));
        }

        [Theory]
        [ClassData(typeof(LoggingExtensionsTestData.IsEnabledForTraceTests))]
        public void IsEnabledForTrace_shouldWork(
            ILogger logger,
            bool enabledExpected)
        {
            Assert.Equal(enabledExpected, logger.IsEnabledForTrace());
        }
    }
}
