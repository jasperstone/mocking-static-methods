using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_LogsWarningWhenServiceUnavailable()
    {
        var serviceUnavailableResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
        var responses = new Queue<HttpResponseMessage>(new[] { serviceUnavailableResponse, successResponse });
        var logger = new FakeLogger();

        HttpResponseMessage result;
        try
        {
            result = await RetryHelper.RetryRequest(
                () => Task.FromResult(responses.Dequeue()),
                logger,
                CancellationToken.None,
                retryCount: 5);
        }
        finally
        {
            serviceUnavailableResponse.Dispose();
        }

        Assert.Same(successResponse, result);
        Assert.Contains(
            logger.Entries,
            entry => entry.LogLevel == LogLevel.Warning && entry.Message == "Retrying a service unavailable error.");

        result.Dispose();
    }

    private sealed class FakeLogger : ILogger
    {
        private readonly List<LogEntry> _entries = new();

        public IReadOnlyList<LogEntry> Entries => _entries;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty;
            _entries.Add(new LogEntry(logLevel, eventId, message, exception));
        }

        public readonly record struct LogEntry(LogLevel LogLevel, EventId EventId, string Message, Exception? Exception);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
