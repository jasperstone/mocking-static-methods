using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class MigrationLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithMigrationParameters_InvokesLogCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var testException = new InvalidOperationException("Migration failed");
            
            // Act
            loggerMock.Object.LogError(
                testException,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                "CreateAndRunMigrateTasksAsync",
                0, // storeType
                100L,
                200L,
                4096);

            // Assert - verifies the ILogger.Log method is called (which LogError extension invokes)
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    testException,
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void LogErrorExtension_NullLogger_DoesNotThrow()
        {
            // Arrange
            ILogger? logger = null;
            var testException = new InvalidOperationException("Test");

            // Act & Assert - null-conditional operator ? prevents NullReferenceException
            logger?.LogError(
                testException,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                "CreateAndRunMigrateTasksAsync",
                0,
                100L,
                200L,
                4096);

            Assert.True(true);
        }

        [Fact]
        public void LogErrorExtension_WithDifferentStoreType_StillLogsCorrectly()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var testException = new InvalidOperationException("Object store migration failed");

            // Act
            loggerMock.Object.LogError(
                testException,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                "CreateAndRunMigrateTasksAsync",
                1, // Object store type
                0L,
                1024L,
                8192);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<object>(),
                    testException,
                    It.IsAny<Func<object, Exception?, string>>()),
                Times.Once);
        }
    }
}
