using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrateOperationMock = new Mock<IMigrateOperation>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var ctsMock = new Mock<System.Threading.CancellationTokenSource>();

            var migrateSession = new MigrateSession(
                loggerMock.Object,
                migrateOperationMock.Object,
                clusterProviderMock.Object,
                ctsMock.Object);

            // Simulate an exception
            var exception = new Exception("Test exception");
            migrateOperationMock.Setup(mo => mo.InitializeAsync()).ThrowsAsync(exception);

            // Act
            var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "{CreateAndRunMigrateTasksAsync}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                    nameof(MigrateSession.CreateAndRunMigrateTasksAsync),
                    StoreType.Main,
                    0,
                    100,
                    10),
                Times.Once);
        }
    }
}
