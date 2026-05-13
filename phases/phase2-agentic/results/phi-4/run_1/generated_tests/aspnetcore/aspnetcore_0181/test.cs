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
        public async Task RetryRequest_LogsWarningOnHttpRequestException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
            {
                throw new HttpRequestException("Test exception");
            });

            // Act
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, new CancellationToken(), 3));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Retry count {retryCount}..", It.IsAny<int>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.LogWarning("Failed to complete the request : {0}.", "Test exception"),
                Times.Once);
        }

        [Fact]
        public async Task RetryRequest_LogsWarningOnWebException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
            {
                throw new WebException("Test exception");
            });

            // Act
            var exception = await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlock, loggerMock.Object, new CancellationToken(), 3));

            // Assert
            loggerMock.Verify(
                x => x.LogWarning("Retry count {retryCount}..", It.IsAny<int>()),
                Times.AtLeastOnce);

            loggerMock.Verify(
                x => x.LogWarning("Failed to complete the request : {0}.", "Test exception"),
                Times.Once);
        }
    }
}
