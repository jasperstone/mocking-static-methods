using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DbUp.Engine.Output;
using Bit.Migrator;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_CallsLoggerLogInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = new DbUpLogger(mockLogger.Object);

        // Act
        logger.LogInformation("Test message");

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<EventId>(),
                It.IsAny<string>(),
                It.IsAny<object[]>()),
            Times.Once);
    }
}
