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
        public async Task RetryRequest_ShouldLogWarningOnServiceUnavailable()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnRetryCount()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ShouldLogErrorOnRetryLimitExceeded()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry limit exceeded.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogInformationOnRetryCanceled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationTokenSource.Token, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry canceled.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnHttpRequestException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                throw new HttpRequestException("Test exception"));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnWebException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
                throw new WebException("Test exception"));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, CancellationToken.None, 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Exactly(2));
        }
    }
}
