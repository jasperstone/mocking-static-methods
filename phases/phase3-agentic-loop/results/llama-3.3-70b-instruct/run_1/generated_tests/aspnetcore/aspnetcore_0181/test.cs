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
        public async Task RetryRequest_SucceedsOnFirstTry_ReturnsResponse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var response = new HttpResponseMessage(HttpStatusCode.OK);
            retryBlockMock.Setup(rb => rb()).ReturnsAsync(response);

            // Act
            var result = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object);

            // Assert
            Assert.Equal(response, result);
        }

        [Fact]
        public async Task RetryRequest_FailsOnFirstTry_Retries()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var exception = new HttpRequestException();
            retryBlockMock.SetupSequence(rb => rb())
                .Throws(exception)
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var result = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task RetryRequest_FailsOnAllTries_ThrowsOperationCanceledException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var exception = new HttpRequestException();
            retryBlockMock.Setup(rb => rb()).Throws(exception);

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, retryCount: 1));
        }

        [Fact]
        public async Task RetryRequest_CancellationTokenIsCanceled_ThrowsOperationCanceledException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            var cts = new CancellationTokenSource();
            cts.Cancel();

            // Act and Assert
            await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cts.Token));
        }

        [Fact]
        public void RetryOperation_SucceedsOnFirstTry_DoesNotRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Action>();
            var exceptionBlockMock = new Mock<Action<Exception>>();

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object);

            // Assert
            retryBlockMock.Verify(rb => rb(), Times.Once);
            exceptionBlockMock.Verify(eb => eb(It.IsAny<Exception>()), Times.Never);
        }

        [Fact]
        public void RetryOperation_FailsOnFirstTry_Retries()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Action>();
            var exceptionBlockMock = new Mock<Action<Exception>>();
            var exception = new Exception();
            retryBlockMock.SetupSequence(rb => rb())
                .Throws(exception)
                .Throws(exception)
                .Throws(exception);

            // Act
            RetryHelper.RetryOperation(retryBlockMock.Object, exceptionBlockMock.Object, retryCount: 3);

            // Assert
            retryBlockMock.Verify(rb => rb(), Times.Exactly(3));
            exceptionBlockMock.Verify(eb => eb(It.IsAny<Exception>()), Times.Exactly(3));
        }
    }
}
