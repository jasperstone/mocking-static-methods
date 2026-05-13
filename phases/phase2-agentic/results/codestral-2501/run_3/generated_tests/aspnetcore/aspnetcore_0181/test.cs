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
        public async Task RetryRequest_ShouldLogWarningOnRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.SetupSequence(x => x())
                .Throws(new HttpRequestException("Test exception"))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 2);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldThrowOperationCanceledException_WhenRetryLimitExceeded()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(x => x()).Throws(new HttpRequestException("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 2));
        }

        [Fact]
        public async Task RetryRequest_ShouldReturnResponse_WhenRequestSucceeds()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(x => x()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task RetryRequest_ShouldRetryOnServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.SetupSequence(x => x())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public void RetryOperation_ShouldCallExceptionBlock_WhenRetryBlockThrows()
        {
            // Arrange
            var retryBlockMock = new Mock<Action>();
            retryBlockMock.Setup(x => x()).Throws(new Exception("Test exception"));
            var exceptionBlockMock = new Mock<Action<Exception>>();

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object, retryCount: 2);

            // Assert
            exceptionBlockMock.Verify(x => x(It.IsAny<Exception>()), Times.Exactly(2));
        }

        [Fact]
        public void RetryOperation_ShouldNotCallExceptionBlock_WhenRetryBlockSucceeds()
        {
            // Arrange
            var retryBlockMock = new Mock<Action>();
            var exceptionBlockMock = new Mock<Action<Exception>>();

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object, retryCount: 2);

            // Assert
            exceptionBlockMock.Verify(x => x(It.IsAny<Exception>()), Times.Never);
        }
    }
}
