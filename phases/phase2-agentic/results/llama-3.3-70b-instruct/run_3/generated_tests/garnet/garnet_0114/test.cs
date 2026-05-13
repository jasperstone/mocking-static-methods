using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public void LogError_OnException_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var exception = new Exception("Test exception");

            // Act
            migrateSession.LogError(exception, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", "CreateAndRunMigrateTasks", "StoreType", 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
