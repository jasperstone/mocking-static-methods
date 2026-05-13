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
        var dbUpLogger = new DbUpLogger(mockLogger.Object);
        var format = "Test message {0}";
        var args = new object[] { 123 };

        // Act
        dbUpLogger.LogInformation(format, args);

        // Assert
        mockLogger.Verify(
            logger => logger.Log(
                It.Is<LogLevel>(logLevel => logLevel == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message 123")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
