using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        private class TestLogger : ILogger
        {
            public List<string> LoggedMessages = new List<string>();
            public List<LogLevel> LogLevels = new List<LogLevel>();

            public IDisposable BeginScope<TState>(TState state) => null!;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (formatter != null)
                {
                    LoggedMessages.Add(formatter(state, exception));
                    LogLevels.Add(logLevel);
                }
            }
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnEachRetryCount()
        {
            var logger = new TestLogger();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    return Task.FromResult(response);
                }
                else
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    return Task.FromResult(response);
                }
            };

            var cancellationToken = CancellationToken.None;

            var response = await RetryHelper.RetryRequest(retryBlock, logger, cancellationToken, retryCount: 5);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Check that "Retry count" warning was logged at least twice
            int retryCountWarnings = 0;
            foreach (var msg in logger.LoggedMessages)
            {
                if (msg.Contains("Retry count"))
                {
                    retryCountWarnings++;
                }
            }
            Assert.True(retryCountWarnings >= 2);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnFailedRequestException()
        {
            var logger = new TestLogger();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Request failed");
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            var cancellationToken = CancellationToken.None;

            var response = await RetryHelper.RetryRequest(retryBlock, logger, cancellationToken, retryCount: 3);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Check that the warning about failed request was logged once
            bool foundFailedRequestWarning = false;
            foreach (var msg in logger.LoggedMessages)
            {
                if (msg.Contains("Failed to complete the request"))
                {
                    foundFailedRequestWarning = true;
                    break;
                }
            }
            Assert.True(foundFailedRequestWarning);
        }
    }
}
