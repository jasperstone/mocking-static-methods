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
        public async Task RetryRequest_RetryCountReached_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_ServiceUnavailable_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
            var cancellationToken = new CancellationToken();

            // Act
            await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RetryRequest_RequestFails_LogsWarning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
            var cancellationToken = new CancellationToken();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object>()), Times.Once);
        }
    }
}
