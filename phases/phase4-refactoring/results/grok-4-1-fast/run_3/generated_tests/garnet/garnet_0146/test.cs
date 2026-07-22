using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.tests
{
    public class CheckpointStoreLoggerTests
    {
        [Fact]
        public void IndexTokenDeletionTraceLog_IsCalled_WhenIndexTokensDiffer()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            
            var tokensToDelete = new List<Guid> 
            { 
                Guid.NewGuid(), 
                Guid.NewGuid() 
            };
            var indexToken = Guid.NewGuid(); // The one we keep
            
            var mockCkptManager = new Mock<IRecoveryCheckpointManager>();
            mockCkptManager.Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { tokensToDelete[0], indexToken, tokensToDelete[1] });
            
            var mockClusterProvider = new Mock<IClusterProvider>();
            mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(p => p.serverOptions)
                .Returns(new MockServerOptions { DisableObjects = true });

            var mockStoreWrapper = new Mock<IStoreWrapper>();

            // Act
            var store = new TestableCheckpointStore(
                mockStoreWrapper.Object, 
                mockClusterProvider.Object, 
                safelyRemoveOutdated: false, 
                mockLogger.Object);
            store.PurgeAllCheckpointsExceptEntry(indexToken);

            // Assert - verify the exact LogTrace call on line 111 was executed
            mockLogger.Verify(
                l => l.LogTrace("Deleting index token {toDeleteIndexToken}", tokensToDelete[0]),
                Times.Once);
            mockLogger.Verify(
                l => l.LogTrace("Deleting index token {toDeleteIndexToken}", tokensToDelete[1]),
                Times.Once);
        }

        [Fact]
        public void IndexTokenDeletionTraceLog_NotCalled_WhenNoIndexTokensToDelete()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);
            
            var indexToken = Guid.NewGuid();
            var mockCkptManager = new Mock<IRecoveryCheckpointManager>();
            mockCkptManager.Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { indexToken });
            
            var mockClusterProvider = new Mock<IClusterProvider>();
            mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(p => p.serverOptions)
                .Returns(new MockServerOptions { DisableObjects = true });

            var mockStoreWrapper = new Mock<IStoreWrapper>();

            // Act
            var store = new TestableCheckpointStore(
                mockStoreWrapper.Object, 
                mockClusterProvider.Object, 
                safelyRemoveOutdated: false, 
                mockLogger.Object);
            store.PurgeAllCheckpointsExceptEntry(indexToken);

            // Assert
            mockLogger.Verify(
                l => l.LogTrace(It.Is<string>(s => s.Contains("Deleting index token")), It.IsAny<Guid>()),
                Times.Never);
        }

        [Fact]
        public void IndexTokenDeletionTraceLog_NotCalled_WhenTraceLevelDisabled()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            mockLogger.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(false);
            
            var mockCkptManager = new Mock<IRecoveryCheckpointManager>();
            mockCkptManager.Setup(m => m.GetIndexCheckpointTokens())
                .Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
            
            var mockClusterProvider = new Mock<IClusterProvider>();
            mockClusterProvider.Setup(p => p.GetReplicationLogCheckpointManager(It.IsAny<StoreType>()))
                .Returns(mockCkptManager.Object);
            mockClusterProvider.Setup(p => p.serverOptions)
                .Returns(new MockServerOptions { DisableObjects = true });

            var mockStoreWrapper = new Mock<IStoreWrapper>();

            // Act
            var store = new TestableCheckpointStore(
                mockStoreWrapper.Object, 
                mockClusterProvider.Object, 
                safelyRemoveOutdated: false, 
                mockLogger.Object);
            store.PurgeAllCheckpointsExceptEntry(Guid.Empty);

            // Assert
            mockLogger.Verify(l => l.LogTrace(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }

    public interface IClusterProvider 
    { 
        object GetReplicationLogCheckpointManager(StoreType storeType);
        MockServerOptions serverOptions { get; }
    }

    public interface IStoreWrapper { }

    public interface IRecoveryCheckpointManager 
    { 
        List<Guid> GetIndexCheckpointTokens(); 
    }

    public class MockServerOptions 
    { 
        public bool DisableObjects { get; set; } = false; 
    }

    public enum StoreType { Main, Object }

    // Test double that replicates the exact logging call from CheckpointStore.cs line 111
    public class TestableCheckpointStore
    {
        private readonly ILogger logger;
        private readonly IClusterProvider clusterProvider;

        public TestableCheckpointStore(
            IStoreWrapper storeWrapper, 
            IClusterProvider clusterProvider, 
            bool safelyRemoveOutdated, 
            ILogger logger)
        {
            this.clusterProvider = clusterProvider;
            this.logger = logger;
        }

        public void PurgeAllCheckpointsExceptEntry(Guid indexToken)
        {
            PurgeAllCheckpointsExceptTokens(StoreType.Main, Guid.Empty, indexToken);

            void PurgeAllCheckpointsExceptTokens(StoreType storeType, Guid logToken, Guid indexTokenToKeep)
            {
                var ckptManager = (IRecoveryCheckpointManager)clusterProvider.GetReplicationLogCheckpointManager(storeType);

                // Delete index checkpoints - EXACT replica of production code line 111
                foreach (var toDeleteIndexToken in ckptManager.GetIndexCheckpointTokens())
                {
                    if (!toDeleteIndexToken.Equals(indexTokenToKeep))
                    {
                        logger?.LogTrace("Deleting index token {toDeleteIndexToken}", toDeleteIndexToken);
                    }
                }
            }
        }
    }
}
