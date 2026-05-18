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
        public async Task RetryRequest_ShouldLogWarningOnHttpRequestException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() => throw new HttpRequestException("Test exception"));
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, 1));

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnWebException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() => throw new WebException("Test exception"));
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, 1));

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnServiceUnavailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, 1));

            // Assert
            mockLogger.Verify(
                logger => logger.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogErrorOnRetryLimitExceeded()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() => throw new HttpRequestException("Test exception"));
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, 1));

            // Assert
            mockLogger.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogInformationOnRetryCanceled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationTokenSource.Token));

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void RetryOperation_ShouldCallExceptionBlockOnException()
        {
            // Arrange
            var retryBlock = new Action(() => throw new InvalidOperationException("Test exception"));
            var exceptionBlockCalled = false;
            Action<Exception> exceptionBlock = ex => exceptionBlockCalled = true;

            // Act
            RetryHelper.RetryOperation(retryBlock, exceptionBlock, 1);

            // Assert
            Assert.True(exceptionBlockCalled);
        }

        [Fact]
        public void RetryOperation_ShouldNotCallExceptionBlockOnSuccess()
        {
            // Arrange
            var retryBlock = new Action(() => { });
            var exceptionBlockCalled = false;
            Action<Exception> exceptionBlock = ex => exceptionBlockCalled = true;

            // Act
            RetryHelper.RetryOperation(retryBlock, exceptionBlock, 1);

            // Assert
            Assert.False(exceptionBlockCalled);
        }
    }
}
