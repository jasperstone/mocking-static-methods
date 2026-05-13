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
            var result = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, CancellationToken.None, 2);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ShouldLogErrorOnFinalRetryFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(x => x()).Throws(new HttpRequestException("Test exception"));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, CancellationToken.None, 2));

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
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
            var result = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, CancellationToken.None, 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public void RetryOperation_ShouldRetryOnException()
        {
            // Arrange
            var retryBlockMock = new Mock<Action>();
            var exceptionBlockMock = new Mock<Action<Exception>>();
            retryBlockMock.Setup(x => x()).Throws(new Exception("Test exception"));

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object, 2, 100);

            // Assert
            retryBlockMock.Verify(x => x(), Times.Exactly(2));
            exceptionBlockMock.Verify(x => x(It.IsAny<Exception>()), Times.Exactly(2));
        }
    }
}
