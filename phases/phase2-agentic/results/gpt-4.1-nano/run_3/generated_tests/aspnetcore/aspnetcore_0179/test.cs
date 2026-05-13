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
        public async Task RetryRequest_Should_LogWarning_OnEachRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    throw new HttpRequestException("Network error");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object);

            // Assert
            Assert.NotNull(response);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task RetryRequest_Should_LogWarning_When_ResponseIs503()
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
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object);

            // Assert
            Assert.NotNull(response);
            loggerMock.Verify(x => x.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogInformation_When_CancellationRequested()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await RetryHelper.RetryRequest(() => Task.FromResult(new HttpResponseMessage()), loggerMock.Object, cts.Token));
            loggerMock.Verify(x => x.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogError_When_LastAttemptFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                throw new HttpRequestException("Error");
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3));
            loggerMock.Verify(x => x.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }
    }
}
