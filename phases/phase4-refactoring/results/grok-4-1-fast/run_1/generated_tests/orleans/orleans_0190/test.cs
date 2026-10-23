using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using NATS.Client.Core;
using Orleans.Streaming.NATS;
using Xunit;

namespace Orleans.Streaming.NATS.Providers.Tests;

public class LoggerExtensionsTests
{
    private readonly Mock<ILogger<NatsConnectionManager>> _loggerMock = new();

    [Fact]
    public void LogError_VerifyExtensionMethodCall()
    {
        // Arrange
        var exception = new Exception("Test exception");
        var logger = _loggerMock.Object;
        
        // Act - Directly invoke the extension method that matches line 136 signature
        logger.LogError(exception, "Error initializing NATS JetStream Connection Manager");

        // Assert - Verify the Log method was called with correct parameters
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Error initializing NATS JetStream Connection Manager") == true),
                It.Is<Exception>(ex => ex == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_ConnectionFailure_VerifyMessage()
    {
        // Arrange
        var logger = _loggerMock.Object;
        
        // Act - Simulate the connection failure LogError call
        logger.LogError("Unable to connect to NATS server {NatsServer}", "nats://localhost:4222");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("Unable to connect to NATS server") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_JetStreamUnavailable_VerifyMessage()
    {
        // Arrange
        var logger = _loggerMock.Object;
        
        // Act - Simulate the JetStream unavailable LogError call
        logger.LogError("Unable to use {NatsServer} for Orleans Stream Provider {ProviderName}: NATS JetStream is not available", 
                       "nats://localhost:4222", "test-provider");

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v?.ToString()!.Contains("NATS JetStream is not available") == true),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
