using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Sdk;

namespace UserProfileService.Common.Tests.Utilities.Logging
{
    public class SimpleLogMessageCheckLogger : ILogger
    {
        private readonly Action<Exception, LogLevel> _furtherExceptionHandling;
        private readonly bool _throwOnError;
        public string Category { get; }

        public SimpleLogMessageCheckLogger(string categoryName, bool throwOnError)
        {
            _throwOnError = throwOnError;
            Category = categoryName;

            Assert.False(
                string.IsNullOrWhiteSpace(Category),
                "Category name should not be empty string or whitespace.");
        }

        public SimpleLogMessageCheckLogger(
            string categoryName,
            Action<Exception, LogLevel> furtherExceptionHandling,
            bool throwOnError = false)
            : this(categoryName, throwOnError)
        {
            _furtherExceptionHandling = furtherExceptionHandling;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception exception,
            Func<TState, Exception, string> formatter)
        {
            string message;

            try
            {
                message = formatter.Invoke(state, exception);
            }
            catch (Exception e)
            {
                throw new XunitException(
                    $"Logger {Category}: Error occurred during generating log message. Message {e.Message}{Environment.NewLine}{e}");
            }

            Assert.False(
                string.IsNullOrWhiteSpace(message),
                $"Logger {Category}: Log message should not be empty or whitespace!");

            if (_furtherExceptionHandling != null
                && exception != null
                && (logLevel == LogLevel.Error
                    || logLevel == LogLevel.Warning
                    || logLevel == LogLevel.Critical))
            {
                _furtherExceptionHandling.Invoke(exception, logLevel);

                return;
            }

            if (_throwOnError && (logLevel == LogLevel.Error || logLevel == LogLevel.Critical))
            {
                throw new XunitException(
                    $"Logger {Category}: Captured error log message (level: {logLevel:G}). Message: {message}{Environment.NewLine}-->{Environment.NewLine}{exception}");
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoopDisposable.Instance;
        }

        private class NoopDisposable : IDisposable
        {
            public static NoopDisposable Instance => new NoopDisposable();

            public void Dispose()
            {
            }
        }
    }
}
