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
            var migrateSession = new MigrateSession(
                new ClusterSession(),
                new ClusterProvider(),
                "localhost",
                6379,
                "node1",
                "username",
                "password",
                "node2",
                true,
                true,
                1000,
                new HashSet<int> { 1, 2, 3 },
                new Sketch(),
                TransferOption.SLOTS
            );
            migrateSession.logger = loggerMock.Object;
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

    public enum StoreType
    {
        Main,
        Object
    }
}
