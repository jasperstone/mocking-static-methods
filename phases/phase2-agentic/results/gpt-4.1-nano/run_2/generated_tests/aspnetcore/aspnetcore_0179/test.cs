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
        public async Task RetryRequest_Should_Log_Warning_For_Each_Retry()
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
                x => x.LogWarning(It.IsAny<string>(), It.IsAny<object>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Warning_For_ServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    return Task.FromResult(response);
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
        public async Task RetryRequest_Should_Log_Information_When_Canceled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await RetryHelper.RetryRequest(async () => new HttpResponseMessage(HttpStatusCode.OK), loggerMock.Object, cts.Token));
            loggerMock.Verify(x => x.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Error_And_Throw_When_Last_Attempt_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () => throw new HttpRequestException("fail");
            var exception = await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2));

            // Assert
            Assert.NotNull(exception);
            loggerMock.Verify(x => x.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }
    }
}
