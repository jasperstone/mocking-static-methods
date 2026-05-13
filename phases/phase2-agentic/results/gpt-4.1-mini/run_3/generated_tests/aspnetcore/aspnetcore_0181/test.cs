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
        public async Task RetryRequest_LogsWarningOnEachRetryCount()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    // Simulate a 503 ServiceUnavailable response to trigger retry
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                else
                {
                    // Success on third try
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 5);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Verify LogWarning was called with "Retry count {retryCount}.." at least twice (for retries 1 and 2)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeast(2));
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnFailedRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new HttpRequestException("Request failed");
                }
                else
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
                }
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            // Verify LogWarning was called with the message containing "Failed to complete the request"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ThrowsOperationCanceledException_WhenCancellationRequested()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cts.Token));

            Assert.Equal("Failed to connect, retry canceled.", ex.Message);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to connect, retry canceled.")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
