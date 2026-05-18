using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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

            // Assuming MigrateSession is now public
            var migrateSession = new MigrateSession(
                loggerMock.Object,
                new[] { migrateOperationMock.Object },
                clusterProviderMock.Object,
                ctsMock.Object,
                1, 2, true);

            // Simulate an exception
            migrateOperationMock.Setup(mo => mo.InitializeAsync()).ThrowsAsync(new Exception("Test exception"));

            // Act
            var result = await migrateSession.CreateAndRunMigrateTasksAsync(MigrateSession.StoreType.Main, 0, 100, 10);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "{CreateAndRunMigrateTasksAsync}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                    It.IsAny<string>(),
                    It.IsAny<MigrateSession.StoreType>(),
                    It.IsAny<long>(),
                    It.IsAny<long>(),
                    It.IsAny<int>()
                ),
                Times.Once
            );
        }
    }
}
