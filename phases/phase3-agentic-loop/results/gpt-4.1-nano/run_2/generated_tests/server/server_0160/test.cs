using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using System;

namespace Bit.Migrator.Tests
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
            string message = "Test message";

            // Act
            _dbUpLogger.LogInformation(message);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    Constants.BypassFiltersEventId,
                    "{InfoMessage}",
                    message),
                Times.Once);
        }
    }
}
