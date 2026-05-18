using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Server.Migration.Tests
{
    public class MigrateSessionSlotsLoggerTests
    {
        private readonly Mock<ILogger> _loggerMock;

        public MigrateSessionSlotsLoggerTests()
        {
            _loggerMock = new Mock<ILogger>();
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);
        }

        [Fact]
        public void LogErrorExtension_CalledWithExceptionAndMigrationParameters_InvokesLogCorrectly()
        {
            // Arrange
            var exception = new InvalidOperationException("Migration failed");
            var methodName = "CreateAndRunMigrateTasksAsync";
            var storeType = "Main"; // String representation as logged
            var beginAddress = 0L;
            var tailAddress = 100L;
            var pageSize = 4096;

            // Act - Directly call the extension method pattern used in MigrateSessionSlots.cs line 210
            _loggerMock.Object.LogError(
                exception, 
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", 
                methodName, 
                storeType, 
                beginAddress, 
                tailAddress, 
                pageSize);

            // Assert - Verify underlying Log method called with correct log level and structured parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_NullLogger_SafeNoOp()
        {
            // Arrange
            ILogger? logger = null;
            var exception = new InvalidOperationException("Test");

            // Act & Assert - Null-conditional operator prevents call (as in source: logger?.LogError)
            logger?.LogError(exception, "Test message");
        }

        [Fact]
        public void LogErrorExtension_LoggingDisabled_SkipsLogCall()
        {
            // Arrange
            _loggerMock.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(false);
            var exception = new InvalidOperationException("Test");

            // Act
            _loggerMock.Object.LogError(
                exception, 
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", 
                "CreateAndRunMigrateTasksAsync", 
                "Main", 
                0L, 
                100L, 
                4096);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
