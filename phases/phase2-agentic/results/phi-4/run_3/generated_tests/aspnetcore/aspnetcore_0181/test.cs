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
        public async Task RetryRequest_LogsWarningOnRetry()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
            retryBlockMock
                .Setup(rb => rb())
                .ThrowsAsync(new HttpRequestException("Test exception"));

            var cancellationToken = CancellationToken.None;

            // Act
            await Assert.ThrowsAsync<OperationCanceledException>(() =>
                RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 3));

            // Assert
            loggerMock.Verify(
                l => l.LogWarning(It.IsAny<string>(), It.Is<object[]>(args => (int)args[0] == 1)),
                Times.Exactly(3));
        }
    }
}
