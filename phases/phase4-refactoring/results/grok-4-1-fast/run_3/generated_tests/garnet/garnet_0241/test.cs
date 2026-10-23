using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.common;

namespace Garnet.cluster.Tests
{
    public class ReplicationReplicaAofSyncTests
    {
        [Fact]
        public unsafe void ProcessPrimaryStream_LogsError_WhenCannotStreamAOF()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            mockLogger
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Replica is recovering cannot sync AOF")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var replicationManager = CreateReplicationManager(mockLogger.Object);
            SetPrivateField(replicationManager, "clusterProvider", CreateMockClusterProvider(cannotStreamAOF: true));

            // Act
            var record = new byte[1];
            fixed (byte* ptr = record)
            {
                Assert.Throws<GarnetException>(() => InvokeProcessPrimaryStream(replicationManager, ptr, record.Length, 0L, 0L, 0L));
            }

            // Assert
            mockLogger.Verify();
        }

        [Fact]
        public unsafe void ProcessPrimaryStream_LogsError_WhenDivergentAOFStream()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<ReplicationManager>>();
            mockLogger
                .Setup(x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => ((string)v).Contains("Divergent AOF Stream")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Verifiable();

            var replicationManager = CreateReplicationManager(mockLogger.Object);
            var mockClusterProvider = CreateMockClusterProvider(cannotStreamAOF: false);
            
            var mockStoreWrapper = new Mock<object>();
            var mockAppendOnlyFile = new Mock<object>();
            SetPrivateField(mockStoreWrapper, "appendOnlyFile", mockAppendOnlyFile.Object);
            SetPrivateProperty(mockAppendOnlyFile.Object, "TailAddress", 100L);
            SetPrivateField(mockClusterProvider.Object, "storeWrapper", mockStoreWrapper.Object);
            
            SetPrivateField(replicationManager, "clusterProvider", mockClusterProvider.Object);
            SetPrivateField(replicationManager, "pageSizeBits", 12);

            // Act
            var record = new byte[5000];
            fixed (byte* ptr = record)
            {
                Assert.ThrowsAny<Exception>(() => InvokeProcessPrimaryStream(replicationManager, ptr, record.Length, 0L, 50L, 100L));
            }

            // Assert
            mockLogger.Verify();
        }

        private static ReplicationManager CreateReplicationManager(ILogger<ReplicationManager> logger)
        {
            var ctors = typeof(ReplicationManager).GetConstructors(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (ReplicationManager)ctors[0].Invoke(new object[] { logger, null! });
        }

        private static object CreateMockClusterProvider(bool cannotStreamAOF)
        {
            var mockClusterProvider = new Mock<object>();
            var mockReplicationManager = new Mock<object>();
            SetPrivateProperty(mockReplicationManager.Object, "CannotStreamAOF", cannotStreamAOF);
            SetPrivateField(mockClusterProvider.Object, "replicationManager", mockReplicationManager.Object);
            return mockClusterProvider.Object;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static void SetPrivateProperty(object target, string propName, object value)
        {
            var prop = target.GetType().GetProperty(propName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            prop?.SetValue(target, value);
        }

        private static void InvokeProcessPrimaryStream(object replicationManager, byte* record, int length, long prev, long curr, long next)
        {
            var method = replicationManager.GetType().GetMethod("ProcessPrimaryStream",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
                null, new[] { typeof(byte*), typeof(int), typeof(long), typeof(long), typeof(long) }, null)!;
            method.Invoke(replicationManager, new object[] { record, length, prev, curr, next });
        }
    }
}
