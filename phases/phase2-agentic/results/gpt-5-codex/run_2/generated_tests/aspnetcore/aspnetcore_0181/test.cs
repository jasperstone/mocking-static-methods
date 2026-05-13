using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningWhenHttpRequestExceptionOccursBeforeSuccessfulRetry()
        {
            // Arrange
            var logger = new TestLogger();
            var attempt = 0;

            async Task<HttpResponseMessage> RetryBlock()
            {
                attempt++;
                if (attempt == 1)
                {
                    throw new HttpRequestException("Test error");
                }

                return new HttpResponseMessage(HttpStatusCode.OK);
            }

            // Act
            var response = await RetryHelper.RetryRequest(RetryBlock, logger, retryCount: 2);

            // Assert
            Assert.Equal(2, attempt);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains(logger.Entries, entry =>
                entry.Level == LogLevel.Warning &&
                entry.Message == "Failed to complete the request : Test error.");
        }

        private sealed class TestLogger : ILogger
        {
            public IList<LogEntry> Entries { get; } = new List<LogEntry>();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, eventId, message, exception));
            }

            public readonly record struct LogEntry(LogLevel Level, EventId EventId, string Message, Exception Exception);

            private sealed class NullScope : IDisposable
            {
                public static NullScope Instance { get; } = new NullScope();

                public void Dispose()
                {
                }
            }
        }
    }
}
