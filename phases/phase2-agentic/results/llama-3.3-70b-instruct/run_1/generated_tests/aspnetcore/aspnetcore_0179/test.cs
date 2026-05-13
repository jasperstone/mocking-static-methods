using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_RetryCountExceeded_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act and Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));
        loggerMock.Verify(l => l.LogWarning("Retry count 1..", 1), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_ServiceUnavailable_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act and Assert
        await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1);
        loggerMock.Verify(l => l.LogWarning("Retrying a service unavailable error."), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_RequestFailed_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act and Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));
        loggerMock.Verify(l => l.LogWarning("Failed to complete the request : {0}.", It.IsAny<string>()), Times.Once);
    }
}
