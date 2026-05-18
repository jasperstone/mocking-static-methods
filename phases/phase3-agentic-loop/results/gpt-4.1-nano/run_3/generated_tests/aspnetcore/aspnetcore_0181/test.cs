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
                var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                return Task.FromResult(response);
            };

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 3));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeast(2));
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
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", exceptionMessage), Times.Once);
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
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", exceptionMessage), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldLogInformationAndThrowOnCancellation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act & Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
                await RetryHelper.RetryRequest(async () =>
                {
                    await Task.Delay(10);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }, loggerMock.Object, cts.Token));

            loggerMock.Verify(l => l.LogInformation("Failed to connect, retry canceled."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ShouldReturnResponseOnSuccess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            int callCount = 0;

            Func<Task<HttpResponseMessage>> retryBlock = () =>
            {
                callCount++;
                if (callCount < 3)
                {
                    return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                }
                return Task.FromResult(expectedResponse);
            };

            // Act
            var response = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, retryCount: 5);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
