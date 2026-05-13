using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningWhenHttpRequestExceptionOccurs()
        {
            var logger = new TestLogger();
            using var response = new HttpResponseMessage(HttpStatusCode.OK);
            var attempt = 0;

            var result = await RetryHelper.RetryRequest(
                () =>
                {
                    attempt++;
                    if (attempt == 1)
                    {
                        throw new HttpRequestException("boom");
                    }

                    return Task.FromResult(response);
                },
                logger,
                retryCount: 2);

            Assert.Same(response, result);
            Assert.Contains(logger.Entries, entry =>
                entry.LogLevel == LogLevel.Warning &&
                entry.Message == "Failed to complete the request : boom.");
        }

        [Fact]
        public async Task RetryRequest_DoesNotLogSpecificWarningForNonHttpOrWebException()
        {
            var logger = new TestLogger();
            using var response = new HttpResponseMessage(HttpStatusCode.OK);
            var attempt = 0;

            var result = await RetryHelper.RetryRequest(
                () =>
                {
                    attempt++;
                    if (attempt == 1)
                    {
                        throw new InvalidOperationException("boom");
                    }

                    return Task.FromResult(response);
                },
                logger,
                retryCount: 2);

            Assert.Same(response, result);
            Assert.DoesNotContain(logger.Entries, entry =>
                entry.LogLevel == LogLevel.Warning &&
                entry.Message == "Failed to complete the request : boom.");
        }

        private sealed class TestLogger : ILogger
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, formatter(state, exception), exception));
            }
        }

        private sealed record LogEntry(LogLevel LogLevel, string Message, Exception? Exception);

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose()
            {
            }
        }
    }
}
