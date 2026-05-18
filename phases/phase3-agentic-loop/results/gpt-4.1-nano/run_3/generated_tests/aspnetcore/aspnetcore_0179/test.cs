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
        public async Task RetryRequest_Should_Log_Warning_For_Each_Retry()
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
            var result = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(3));
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
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var result = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            loggerMock.Verify(x => x.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Error_And_Throw_When_All_Retries_Fail()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                throw new HttpRequestException("Request failed");
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2));
            loggerMock.Verify(x => x.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_Log_Information_When_Canceled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task<HttpResponseMessage>> retryBlock = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));
            loggerMock.Verify(x => x.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }
    }
}
