using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Migrator;

namespace Bit.Migrator.Tests
{
    public class DbUpLoggerTests
    {
        private static readonly EventId BypassFiltersEventId = new EventId(0, "BypassFilters");

        [Fact]
        public void LogInformation_CallsLoggerLogInformationWithCorrectParameters()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var dbUpLogger = new DbUpLogger(mockLogger.Object);
            string format = "Test message {0}";
            object[] args = { 123 };
            string expectedMessage = string.Format(format, args);

            // Act
            dbUpLogger.LogInformation(format, args);

            // Assert
            mockLogger.Verify(logger => logger.Log(
                It.Is<EventId>(e => e.Id == BypassFiltersEventId.Id && e.Name == BypassFiltersEventId.Name),
                LogLevel.Information,
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "{InfoMessage}"),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
