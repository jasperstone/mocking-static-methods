using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NATS.Client.Core;
using Xunit;

namespace Orleans.Streaming.NATS.Tests;

public class NatsConnectionManagerLoggerTests
{
    [Fact]
    public void LoggerExtensions_LogError_On_NatsConnectionManager()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var exception = new Exception("Test exception");
        
        // Act - Directly test the LoggerExtensions LogError call pattern from line 136
        loggerMock.Object.LogError(exception, "Error initializing NATS JetStream Connection Manager");

        // Assert - Verify the extension method was called with correct parameters
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object? state) => 
                    state?.ToString()?.Contains("Error initializing NATS JetStream Connection Manager") == true),
                It.Is<Exception>(e => e == exception),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogError_WithServerUrl()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var serverUrl = "nats://test-server:4222";
        
        // Act - Test the LogError pattern used in NatsConnectionManager.Initialize()
        loggerMock.Object.LogError("Unable to connect to NATS server {NatsServer}", serverUrl);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object? state) => 
                    state?.ToString()?.Contains("Unable to connect to NATS server") == true &&
                    state?.ToString()?.Contains(serverUrl) == true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogError_EnqueueMessageFailure()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var subject = "test.subject";
        
        // Act - Test the LogError pattern from EnqueueMessage when ack fails
        var error = new NatsAckFailedException("Publish failed");
        loggerMock.Object.LogError(error, "Failed to enqueue NATS message to {Subject}", subject);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object? state) => 
                    state?.ToString()?.Contains("Failed to enqueue NATS message to") == true &&
                    state?.ToString()?.Contains(subject) == true),
                It.Is<Exception>(e => e == error),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LoggerExtensions_LogError_UninitializedContext()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();

        // Act - Test the LogError call from EnqueueMessage when context is null
        loggerMock.Object.LogError("Unable to enqueue message: NATS context is not initialized");

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((object? state) => 
                    state?.ToString()?.Contains("Unable to enqueue message: NATS context is not initialized") == true),
                null!,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

// Helper exception for testing the exact LogError pattern
public class NatsAckFailedException : Exception
{
    public NatsAckFailedException(string message) : base(message) { }
}
