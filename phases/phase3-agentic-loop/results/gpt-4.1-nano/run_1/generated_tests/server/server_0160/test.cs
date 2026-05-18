using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Bit.Migrator;
using System;

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
            string message = "Test message {0}";
            object[] args = { 123 };

            // Act
            _logger.LogInformation(message, args);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<int>(),
                    It.Is<string>(s => s.Contains("{InfoMessage}")),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
