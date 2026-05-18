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
        public async Task RetryRequest_LogsWarningOnServiceUnavailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 1));

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_LogsErrorOnRetryLimitExceeded()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 1));

            // Assert
            mockLogger.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsInformationOnCancellation()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cts.Token));

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnHttpRequestException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                throw new HttpRequestException("Test exception"));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 1));

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
