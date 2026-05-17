using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Garnet.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task LogError_CallsLoggerLogError_WithExpectedArguments()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object);
            var exception = new Exception("Test exception");
            var storeType = StoreType.Main;
            var beginAddress = 0L;
            var tailAddress = 10L;
            var pageSize = 1024;

            // Act
            try
            {
                await migrateSession.MigrateSlotsDriverInlineAsync();
            }
            catch (Exception ex)
            {
                loggerMock.Object.LogError(ex, "{CreateAndRunMigrateTasks}: {storeType} {beginAddress} {tailAddress} {pageSize}", nameof(migrateSession.MigrateSlotsDriverInlineAsync), storeType, beginAddress, tailAddress, pageSize);
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
