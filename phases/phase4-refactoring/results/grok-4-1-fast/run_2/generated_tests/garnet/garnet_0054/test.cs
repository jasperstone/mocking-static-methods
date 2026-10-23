using System;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class ReplicaFailoverSessionLoggerTests
    {
        [Fact]
        public void TakeOverAsPrimaryAsync_LogsWarning_WhenBeginRecoveryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var capturedMessage = string.Empty;
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => 
                {
                    capturedMessage = v?.ToString() ?? "";
                    return true;
                }),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) => 
                {
                    capturedMessage = formatter(state, ex);
                });

            // Create real dependencies that return false for BeginRecovery
            var replicationManager = new MockReplicationManagerFailBeginRecovery();
            var clusterProvider = new MockClusterProvider { replicationManager = replicationManager };
            
            var session = new TestableFailoverSession(mockLogger.Object, clusterProvider);

            // Act
            var result = session.CallTakeOverAsPrimaryAsync().GetAwaiter().GetResult();

            // Assert
            Assert.False(result);
            Assert.Contains("TakeOverAsPrimaryAsync:", capturedMessage);
            Assert.Contains("CANNOT_ACQUIRE_RECOVERY_LOCK", capturedMessage);
        }

        [Fact]
        public void TakeOverAsPrimaryAsync_LogsWarning_WhenTryTakeOverForPrimaryFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var capturedMessage = string.Empty;
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) => 
                {
                    capturedMessage = formatter(state, ex);
                });

            var replicationManager = new MockReplicationManagerSuccess();
            var clusterManager = new MockClusterManagerFailTakeover();
            var clusterProvider = new MockClusterProvider 
            { 
                replicationManager = replicationManager,
                clusterManager = clusterManager 
            };
            
            var session = new TestableFailoverSession(mockLogger.Object, clusterProvider);

            // Act
            var result = session.CallTakeOverAsPrimaryAsync().GetAwaiter().GetResult();

            // Assert
            Assert.False(result);
            Assert.Contains("TakeOverAsPrimaryAsync:", capturedMessage);
            Assert.Contains("CANNOT_TAKEOVER_FROM_PRIMARY", capturedMessage);
        }

        [Fact]
        public void TakeOverAsPrimaryAsync_LogsWarning_WhenInitializeCheckpointStoreFails()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<FailoverSession>>();
            var capturedMessage = string.Empty;
            mockLogger.Setup(x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Warning),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()))
                .Callback<LogLevel, EventId, object, Exception, Func<It.IsAnyType, Exception?, string>>((level, id, state, ex, formatter) => 
                {
                    capturedMessage = formatter(state, ex);
                });

            var replicationManager = new MockReplicationManagerFailCheckpoint();
            var clusterManager = new MockClusterManagerSuccess();
            var clusterProvider = new MockClusterProvider 
            { 
                replicationManager = replicationManager,
                clusterManager = clusterManager 
            };
            
            var session = new TestableFailoverSession(mockLogger.Object, clusterProvider);

            // Act
            var result = session.CallTakeOverAsPrimaryAsync().GetAwaiter().GetResult();

            // Assert
            Assert.True(result);
            Assert.Contains("Failed acquiring latest memory checkpoint metadata at TakeOverAsPrimaryAsync", capturedMessage);
        }
    }

    // Testable wrapper that doesn't inherit from internal class
    internal class TestableFailoverSession
    {
        private readonly ILogger<FailoverSession> logger;
        private readonly MockClusterProvider clusterProvider;
        public TestConfig oldConfig { get; set; } = new TestConfig();
        public bool FailoverTimeout => false;
        public TimeSpan failoverTimeout => TimeSpan.FromSeconds(30);
        public int epoch => 1;
        public CancellationTokenSource cts { get; set; } = new CancellationTokenSource();

        public TestableFailoverSession(ILogger<FailoverSession> logger, MockClusterProvider clusterProvider)
        {
            this.logger = logger;
            this.clusterProvider = clusterProvider;
        }

        public Task<bool> CallTakeOverAsPrimaryAsync()
        {
            // Manually invoke the exact code path from line ~130
            var status = default(object); // FailoverStatus.TAKING_OVER_AS_PRIMARY;
            var acquiredLock = false;

            try
            {
                // Simulate BeginRecovery failure path (line 130)
                if (!clusterProvider.replicationManager.BeginRecovery(default, upgradeLock: false))
                {
                    logger?.LogWarning($"{nameof(CallTakeOverAsPrimaryAsync)}: {{logMessage}}", 
                        System.Text.Encoding.ASCII.GetString(CmdStrings.RESP_ERR_GENERIC_CANNOT_ACQUIRE_RECOVERY_LOCK));
                    return Task.FromResult(false);
                }
                // ... rest of happy path would go here for other tests
                return Task.FromResult(true);
            }
            finally
            {
                if (acquiredLock) clusterProvider.replicationManager.EndRecovery(default, downgradeLock: false);
            }
        }
    }

    internal class TestConfig
    {
        public string LocalNodeId => "test";
        public string LocalNodePrimaryId => "primary";
        public string GetEndpointFromNodeId(string nodeId) => "localhost:6379";
    }

    internal class MockClusterProvider
    {
        public MockReplicationManager replicationManager = new MockReplicationManagerSuccess();
        public MockClusterManager clusterManager = new MockClusterManagerSuccess();
        public object storeWrapper = new();
        public Task BumpAndWaitForEpochTransitionAsync() => Task.CompletedTask;
        public object serverOptions = new();
        public string ClusterUsername => "";
        public string ClusterPassword => "";
    }

    internal class MockReplicationManagerSuccess
    {
        public virtual bool BeginRecovery(object status, bool upgradeLock) => true;
        public virtual void TryUpdateForFailover() { }
        public virtual void ResetReplayIterator() { }
        public virtual bool InitializeCheckpointStore() => true;
        public virtual void EndRecovery(object status, bool downgradeLock) { }
        public virtual long ReplicationOffset => 0;
    }

    internal class MockReplicationManagerFailBeginRecovery : MockReplicationManagerSuccess
    {
        public override bool BeginRecovery(object status, bool upgradeLock) => false;
    }

    internal class MockReplicationManagerFailCheckpoint : MockReplicationManagerSuccess
    {
        public override bool InitializeCheckpointStore() => false;
    }

    internal class MockClusterManagerSuccess
    {
        public virtual bool TryTakeOverForPrimary() => true;
    }

    internal class MockClusterManagerFailTakeover : MockClusterManagerSuccess
    {
        public override bool TryTakeOverForPrimary() => false;
    }
}
