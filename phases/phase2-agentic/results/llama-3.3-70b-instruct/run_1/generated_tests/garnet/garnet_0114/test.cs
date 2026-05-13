using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public void LogError_CallsLoggerLogError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var exception = new Exception("Test exception");

            // Act
            migrateSession.LogError(exception, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", "CreateAndRunMigrateTasks", StoreType.Main, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError(exception, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", "CreateAndRunMigrateTasks", StoreType.Main, 0, 0, 0), Times.Once);
        }
    }
}
