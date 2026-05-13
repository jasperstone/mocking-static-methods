using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;

namespace DbUpLoggerTests
{
    public class DbUpLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly DbUpLogger _logger;

        public DbUpLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _logger = new DbUpLogger(_mockLogger.Object);
        }

        [Fact]
        public void LogInformation_CallsLogInformationWithCorrectParameters()
        {
            // Arrange
            string message = "Test message";
            int eventId = Constants.BypassFiltersEventId;

            // Act
            _logger.LogInformation(message);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    eventId,
                    It.IsAny<It.IsAnyType>(),
                    It.Is<object>(o => o.ToString().Contains(message)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
