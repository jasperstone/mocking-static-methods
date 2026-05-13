using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_SuccessfulResponse_ReturnsResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryBlock = new Func<Task<HttpResponseMessage>>(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var cancellationToken = new CancellationToken();

        // Act
        var response = await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        mockLogger.Verify(logger => logger.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_ServiceUnavailable_RetriesAndLogsWarning()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryBlock = new Func<Task<HttpResponseMessage>>(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));
        var cancellationToken = new CancellationToken();

        // Act
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken));

        // Assert
        mockLogger.Verify(logger => logger.LogWarning("Retrying a service unavailable error."), Times.Exactly(60));
    }

    [Fact]
    public async Task RetryRequest_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryBlock = new Func<Task<HttpResponseMessage>>(() => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationTokenSource.Token));
        mockLogger.Verify(logger => logger.LogInformation("Failed to connect, retry canceled."), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_Exception_LogsErrorAndThrows()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryBlock = new Func<Task<HttpResponseMessage>>(() => throw new HttpRequestException("Test exception"));
        var cancellationToken = new CancellationToken();

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken));
        mockLogger.Verify(logger => logger.LogError(0, It.IsAny<Exception>(), "Failed to connect, retry limit exceeded."), Times.Once);
    }

    [Fact]
    public void RetryOperation_SuccessfulOperation_DoesNotRetry()
    {
        // Arrange
        var retryBlock = new Action(() => { });
        var exceptionBlock = new Action<Exception>(ex => { });

        // Act
        RetryHelper.RetryOperation(retryBlock, exceptionBlock);

        // Assert
        // No exception should be thrown, and no retries should occur
    }

    [Fact]
    public void RetryOperation_Exception_RetriesAndLogsException()
    {
        // Arrange
        var retryCount = 0;
        var retryBlock = new Action(() => throw new InvalidOperationException("Test exception"));
        var exceptionBlock = new Action<Exception>(ex => retryCount++);

        // Act
        RetryHelper.RetryOperation(retryBlock, exceptionBlock, retryCount: 3);

        // Assert
        Assert.Equal(3, retryCount);
    }
}
