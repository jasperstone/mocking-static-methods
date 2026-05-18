using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Server.IntegrationTesting;

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
                if (callCount < 3)
                {
                    var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                    return Task.FromResult(response);
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 5);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(x => x.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeast(2));
        }

        [Fact]
        public async Task RetryRequest_ShouldLogErrorAndThrow_WhenRetriesExhausted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                throw new HttpRequestException("Request failed");
            };

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(async () =>
                await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3));
            loggerMock.Verify(x => x.LogError(It.IsAny<int>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogWarningOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 2)
                {
                    throw new HttpRequestException("Network error");
                }
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            loggerMock.Verify(x => x.LogWarning("Failed to complete the request : {0}.", "Network error"), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogInformationAndCancel_WhenCancellationRequested()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await RetryHelper.RetryRequest(async () => new HttpResponseMessage(HttpStatusCode.OK), loggerMock.Object, cts.Token));
            loggerMock.Verify(x => x.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }
    }
}
