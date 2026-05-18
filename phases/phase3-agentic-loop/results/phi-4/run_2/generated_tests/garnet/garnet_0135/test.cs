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
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenRelinquishOwnershipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var migrateSession = new MigrateSession
            {
                logger = loggerMock.Object,
                clusterProvider = clusterProviderMock.Object
            };

            // Mock the necessary methods to simulate the flow
            clusterProviderMock.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).Returns(Task.FromResult(true));
            clusterProviderMock.Setup(cp => cp.storeWrapper.DefaultDatabase.VectorManager.GetNamespacesForHashSlots(It.IsAny<int[]>()))
                .Returns(new List<string>());

            // Act
            await migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("Failed to relinquish ownership from source node")),
                    It.IsAny<string>(),
                    It.IsAny<string>()
                ),
                Times.Once
            );
        }
    }
}
