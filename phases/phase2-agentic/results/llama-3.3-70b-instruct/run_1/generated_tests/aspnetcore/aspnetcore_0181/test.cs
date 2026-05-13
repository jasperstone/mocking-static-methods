using Xunit;
using Moq;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningOnRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cancellationToken = new CancellationTokenSource().Token;

            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cancellationToken = new CancellationTokenSource().Token;

            retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnRetryCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cancellationToken = new CancellationTokenSource().Token;

            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken));

            // Assert
            loggerMock.Verify(l => l.LogWarning("Retry count {retryCount}..", It.IsAny<int>()), Times.Once);
        }
    }
}
