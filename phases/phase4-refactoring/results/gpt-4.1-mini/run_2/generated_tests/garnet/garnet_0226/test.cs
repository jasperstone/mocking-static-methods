using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_LogsForegroundCheckpointRetrieval()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var replicationManager = (ReplicationManager)Activator.CreateInstance(typeof(ReplicationManager), nonPublic: true);

            // Use reflection to set private logger field
            var loggerField = typeof(ReplicationManager).GetField("logger", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(loggerField);
            loggerField.SetValue(replicationManager, loggerMock.Object);

            // Create minimal ReplicateSyncOptions instance with Background = false
            var optionsType = Type.GetType("Garnet.cluster.ReplicateSyncOptions, Garnet.cluster");
            Assert.NotNull(optionsType);
            var options = Activator.CreateInstance(optionsType);
            optionsType.GetProperty("NodeId").SetValue(options, 1);
            optionsType.GetProperty("TryAddReplica").SetValue(options, false);
            optionsType.GetProperty("Background").SetValue(options, false);
            optionsType.GetProperty("UpgradeLock").SetValue(options, false);
            optionsType.GetProperty("Force").SetValue(options, false);

            // Act
            var task = (Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)>)typeof(ReplicationManager)
                .GetMethod("TryReplicateDiskbasedSyncAsync", BindingFlags.Instance | BindingFlags.Public)
                .Invoke(replicationManager, new object[] { null, options });
            var result = await task;

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Initiating foreground checkpoint retrieval")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
