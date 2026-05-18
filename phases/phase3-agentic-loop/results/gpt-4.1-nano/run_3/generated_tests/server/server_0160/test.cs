using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;

namespace Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly DbUpLogger _logger;

        public DbUpLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _logger = new DbUpLogger(_loggerMock.Object);
        }

        [Fact]
        public void LogInformation_CallsLoggerLogInformation_WithCorrectParameters()
        {
            // Arrange
            string messageFormat = "Test message {0}";
            object[] args = { 123 };

            // Act
            _logger.LogInformation(messageFormat, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    Constants.BypassFiltersEventId,
                    "{InfoMessage}",
                    string.Format(messageFormat, args)),
                Times.Once);
        }
    }
}
