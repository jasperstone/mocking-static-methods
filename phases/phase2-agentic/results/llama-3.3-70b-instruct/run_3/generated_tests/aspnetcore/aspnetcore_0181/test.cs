using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningOnRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(r => r()).Throws(new HttpRequestException());

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Retry count {retryCount}..", 1), Times.Once);
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(r => r()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Retry count {retryCount}..", 1), Times.Once);
            loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsErrorOnRetryLimitExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(r => r()).Throws(new HttpRequestException());

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 1));

            // Assert
            loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }
    }
}
