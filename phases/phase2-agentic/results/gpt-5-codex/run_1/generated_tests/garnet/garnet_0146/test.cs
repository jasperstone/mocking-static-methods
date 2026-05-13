using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Cluster.Server.Replication
{
    public class CheckpointStoreTests
    {
        private readonly Type _checkpointStoreType;
        private readonly object _checkpointStore;
        private readonly Mock<IReplicationLogCheckpointManagerShim> _mainStoreManagerMock;
        private readonly Mock<IReplicationLogCheckpointManagerShim> _objectStoreManagerMock;
        private readonly Mock<ILogger> _loggerMock;
        private readonly TestClusterProvider _clusterProvider;

        public CheckpointStoreTests()
        {
            var clusterAssembly = typeof(Garnet.cluster.ClusterProvider).Assembly;
            _checkpointStoreType = clusterAssembly.GetType("Garnet.cluster.CheckpointStore", throwOnError: true)!;

            _loggerMock = new Mock<ILogger>(MockBehavior.Strict);

            _mainStoreManagerMock = new Mock<IReplicationLogCheckpointManagerShim>(MockBehavior.Strict);
            _objectStoreManagerMock = new Mock<IReplicationLogCheckpointManagerShim>(MockBehavior.Strict);

            _clusterProvider = new TestClusterProvider(_mainStoreManagerMock.Object, _objectStoreManagerMock.Object);

            var storeWrapper = Activator.CreateInstance(clusterAssembly.GetType("Garnet.cluster.StoreWrapper", throwOnError: true)!,
                BindingFlags.NonPublic | BindingFlags.Instance,
                binder: null,
                args: new object[] { _clusterProvider.StoreWrapper },
                culture: null);

            _checkpointStore = Activator.CreateInstance(
                _checkpointStoreType,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public,
                binder: null,
                args: new[] { storeWrapper, _clusterProvider.InnerProvider, false, _loggerMock.Object },
                culture: null)!;
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForNonMatchingIndexTokens()
        {
            // Arrange
            var retainedIndexToken = Guid.NewGuid();
            var deletingIndexToken = Guid.NewGuid();
            var entry = CheckpointEntryFactory.Create(retainedIndexToken);

            _mainStoreManagerMock
                .Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new[] { retainedIndexToken, deletingIndexToken });
            _mainStoreManagerMock
                .Setup(m => m.GetLogCheckpointTokens())
                .Returns(Array.Empty<Guid>());
            _mainStoreManagerMock
                .Setup(m => m.DeleteIndexCheckpoint(deletingIndexToken));

            _loggerMock
                .Setup(l => l.IsEnabled(LogLevel.Trace))
                .Returns(true);

            _loggerMock
                .Setup(l => l.Log(
                    It.Is<LogLevel>(lvl => lvl == LogLevel.Trace),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((state, _) => state.ToString() == $"Deleting index token {deletingIndexToken}"),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()))
                .Verifiable();

            // Act
            InvokePurgeAllCheckpointsExceptEntry(entry);

            // Assert
            _loggerMock.VerifyAll();
            _mainStoreManagerMock.Verify(m => m.DeleteIndexCheckpoint(deletingIndexToken), Times.Once);
            _mainStoreManagerMock.Verify(m => m.DeleteIndexCheckpoint(It.Is<Guid>(g => g == retainedIndexToken)), Times.Never);
        }

        private void InvokePurgeAllCheckpointsExceptEntry(object entry)
        {
            var method = _checkpointStoreType.GetMethod("PurgeAllCheckpointsExceptEntry", BindingFlags.Instance | BindingFlags.Public);
            method!.Invoke(_checkpointStore, new[] { entry });
        }

        private interface IReplicationLogCheckpointManagerShim
        {
            Guid[] GetLogCheckpointTokens();
            Guid[] GetIndexCheckpointTokens();
            void DeleteLogCheckpoint(Guid token);
            void DeleteIndexCheckpoint(Guid token);
        }

        private sealed class TestClusterProvider
        {
            public object InnerProvider { get; }
            public object StoreWrapper { get; }

            public TestClusterProvider(IReplicationLogCheckpointManagerShim mainManager, IReplicationLogCheckpointManagerShim objectManager)
            {
                var clusterType = typeof(Garnet.cluster.ClusterProvider);

                var replicationManagerField = clusterType.GetField("replicationManager", BindingFlags.NonPublic | BindingFlags.Instance);
                var serverOptionsField = clusterType.GetField("serverOptions", BindingFlags.NonPublic | BindingFlags.Instance);

                var innerProvider = FormatterServices.GetUninitializedObject(clusterType);
                replicationManagerField!.SetValue(innerProvider, new object());
                serverOptionsField!.SetValue(innerProvider, new object());

                var storeWrapperType = typeof(Garnet.cluster.StoreWrapper);
                StoreWrapper = FormatterServices.GetUninitializedObject(storeWrapperType);

                InnerProvider = innerProvider;
            }
        }

        private static class CheckpointEntryFactory
        {
            public static object Create(Guid indexToken)
            {
                var entryType = typeof(Garnet.cluster.ClusterProvider).Assembly.GetType("Garnet.cluster.CheckpointEntry", throwOnError: true)!;
                var metadataProperty = entryType.GetProperty("metadata", BindingFlags.Public | BindingFlags.Instance);
                var metadataType = metadataProperty!.PropertyType;
                var metadataInstance = Activator.CreateInstance(metadataType)!;

                var storeIndexTokenField = metadataType.GetField("storeIndexToken", BindingFlags.Public | BindingFlags.Instance);
                storeIndexTokenField!.SetValue(metadataInstance, indexToken);

                var entryInstance = Activator.CreateInstance(entryType, true)!;
                metadataProperty.SetValue(entryInstance, metadataInstance);
                return entryInstance;
            }
        }
    }
}
