using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public void LogError_Called_When_CreateAndRunMigrateTasksAsync_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MigrateSession>>();
            var migrateSession = new MigrateSession(loggerMock.Object);

            // Act
            var ex = new Exception("Test exception");
            migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 0, 0);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
