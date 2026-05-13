using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task RelinquishOwnership_Failure_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var migrationManagerMock = new Mock<IMigrationManager>();

            // Mock the necessary methods and properties
            clusterProviderMock.SetupGet(c => c.migrationManager).Returns(migrationManagerMock.Object);
            migrationManagerMock.Setup(m => m.TryRemoveMigrationTask(It.IsAny<MigrateSession>())).Returns(true);

            var migrationSession = new MigrateSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object,
                Status = MigrateState.INIT
            };

            // Mock the RelinquishOwnership method to return false
            var relinquishOwnershipMock = new Mock<Func<bool>>();
            relinquishOwnershipMock.Setup(m => m()).Returns(false);
            migrationSession.RelinquishOwnership = relinquishOwnershipMock.Object;

            // Act
            await migrationSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Failed to relinquish ownership from source node")),
                    It.IsAny<object>(),
                    It.IsAny<object>()
                ),
                Times.Once
            );
        }
    }
}
