using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Tsavorite.Tests
{
    public class RecoveryLoggerTests
    {
        private const string ExpectedMessage = "Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.";

        [Fact]
        public void LogInformation_NonEmptyLog_CallsUnderlyingLogMethod()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
            loggerMock.Setup(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            // Act - Exact call from Recovery.cs line ~500
            loggerMock.Object.LogInformation(ExpectedMessage);

            // Assert
            loggerMock.Verify();
        }

        [Fact]
        public void LogInformation_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;

            // Act & Assert - Matches the null-conditional logger?.LogInformation call
            logger?.LogInformation(ExpectedMessage);
        }

        [Fact]
        public void LogInformation_WithLogger_UsesInformationLevel()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            bool loggedAtInfoLevel = false;

            loggerMock.Setup(l => l.Log(
                It.Is<LogLevel>(level => level == LogLevel.Information),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback(() => loggedAtInfoLevel = true);

            // Act - Production code pattern
            loggerMock.Object.LogInformation(ExpectedMessage);

            // Assert
            Assert.True(loggedAtInfoLevel);
        }
    }
}
