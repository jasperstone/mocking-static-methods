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
        public async Task LogErrorCalledWhenRelinquishOwnershipFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var clusterProviderMock = new Mock<ClusterProvider>();
            var migrateSession = new MigrateSession(loggerMock.Object, clusterProviderMock.Object);

            // Mock the RelinquishOwnership method to return false
            var migrateSessionMock = new Mock<MigrateSession>(loggerMock.Object, clusterProviderMock.Object);
            migrateSessionMock.Setup(m => m.RelinquishOwnership()).Returns(false);

            // Act
            await migrateSessionMock.Object.BeginAsyncMigrationTaskAsync();

            // Assert
            loggerMock.Verify(
                l => l.LogError(
                    It.Is<string>(s => s.Contains("Failed to relinquish ownership from source node")),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<object>(),
                    It.IsAny<Exception>()
                ),
                Times.Once
            );
        }
    }
}
