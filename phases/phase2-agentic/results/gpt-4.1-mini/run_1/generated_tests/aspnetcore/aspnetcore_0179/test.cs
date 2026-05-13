using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningOnEachRetryAndRetriesOn503()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    // Return 503 ServiceUnavailable on first call to trigger retry
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                // Return 200 OK on second call
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify LogWarning called with retry count message at least once
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);

            // Verify LogWarning called with "Retrying a service unavailable error."
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var retryBlock = new Func<Task<HttpResponseMessage>>(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

            var ex = await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));

            Assert.Contains("Failed to connect, retry canceled.", ex.Message);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry canceled.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
