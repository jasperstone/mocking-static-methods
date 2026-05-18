using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Migrator.Tests;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_CallsLoggerLogInformation()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var dbUpLogger = new DbUpLogger(loggerMock.Object);

        // Act
        dbUpLogger.LogInformation("Test message");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), It.IsAny<string>(), (object[])It.IsAny<object[]>()), Times.Once);
    }
}
