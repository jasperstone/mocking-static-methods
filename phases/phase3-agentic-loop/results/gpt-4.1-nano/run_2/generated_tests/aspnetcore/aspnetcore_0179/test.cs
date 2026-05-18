using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace RetryHelperTests
{
    public class RetryRequestTests
    {
        [Fact]
        public async Task RetryRequest_Should_LogWarning_ForEachRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                var response = new HttpResponseMessage(HttpStatusCode.OK);
                return Task.FromResult(response);
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(x => x.LogWarning("Retry count {retryCount}..", It.IsAny<object>()), Times.Exactly(1));
        }

        [Fact]
        public async Task RetryRequest_Should_LogWarning_When_ServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Verify that LogWarning for "Retrying a service unavailable error." was called
            loggerMock.Verify(x => x.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogInformation_When_CancellationRequested()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task<HttpResponseMessage>> retryBlock = () => throw new Exception();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));
            loggerMock.Verify(x => x.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogError_When_LastRetryFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var exception = new HttpRequestException("Request failed");
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                throw exception;
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2));
            loggerMock.Verify(x => x.LogError(0, exception, "Failed to connect, retry limit exceeded."), Times.Once);
        }
    }
}
