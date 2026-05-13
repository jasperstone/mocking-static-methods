using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_Should_Log_Warning_On_HttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            var exceptionMessage = "Network error";

            Func<Task<HttpResponseMessage>> failingRetryBlock = () =>
            {
                callCount++;
                throw new HttpRequestException(exceptionMessage);
            };

            // Act
            await RetryHelper.RetryRequest(failingRetryBlock, loggerMock.Object, retryCount: 2);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Failed to complete the request : {0}.", exceptionMessage),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Warning_On_WebException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            var exceptionMessage = "Web error";

            Func<Task<HttpResponseMessage>> failingRetryBlock = () =>
            {
                callCount++;
                throw new WebException(exceptionMessage);
            };

            // Act
            await RetryHelper.RetryRequest(failingRetryBlock, loggerMock.Object, retryCount: 2);

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Failed to complete the request : {0}.", exceptionMessage),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Warning_On_ServiceUnavailableResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var responses = new[]
            {
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                new HttpResponseMessage(HttpStatusCode.OK)
            };
            int callIndex = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                var response = responses[callIndex];
                callIndex++;
                return Task.FromResult(response);
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(x => x.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_Throw_OperationCanceledException_When_CancellationRequested()
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
        public async Task RetryRequest_Should_Throw_When_RetryLimitExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> failingRetryBlock = () => throw new Exception("fail");
            int retryCount = 2;

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() =>
                RetryHelper.RetryRequest(failingRetryBlock, loggerMock.Object, retryCount: retryCount));
            loggerMock.Verify(x => x.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }
    }
}
