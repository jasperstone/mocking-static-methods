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
        public async Task RetryRequest_Should_LogWarning_OnEachRetry()
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
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Verify that LogWarning was called at least once
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        [Fact]
        public async Task RetryRequest_Should_LogWarning_ForServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            var responses = new[]
            {
                new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                new HttpResponseMessage(HttpStatusCode.OK)
            };
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                var response = responses[callCount];
                callCount++;
                return Task.FromResult(response);
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Verify that LogWarning for "Retrying a service unavailable error." was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogInformation_WhenRetryCanceled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task<HttpResponseMessage>> retryBlock = () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));
            // Verify LogInformation called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry canceled.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_Should_LogError_When_LastAttemptFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                throw new HttpRequestException("Error");
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3));
            // Verify LogError was called
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry limit exceeded.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
