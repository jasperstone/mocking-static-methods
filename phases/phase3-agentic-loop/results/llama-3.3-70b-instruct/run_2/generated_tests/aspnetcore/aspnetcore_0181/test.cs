using Xunit;
using Moq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_RetryCountReached_LogsWarningAndThrowsOperationCanceledException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
            var cancellationTokenSource = new CancellationTokenSource();
            var retryCount = 1;

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationTokenSource.Token, retryCount));
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
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
            var cancellationTokenSource = new CancellationTokenSource();
            var retryCount = 2;

            // Act
            await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationTokenSource.Token, retryCount);

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(2));
        }

        [Fact]
        public async Task RetryRequest_RetrySucceeds_LogsInformationAndReturnsResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            var cancellationTokenSource = new CancellationTokenSource();
            var retryCount = 1;

            // Act
            var response = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationTokenSource.Token, retryCount);

            // Assert
            Assert.NotNull(response);
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>()), Times.Once);
        }
    }
}
