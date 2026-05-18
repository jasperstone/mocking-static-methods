using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.cluster
{
    public class CheckpointStoreTests
    {
        // Minimal interface to mock checkpoint manager behavior
        public interface IReplicationLogCheckpointManager
        {
            IEnumerable<Guid> GetLogCheckpointTokens();
            IEnumerable<Guid> GetIndexCheckpointTokens();
            void DeleteLogCheckpoint(Guid token);
            void DeleteIndexCheckpoint(Guid token);
        }

        // Enum matching StoreType in production code
        public enum StoreType
        {
            Main,
            Object
        }

        // Minimal GarnetServerOptions class for mocking
        public class GarnetServerOptions
        {
            public bool DisableObjects { get; set; }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTraceForDeletingIndexToken()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Create tokens
            var mainLogToken = Guid.NewGuid();
            var mainIndexToken = Guid.NewGuid();
            var mainOtherLogToken = Guid.NewGuid();
            var mainOtherIndexToken = Guid.NewGuid();

            var objectLogToken = Guid.NewGuid();
            var objectIndexToken = Guid.NewGuid();
            var objectOtherLogToken = Guid.NewGuid();
            var objectOtherIndexToken = Guid.NewGuid();

            // Create a CheckpointEntry instance with metadata set via reflection
            var entry = (CheckpointEntry)Activator.CreateInstance(typeof(CheckpointEntry), nonPublic: true);
            var metadataField = typeof(CheckpointEntry).GetField("metadata", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            var metadataType = metadataField.FieldType;
            var metadata = Activator.CreateInstance(metadataType);
            metadataType.GetField("storeHlogToken").SetValue(metadata, mainLogToken);
            metadataType.GetField("storeIndexToken").SetValue(metadata, mainIndexToken);
            metadataType.GetField("objectStoreHlogToken").SetValue(metadata, objectLogToken);
            metadataType.GetField("objectStoreIndexToken").SetValue(metadata, objectIndexToken);
            metadataField.SetValue(entry, metadata);

            // Setup mock checkpoint managers
            var mainManagerMock = new Mock<IReplicationLogCheckpointManager>();
            mainManagerMock.Setup(m => m.GetLogCheckpointTokens()).Returns(new[] { mainLogToken, mainOtherLogToken });
            mainManagerMock.Setup(m => m.GetIndexCheckpointTokens()).Returns(new[] { mainIndexToken, mainOtherIndexToken });
            mainManagerMock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>()));
            mainManagerMock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>()));

            var objectManagerMock = new Mock<IReplicationLogCheckpointManager>();
            objectManagerMock.Setup(m => m.GetLogCheckpointTokens()).Returns(new[] { objectLogToken, objectOtherLogToken });
            objectManagerMock.Setup(m => m.GetIndexCheckpointTokens()).Returns(new[] { objectIndexToken, objectOtherIndexToken });
            objectManagerMock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>()));
            objectManagerMock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>()));

            // Setup mock ClusterProvider
            var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict, null);
            clusterProviderMock.Setup(cp => cp.GetType().GetMethod("GetReplicationLogCheckpointManager", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                .Returns(null); // We will setup a helper method below

            // Setup GetReplicationLogCheckpointManager method via Moq's Setup using reflection invoke
            clusterProviderMock.Setup(cp => cp.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns<StoreType>(storeType =>
                {
                    return storeType switch
                    {
                        StoreType.Main => mainManagerMock.Object,
                        StoreType.Object => objectManagerMock.Object,
                        _ => throw new ArgumentOutOfRangeException(nameof(storeType))
                    };
                });

            // Setup serverOptions property via reflection
            var serverOptions = new GarnetServerOptions { DisableObjects = false };
            var serverOptionsField = typeof(ClusterProvider).GetField("serverOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            serverOptionsField.SetValue(clusterProviderMock.Object, serverOptions);

            // Create CheckpointStore instance via reflection
            var checkpointStore = (CheckpointStore)Activator.CreateInstance(
                typeof(CheckpointStore),
                BindingFlags.Instance | BindingFlags.NonPublic,
                binder: null,
                args: new object[] { null, clusterProviderMock.Object, false, loggerMock.Object },
                culture: null);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            // Verify logger.LogTrace called for deleting tokens not equal to entry tokens
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token") && v.ToString().Contains(mainOtherLogToken.ToString())),
                null,
                It.IsAny<Func<object, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token") && v.ToString().Contains(mainOtherIndexToken.ToString())),
                null,
                It.IsAny<Func<object, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting log token") && v.ToString().Contains(objectOtherLogToken.ToString())),
                null,
                It.IsAny<Func<object, Exception, string>>()), Times.Once);

            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting index token") && v.ToString().Contains(objectOtherIndexToken.ToString())),
                null,
                It.IsAny<Func<object, Exception, string>>()), Times.Once);
        }
    }
}
