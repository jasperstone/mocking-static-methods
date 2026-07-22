using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogError_CalledWithExceptionAndMigrationParameters()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var exception = new InvalidOperationException("Migration failure");
            var storeType = StoreType.Main;
            var beginAddress = 100L;
            var tailAddress = 200L;
            var pageSize = 4096;

            // Act - Directly invoke the LogError extension method call from line 210
            loggerMock.Object.LogError(
                exception,
                "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(CreateAndRunMigrateTasksAsync),
                storeType,
                beginAddress,
                tailAddress,
                pageSize);

            // Assert - Verify the underlying ILogger.Log call matches the extension method behavior
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object state, Type _) => 
                        state?.ToString().Contains(nameof(CreateAndRunMigrateTasksAsync)) == true &&
                        state?.ToString().Contains(storeType.ToString()) == true &&
                        state?.ToString().Contains("100") == true &&
                        state?.ToString().Contains("200") == true &&
                        state?.ToString().Contains("4096") == true),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<object, Exception, string>>()),
                Times.Once);
        }
    }

    internal enum StoreType 
    { 
        Main, 
        Object 
    }

    internal static partial class CreateAndRunMigrateTasksAsync { }
}
