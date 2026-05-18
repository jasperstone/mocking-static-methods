using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_ServiceUnavailableResponse_LogsRetryingMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var cancellationToken = new CancellationToken(false);
        int callCount = 0;

        Func<Task<HttpResponseMessage>> retryBlock = async () =>
        {
            callCount++;
            await Task.Yield();
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        };

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount: 2);
        }
        catch
        {
            // Expect to throw after retry limit exceeded
        }

        // Assert
        Assert.Equal(2, callCount);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying a service unavailable error.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Fact]
    public async Task RetryRequest_SuccessfulResponse_DoesNotLog503RetryMessage()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.IsEnabled(LogLevel.Warning)).Returns(true);
        var cancellationToken = new CancellationToken(false);

        Func<Task<HttpResponseMessage>> retryBlock = async () =>
        {
            await Task.Yield();
            return new HttpResponseMessage(HttpStatusCode.OK);
        };

        // Act
        var result = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount: 1);

        // Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Retrying a service unavailable error.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Never);
    }
}
