using System;
using System.Text;
using System.Threading.Tasks;
using Garnet.cluster;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class ReplicationManagerTests
    {
        [Fact]
        public async Task TryReplicateDiskbasedSyncAsync_ReturnsFalseOnNullOptions()
        {
            // Arrange
            var replicationManagerType = typeof(ReplicationManager);
            var ctor = replicationManagerType.GetConstructor(
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, Type.EmptyTypes, null);
            Assert.NotNull(ctor);
            var replicationManager = ctor.Invoke(null);

            // Act
            var method = replicationManagerType.GetMethod("TryReplicateDiskbasedSyncAsync",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            Assert.NotNull(method);
            var task = (Task<(bool Success, ReadOnlyMemory<byte> ErrorMessage)>)method.Invoke(replicationManager, new object[] { null, null });
            var result = await task;

            // Assert
            Assert.False(result.Success);
            Assert.True(result.ErrorMessage.Length > 0 || !result.ErrorMessage.IsEmpty);
        }
    }
}
