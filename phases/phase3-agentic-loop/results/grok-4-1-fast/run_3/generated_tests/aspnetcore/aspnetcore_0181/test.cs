using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.IntegrationTesting;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Microsoft.AspNetCore.Server.IntegrationTesting.Tests;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_LogsWarningOnHttpRequestException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        int calls = 0;
        Func<Task<HttpResponseMessage>> retryBlock = async () =>
        {
            calls++;
            if (calls < 2)
            {
                throw new HttpRequestException("Connection failed");
            }
            return new HttpResponseMessage();
        };

        var cancellationToken = CancellationToken.None;
        const int retryCount = 2;

        // Act
        var result = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount);

        // Assert
        Assert.NotNull(result);
        loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v?.ToString()?.Contains("Failed to complete the request : Connection failed") == true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_LogsWarningOnWebException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        int calls = 0;
        Func<Task<HttpResponseMessage>> retryBlock = async () =>
        {
            calls++;
            if (calls < 2)
            {
                throw new WebException("Web connection failed");
            }
            return new HttpResponseMessage();
        };

        var cancellationToken = CancellationToken.None;
        const int retryCount = 2;

        // Act
        var result = await RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount);

        // Assert
        Assert.NotNull(result);
        loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v?.ToString()?.Contains("Failed to complete the request : Web connection failed") == true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public async Task RetryRequest_DoesNotLogSpecificWarningOnOtherException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        loggerMock.Setup(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

        Func<Task<HttpResponseMessage>> retryBlock = () => 
            Task.FromException<HttpResponseMessage>(new InvalidOperationException("Invalid operation"));

        var cancellationToken = CancellationToken.None;
        const int retryCount = 2;

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => 
            RetryHelper.RetryRequest(retryBlock, loggerMock.Object, cancellationToken, retryCount));

        loggerMock.Verify(x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => 
                v?.ToString()?.Contains("Failed to complete the request") == true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }
}
