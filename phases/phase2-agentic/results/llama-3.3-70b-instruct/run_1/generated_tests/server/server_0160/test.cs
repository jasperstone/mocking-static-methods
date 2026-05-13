using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        [Fact]
        public void LogInformation_CallsLogInformationOnLogger()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(loggerMock.Object);
            var format = "Test format";
            var args = new object[] { "arg1", "arg2" };

            // Act
            dbUpLogger.LogInformation(format, args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogInformation_CallsLogInformationOnLogger_WithCorrectArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(loggerMock.Object);
            var format = "Test format";
            var args = new object[] { "arg1", "arg2" };

            // Act
            dbUpLogger.LogInformation(format, args);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<EventId>(), "{InfoMessage}", string.Format(format, args)), Times.Once);
        }
    }
}
