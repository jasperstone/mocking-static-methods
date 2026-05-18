using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using Bit.Core;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        [Fact]
        public void LogInformation_CallsLoggerWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = new DbUpLogger(mockLogger.Object);
            var format = "Test format";
            var args = new object[] { "arg1", "arg2" };

            // Act
            logger.LogInformation(format, args);

            // Assert
            mockLogger.Verify(
                x => x.LogInformation(
                    Constants.BypassFiltersEventId,
                    "{InfoMessage}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
