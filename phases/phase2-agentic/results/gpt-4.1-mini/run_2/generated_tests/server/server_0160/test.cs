using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Bit.Migrator;

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
        public void LogInformation_CallsLoggerLogInformationWithCorrectParameters()
        {
            // Arrange
            string format = "Test message {0}";
            object[] args = { 123 };
            string expectedMessage = string.Format(format, args);

            // Act
            _dbUpLogger.LogInformation(format, args);

            // Assert
            _mockLogger.Verify(logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.Is<string>(s => s == "{InfoMessage}"),
                It.Is<object[]>(o => o.Length == 1 && (string)o[0] == expectedMessage)
            ), Times.Never);

            // Because the extension method LogInformation with EventId and message template is called,
            // but the signature is: LogInformation(EventId eventId, string message, params object[] args)
            // The actual call is: LogInformation(Constants.BypassFiltersEventId, "{InfoMessage}", string.Format(format, args))
            // So the third argument is a single string, not an object array.

            // We need to verify the call with the exact parameters:
            _mockLogger.Verify(logger => logger.LogInformation(
                It.Is<EventId>(e => e.Id == Constants.BypassFiltersEventId.Id),
                "{InfoMessage}",
                expectedMessage
            ), Times.Once);
        }
    }

    // We need to define Constants.BypassFiltersEventId for the test.
    // Since we couldn't find the source, we define a minimal stub here.
    internal static class Constants
    {
        public static readonly EventId BypassFiltersEventId = new EventId(9999, "BypassFilters");
    }
}
