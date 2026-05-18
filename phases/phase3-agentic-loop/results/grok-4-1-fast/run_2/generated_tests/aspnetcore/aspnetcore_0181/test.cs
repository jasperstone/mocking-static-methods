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
    private class TestLogger : ILogger
    {
        public List<(LogLevel level, string message, object[] args)> Logs { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Logs.Add((logLevel, message, new object[0]));
        }
    }

    [Fact]
    public async Task RetryRequest_LogsWarningOnHttpRequestException()
    {
        // Arrange
        var logger = new TestLogger();
        var retryBlockCalled = false;
        var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
        {
            retryBlockCalled = true;
            throw new HttpRequestException("Connection failed");
        });
        var cancellationToken = new CancellationToken();

        // Act
        await Assert.ThrowsAsync<Exception>(() => RetryHelper.RetryRequest(retryBlock, logger, cancellationToken, retryCount: 1));

        // Assert
        Assert.True(retryBlockCalled);
        Assert.Contains(logger.Logs, log => log.level == LogLevel.Warning && log.message.Contains("Failed to complete the request : Connection failed"));
    }

    [Fact]
    public async Task RetryRequest_LogsWarningOnWebException()
    {
        // Arrange
        var logger = new TestLogger();
        var retryBlockCalled = false;
        var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
        {
            retryBlockCalled = true;
            throw new WebException("Web error");
        });
        var cancellationToken = new CancellationToken();

        // Act
        await Assert.ThrowsAsync<Exception>(() => RetryHelper.RetryRequest(retryBlock, logger, cancellationToken, retryCount: 1));

        // Assert
        Assert.True(retryBlockCalled);
        Assert.Contains(logger.Logs, log => log.level == LogLevel.Warning && log.message.Contains("Failed to complete the request : Web error"));
    }

    [Fact]
    public async Task RetryRequest_DoesNotLogWarningOnOtherException()
    {
        // Arrange
        var logger = new TestLogger();
        var retryBlockCalled = false;
        var retryBlock = new Func<Task<HttpResponseMessage>>(async () =>
        {
            retryBlockCalled = true;
            throw new InvalidOperationException("Invalid operation");
        });
        var cancellationToken = new CancellationToken();

        // Act
        await Assert.ThrowsAsync<Exception>(() => RetryHelper.RetryRequest(retryBlock, logger, cancellationToken, retryCount: 1));

        // Assert
        Assert.True(retryBlockCalled);
        Assert.DoesNotContain(logger.Logs, log => log.level == LogLevel.Warning && log.message.Contains("Failed to complete the request"));
    }
}
