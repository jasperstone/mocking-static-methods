using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Migrator.Tests;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_CallsLogInformationOnLogger()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var dbUpLogger = new DbUpLogger(loggerMock.Object);

        // Act
        dbUpLogger.LogInformation("Test message");

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), "{InfoMessage}", "Test message"), Times.Once);
    }
}
