using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Moq;

namespace UserProfileService.Common.Logging.Tests.TestData
{
    public class LoggingExtensionsTestData
    {
        private static readonly ILogger _NullLogger = null;

        private static readonly LogLevel[] _LogLevels =
        {
            LogLevel.Critical,
            LogLevel.Error,
            LogLevel.Information,
            LogLevel.Warning,
            LogLevel.Debug,
            LogLevel.Trace
        };

        private static ILogger GetLogger(
            LogLevel logLevel,
            bool enabled)
        {
            var mock = new Mock<ILogger>();

            mock.Setup(m => m.IsEnabled(It.IsAny<LogLevel>()))
                .Returns((LogLevel l) => l == logLevel && enabled);

            return mock.Object;
        }

        public class IsEnabledForTraceTests : IEnumerable<object[]>
        {
            private static readonly List<object[]> _Data = new List<object[]>
            {
                new object[] { GetLogger(LogLevel.Trace, true), true },
                new object[] { GetLogger(LogLevel.Information, true), false }
            };

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                return _Data.GetEnumerator();
            }
        }

        public class IsEnabledTests : IEnumerable<object[]>
        {
            private static IEnumerable<object[]> GetLoggerData(LogLevel level)
            {
                yield return new object[] { GetLogger(level, false), level, false };
                yield return new object[] { GetLogger(level, true), level, true };
            }

            private static IEnumerable<object[]> GetMixedData()
            {
                return _LogLevels.SelectMany(GetLoggerData)
                    .Append(new object[] { _NullLogger, LogLevel.Critical, false });
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<object[]> GetEnumerator()
            {
                return GetMixedData().GetEnumerator();
            }
        }
    }
}
