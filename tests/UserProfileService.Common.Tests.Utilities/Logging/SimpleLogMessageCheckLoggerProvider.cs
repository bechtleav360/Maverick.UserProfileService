using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace UserProfileService.Common.Tests.Utilities.Logging
{
    public class SimpleLogMessageCheckLoggerProvider : ILoggerProvider
    {
        private readonly Action<Exception, LogLevel> _exceptionHandling;

        private readonly ConcurrentDictionary<string, SimpleLogMessageCheckLogger> _storedLogger
            = new ConcurrentDictionary<string, SimpleLogMessageCheckLogger>(StringComparer.OrdinalIgnoreCase);

        private readonly bool _throwOnError;

        public SimpleLogMessageCheckLoggerProvider(bool throwOnError)
        {
            _throwOnError = throwOnError;
        }

        public SimpleLogMessageCheckLoggerProvider(
            Action<Exception, LogLevel> exceptionHandling,
            bool throwOnError = false)
        {
            _exceptionHandling = exceptionHandling;
            _throwOnError = throwOnError;
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (_exceptionHandling != null)
            {
                return _storedLogger.GetOrAdd(
                    categoryName,
                    key => new SimpleLogMessageCheckLogger(key, _exceptionHandling, _throwOnError));
            }

            return _storedLogger.GetOrAdd(categoryName, key => new SimpleLogMessageCheckLogger(key, _throwOnError));
        }
    }
}
