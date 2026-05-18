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
        public async Task RetryRequest_ShouldLogWarningOnEachRetry()
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
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            var exceptionMessage = "Network error";

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 2)
                {
                    throw new HttpRequestException(exceptionMessage);
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", exceptionMessage), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnWebException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            var exceptionMessage = "Web error";

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 2)
                {
                    throw new WebException(exceptionMessage);
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", exceptionMessage), Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogRetryLimitExceededAndThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                throw new HttpRequestException("fail");
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 2));
            loggerMock.Verify(l => l.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogRetryingOnServiceUnavailable()
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
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogInformationOnCancel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task<HttpResponseMessage>> retryBlock = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));
            loggerMock.Verify(l => l.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }
    }
}
