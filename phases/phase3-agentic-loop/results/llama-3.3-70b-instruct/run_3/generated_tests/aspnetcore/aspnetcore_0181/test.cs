using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests
{
    public class RetryHelperTests
    {
        [Fact]
        public async Task RetryRequest_LogsWarning_WhenRetryCountIsReached()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cancellationTokenSource = new CancellationTokenSource();
            var retryCount = 3;

            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationTokenSource.Token, retryCount));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(retryCount));
        }

        [Fact]
        public async Task RetryRequest_LogsWarning_WhenServiceUnavailable()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cancellationTokenSource = new CancellationTokenSource();
            var retryCount = 3;

            retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationTokenSource.Token, retryCount));

            // Assert
            loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(retryCount));
        }

        [Fact]
        public void RetryOperation_LogsWarning_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Action>();
            var exceptionBlockMock = new Mock<Action<Exception>>();
            var retryCount = 3;

            retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object, retryCount);

            // Assert
            exceptionBlockMock.Verify(eb => eb(It.IsAny<Exception>()), Times.Exactly(retryCount - 1));
        }
    }
}
