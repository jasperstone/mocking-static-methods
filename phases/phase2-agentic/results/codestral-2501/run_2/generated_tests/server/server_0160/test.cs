using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using DbUp.Engine.Output;
using Bit.Migrator;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_ShouldLogInformationWithCorrectEventIdAndMessage()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var dbUpLogger = new DbUpLogger(mockLogger.Object);
        var format = "Test message {0}";
        var args = new object[] { "arg1" };

        // Act
        dbUpLogger.LogInformation(format, args);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message arg1")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
