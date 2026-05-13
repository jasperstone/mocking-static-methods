using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
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

            // Setup retryBlock to return 503 for first 2 calls, then 200 OK
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                HttpResponseMessage response;
                if (callCount <= 2)
                {
                    response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                }
                else
                {
                    response = new HttpResponseMessage(HttpStatusCode.OK);
                }
                return Task.FromResult(response);
            };

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 5);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, callCount);

            // Verify LogWarning called with retry count message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(3));

            // Verify LogWarning called with "Retrying a service unavailable error."
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            var retryBlock = new Mock<Func<Task<HttpResponseMessage>>>();

            var ex = await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock.Object, loggerMock.Object, cts.Token));

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

        [Fact]
        public async Task RetryRequest_LogsWarningAndRetriesOnHttpRequestException()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new HttpRequestException("Request failed");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 5);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, callCount);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request : Request failed.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ThrowsAndLogsError_WhenRetryLimitExceeded()
        {
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                throw new HttpRequestException("Request failed");
            };

            var ex = await Assert.ThrowsAsync<HttpRequestException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3));

            Assert.Equal(3, callCount);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    0,
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<HttpRequestException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
