using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using DbUp.Engine.Output;
using Bit.Migrator;
using System;

namespace Bit.Migrator.Tests;

public class DbUpLoggerTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly DbUpLogger _sut;

    public DbUpLoggerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _sut = new DbUpLogger(_mockLogger.Object);
    }

    [Fact]
    public void LogInformation_WithArgs_CallsLogInformation_WithCorrectParameters()
    {
        // Arrange
        var format = "Migration {0} completed";
        var args = new object[] { "1.0" };
        var formattedMessage = string.Format(format, args);

        // Act
        _sut.LogInformation(format, args);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "{InfoMessage}",
                formattedMessage),
            Times.Once);
    }

    [Fact]
    public void LogInformation_NoArgs_CallsLogInformation_WithCorrectParameters()
    {
        // Arrange
        var format = "Migration started";
        var formattedMessage = format;

        // Act
        _sut.LogInformation(format);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "{InfoMessage}",
                formattedMessage),
            Times.Once);
    }

    [Fact]
    public void LogInformation_MultipleArgs_CallsLogInformation_WithCorrectParameters()
    {
        // Arrange
        var format = "Migration {0} for {1}";
        var args = new object[] { "1.0", "database" };
        var formattedMessage = string.Format(format, args);

        // Act
        _sut.LogInformation(format, args);

        // Assert
        _mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<Exception>(),
                "{InfoMessage}",
                formattedMessage),
            Times.Once);
    }
}
