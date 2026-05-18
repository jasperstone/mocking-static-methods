using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using Bit.Core;

public class DbUpLoggerTests
{
    [Fact]
    public void LogInformation_CallsLoggerWithCorrectParameters()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var logger = new DbUpLogger(mockLogger.Object);
        var format = "Test message {0}";
        var args = new object[] { 123 };

        // Act
        logger.LogInformation(format, args);

        // Assert
        mockLogger.Verify(
            x => x.LogInformation(
                Constants.BypassFiltersEventId,
                "{InfoMessage}",
                It.Is<string>(s => s == string.Format(format, args))),
            Times.Once);
    }
}
