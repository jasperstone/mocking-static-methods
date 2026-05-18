using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsServiceUnavailableWarning_WhenResponseIs503()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var serviceUnavailableResponse = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            int callCount = 0;
            Func<Task<HttpResponseMessage>> retryBlock = async () =>
            {
                callCount++;
                return serviceUnavailableResponse;
            };

            // Act
            var token = new CancellationToken(false);
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                RetryHelper.RetryRequest(retryBlock, mockLogger.Object, token, retryCount: 2));

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying a service unavailable error.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Equal(2, callCount);
        }

        [Fact]
        public async Task RetryRequest_ReturnsSuccess_WhenResponseIsSuccessful()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            var successResponse = new HttpResponseMessage(HttpStatusCode.OK);
            Func<Task<HttpResponseMessage>> retryBlock = async () => successResponse;

            // Act
            var token = new CancellationToken(false);
            var result = await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, token, retryCount: 1);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying a service unavailable error.")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
