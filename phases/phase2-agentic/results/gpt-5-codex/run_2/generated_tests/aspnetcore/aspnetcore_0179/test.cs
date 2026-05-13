using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarningForServiceUnavailableResponseAndRetries()
        {
            // Arrange
            var attempt = 0;
            var mockLogger = new Mock<ILogger>();
            var response503 = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            Task<HttpResponseMessage> RetryBlock()
                => Task.FromResult(++attempt == 1 ? response503 : new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await RetryHelper.RetryRequest(RetryBlock, mockLogger.Object, CancellationToken.None, retryCount: 2);

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString() == "Retry count 1.."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, t) => o.ToString() == "Retrying a service unavailable error."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
