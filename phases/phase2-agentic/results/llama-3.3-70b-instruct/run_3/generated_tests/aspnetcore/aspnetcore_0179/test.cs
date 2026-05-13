using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_RetryCountReached_LogsWarningAndThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).Throws(new HttpRequestException());
            var cancellationToken = new CancellationTokenSource().Token;

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));
            loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Never);
            loggerMock.Verify(l => l.LogError(It.IsAny<EventId>(), It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ServiceUnavailable_LogsWarningAndRetries()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.SetupSequence(rb => rb())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_RequestFailed_LogsWarningAndRetries()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.SetupSequence(rb => rb())
                .Throws(new HttpRequestException())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var cancellationToken = new CancellationTokenSource().Token;

            // Act
            await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken);

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", It.IsAny<string>()), Times.Once);
        }
    }
}
