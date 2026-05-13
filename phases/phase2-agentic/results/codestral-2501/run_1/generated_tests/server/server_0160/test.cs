using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using System;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly DbUpLogger _dbUpLogger;

        public DbUpLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _dbUpLogger = new DbUpLogger(_mockLogger.Object);
        }

        [Fact]
        public void LogInformation_CallsLoggerWithCorrectParameters()
        {
            // Arrange
            var format = "Test message {0}";
            var args = new object[] { 123 };

            // Act
            _dbUpLogger.LogInformation(format, args);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test message 123")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
