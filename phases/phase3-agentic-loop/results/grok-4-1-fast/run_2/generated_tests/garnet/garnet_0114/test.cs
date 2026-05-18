using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class MigrateSessionLoggerTests
    {
        [Fact]
        public void LogErrorExtension_CalledWithExceptionAndParameters_InvokesUnderlyingLog()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var logger = mockLogger.Object;
            var exception = new InvalidOperationException("Migration failed");
            
            // Act - Directly call the logger extension with the exact format from line 210
            logger.LogError(exception, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", 
                "CreateAndRunMigrateTasksAsync", "Main", 0L, 1000L, 4096);

            // Assert - Verify the underlying ILogger.Log method was called with Error level
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
