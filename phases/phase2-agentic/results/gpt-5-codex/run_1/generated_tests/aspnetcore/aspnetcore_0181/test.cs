using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequestAsync_LogsWarningWhenHttpRequestExceptionOccurs()
        {
            // Arrange
            var sink = new TestSink();
            var logger = new TestLogger("TestLogger", sink, enabled: true);
            var retryCount = 2;
            var attempts = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                attempts++;
                throw new HttpRequestException("network failure");
            };

            // Act
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                RetryHelper.RetryRequest(async () => await retryBlock(), logger, retryCount, CancellationToken.None));

            // Assert
            Assert.Equal(retryCount, attempts);
            var warnings = sink.Writes.Where(w => w.LogLevel == LogLevel.Warning).ToList();
            Assert.Contains(warnings, write => write.State.ToString() == "Retry count 1..");
            Assert.Contains(warnings, write => write.State.ToString() == "Failed to complete the request : network failure.");
        }

        [Fact]
        public async Task RetryRequestAsync_LogsWarningAndRetriesOnServiceUnavailable()
        {
            // Arrange
            var sink = new TestSink();
            var logger = new TestLogger("TestLogger", sink, enabled: true);
            var attempts = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                attempts++;
                if (attempts == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }

                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(async () => await retryBlock(), logger, cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, attempts);
            Assert.Contains(sink.Writes, write => write.LogLevel == LogLevel.Warning && write.State.ToString() == "Retrying a service unavailable error.");
        }
    }
}
