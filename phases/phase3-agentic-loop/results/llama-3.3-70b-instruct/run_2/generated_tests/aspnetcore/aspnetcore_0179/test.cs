using Xunit;
using Moq;
using System.Net;
using System.Net.Http;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Server.IntegrationTesting;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_RetryCountReached_ThrowsOperationCanceledException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act and Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));
    }

    [Fact]
    public async Task RetryRequest_ServiceUnavailable_Retries()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.SetupSequence(rb => rb())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var response = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RetryRequest_RequestSucceeds_ReturnsResponse()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        var response = await RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task RetryRequest_RequestFails_LogsWarning()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var retryBlockMock = new Mock<Func<Task<HttpResponseMessage>>>();
        retryBlockMock.Setup(rb => rb()).Throws<HttpRequestException>();
        var cancellationToken = new CancellationTokenSource().Token;

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlockMock.Object, loggerMock.Object, cancellationToken, 1));

        // Assert
        loggerMock.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }
}
