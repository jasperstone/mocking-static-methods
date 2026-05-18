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
            mockLogger.Verify(
                x => x.LogInformation(
                    BypassFiltersEventId,
                    "{InfoMessage}",
                    expectedMessage),
                Times.Once);
        }
    }
}
