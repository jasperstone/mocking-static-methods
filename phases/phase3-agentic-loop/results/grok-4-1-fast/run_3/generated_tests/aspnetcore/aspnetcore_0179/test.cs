using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_LogsWarningForServiceUnavailable()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var cancellationToken = new CancellationToken(false);
        var callCount = 0;

        // Act
        try
        {
            await RetryHelper.RetryRequest(
                async () =>
                {
                    callCount++;
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
                },
                mockLogger.Object,
                cancellationToken,
                retryCount: 2);
        }
        catch
        {
            // Expected after retries exhaust
        }

        // Assert
        Assert.Equal(2, callCount);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retrying a service unavailable error.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryRequest_SuccessOnNon503_ReturnsResponse()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var cancellationToken = new CancellationToken(false);
        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);

        // Act
        var result = await RetryHelper.RetryRequest(
            () => Task.FromResult(expectedResponse),
            mockLogger.Object,
            cancellationToken,
            retryCount: 1);

        // Assert
        Assert.Same(expectedResponse, result);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Retry count")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryRequest_Canceled_LogsInformationAndThrows()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<OperationCanceledException>(
            () => RetryHelper.RetryRequest(
                () => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)),
                mockLogger.Object,
                cts.Token));

        Assert.Equal("Failed to connect, retry canceled.", ex.Message);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("retry canceled")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task RetryRequest_HttpRequestException_LogsWarningAndRetries()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var cancellationToken = new CancellationToken(false);
        var callCount = 0;
        var exceptionMessage = "Connection failed";

        // Act
        try
        {
            await RetryHelper.RetryRequest(
                async () =>
                {
                    callCount++;
                    throw new HttpRequestException(exceptionMessage);
                },
                mockLogger.Object,
                cancellationToken,
                retryCount: 2);
        }
        catch
        {
            // Expected
        }

        // Assert
        Assert.Equal(2, callCount);
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to complete the request") && v.ToString().Contains(exceptionMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
