using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NATS.Client.Core;
using Xunit;

namespace Orleans.Streaming.NATS.Providers.Tests;

public class LoggerExtensionsTests
{
    [Fact]
    public void LogError_WhenCalledWithException_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var exception = new InvalidOperationException("Test exception");
        var message = "Error initializing NATS JetStream Connection Manager";

        // Act
        loggerMock.Object.LogError(exception, message);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WhenCalledWithErrorObject_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var error = new NatsConnectionException("Test error");
        var message = "Failed to enqueue NATS message to {Subject}";
        var subject = "test.subject";

        // Act
        loggerMock.Object.LogError(error, message, subject);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains(message) && ((string)v).Contains(subject)),
                error,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WhenCalledWithSimpleMessage_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var message = "Unable to enqueue message: NATS context is not initialized";

        // Act
        loggerMock.Object.LogError(message);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void LogError_WhenCalledWithTemplate_LogsCorrectly()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<NatsConnectionManager>>();
        var serverUrl = "nats://localhost:4222";

        // Act
        loggerMock.Object.LogError("Unable to connect to NATS server {NatsServer}", serverUrl);

        // Assert
        loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Unable to connect to NATS server") && ((string)v).Contains(serverUrl)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
