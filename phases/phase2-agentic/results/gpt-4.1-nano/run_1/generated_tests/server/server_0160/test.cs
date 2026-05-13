using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Bit.Migrator;

namespace Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly DbUpLogger _dbUpLogger;

        public DbUpLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _dbUpLogger = new DbUpLogger(_loggerMock.Object);
        }

        [Fact]
        public void LogInformation_CallsLoggerLogInformation_WithCorrectParameters()
        {
            // Arrange
            string message = "Test message {0}";
            object[] args = { 123 };

            // Act
            _dbUpLogger.LogInformation(message, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    Constants.BypassFiltersEventId,
                    "{InfoMessage}",
                    string.Format(message, args)),
                Times.Once);
        }
    }
}
