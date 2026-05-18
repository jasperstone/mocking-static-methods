using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Server.IntegrationTesting;

public class RetryHelperTests
{
    [Fact]
    public async Task RetryRequest_ShouldLogWarningOnRetry()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryCount = 3;
        var cancellationToken = new CancellationToken();

        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
        {
            throw new HttpRequestException("Test exception");
        });

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, retryCount);
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(retryCount));
    }

    [Fact]
    public async Task RetryRequest_ShouldLogErrorOnFinalRetry()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryCount = 3;
        var cancellationToken = new CancellationToken();

        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
        {
            throw new HttpRequestException("Test exception");
        });

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, retryCount);
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task RetryRequest_ShouldLogInformationOnCancellation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryCount = 3;
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
        {
            throw new HttpRequestException("Test exception");
        });

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationTokenSource.Token, retryCount);
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task RetryRequest_ShouldLogWarningOnServiceUnavailable()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryCount = 3;
        var cancellationToken = new CancellationToken();

        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            return Task.FromResult(response);
        });

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, retryCount);
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(retryCount));
    }

    [Fact]
    public async Task RetryRequest_ShouldLogWarningOnHttpRequestException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var retryCount = 3;
        var cancellationToken = new CancellationToken();

        var retryBlock = new Func<Task<HttpResponseMessage>>(() =>
        {
            throw new HttpRequestException("Test exception");
        });

        // Act
        try
        {
            await RetryHelper.RetryRequest(retryBlock, mockLogger.Object, cancellationToken, retryCount);
        }
        catch (OperationCanceledException)
        {
            // Expected exception
        }

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Exactly(retryCount));
    }
}
