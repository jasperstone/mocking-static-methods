using Bit.Migrator;
using DbUp.Engine.Output;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Moq.Language.Flow;
using System;
using Xunit;

namespace Bit.Migrator.Tests;

public class DbUpLoggerTests
{
    private readonly Mock<ILogger> _mockLogger;
    private readonly DbUpLogger _sut;

    public DbUpLoggerTests()
    {
        _mockLogger = new Mock<ILogger>();
        _mockLogger.Setup(x => x.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        _sut = new DbUpLogger(_mockLogger.Object);
    }

    [Fact]
    public void LogInformation_CallsLogInformation_WithCorrectParameters()
    {
        // Arrange
        string format = "Migration {0} completed";
        object[] args = { "1.0" };
        string expectedMessage = string.Format(format, args);

        // Act
        _sut.LogInformation(format, args);

        // Assert
        _mockLogger.Verify(x => x.LogInformation(
            It.Is<EventId>(eid => eid.Id == Bit.Core.Constants.BypassFiltersEventId),
            It.IsAny<object>(),
            null,
            "{InfoMessage}",
            It.Is<object[]>(objs => objs.Length == 1 && objs[0].ToString() == expectedMessage)),
            Times.Once);
    }

    [Fact]
    public void LogInformation_WithNoArgs_CallsLogInformation()
    {
        // Arrange
        string format = "Migration started";
        string expectedMessage = format;

        // Act
        _sut.LogInformation(format);

        // Assert
        _mockLogger.Verify(x => x.LogInformation(
            It.Is<EventId>(eid => eid.Id == Bit.Core.Constants.BypassFiltersEventId),
            It.IsAny<object>(),
            null,
            "{InfoMessage}",
            It.Is<object[]>(objs => objs.Length == 1 && objs[0].ToString() == expectedMessage)),
            Times.Once);
    }

    [Fact]
    public void LogInformation_UsesCorrectEventId()
    {
        // Act
        _sut.LogInformation("Test message");

        // Assert
        _mockLogger.Verify(x => x.LogInformation(
            It.Is<EventId>(eid => eid.Id == Bit.Core.Constants.BypassFiltersEventId),
            It.IsAny<object>(),
            null,
            It.IsAny<string>(),
            It.IsAny<object[]>()),
            Times.Once);
    }
}
