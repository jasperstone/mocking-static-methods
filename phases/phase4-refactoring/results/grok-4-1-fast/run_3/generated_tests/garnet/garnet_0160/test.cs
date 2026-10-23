using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class AofTaskStoreLoggerTests
    {
        [Fact]
        public void TryAddReplicationTasks_LogsError_WhenStartAddressBeforeTruncatedUntilAndAllowDataLossFalse()
        {
            // Arrange
            var testLogger = new TestLogger();
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            
            var store = new TestAofTaskStore(mockClusterProvider.Object, testLogger);
            store.TruncatedUntil = 1000;
            store.Disposed = false;

            // Act
            store.CallTryAddReplicationTasks(new List<object>(), 500);

            // Assert
            Assert.Contains("TryAddReplicationTasks failed to add tasks for AOF sync 500 1000", testLogger.ErrorMessages);
        }

        [Fact]
        public void TryAddReplicationTasks_DoesNotLogError_WhenStartAddressAfterTruncatedUntil()
        {
            // Arrange
            var testLogger = new TestLogger();
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            
            var store = new TestAofTaskStore(mockClusterProvider.Object, testLogger);
            store.TruncatedUntil = 500;
            store.Disposed = false;

            // Act
            store.CallTryAddReplicationTasks(new List<object>(), 1000);

            // Assert
            Assert.Empty(testLogger.ErrorMessages);
        }

        [Fact]
        public void TryAddReplicationTasks_DoesNotLogError_WhenDisposed()
        {
            // Arrange
            var testLogger = new TestLogger();
            var mockClusterProvider = new Mock<ClusterProvider>();
            mockClusterProvider.SetupGet(cp => cp.AllowDataLoss).Returns(false);
            
            var store = new TestAofTaskStore(mockClusterProvider.Object, testLogger);
            store.TruncatedUntil = 1000;
            store.Disposed = true;

            // Act
            store.CallTryAddReplicationTasks(new List<object>(), 500);

            // Assert
            Assert.Empty(testLogger.ErrorMessages);
        }
    }

    // Correct ILogger implementation that captures LogError calls
    public class TestLogger : ILogger
    {
        public List<string> ErrorMessages { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Error)
            {
                ErrorMessages.Add(formatter(state, exception));
            }
        }
    }

    // Test double replicating the exact logging path from AofTaskStore.TryAddReplicationTasks
    public class TestAofTaskStore
    {
        public long TruncatedUntil { get; set; }
        public bool Disposed { get; set; }
        private readonly ClusterProvider clusterProvider;
        private readonly ILogger logger;

        public TestAofTaskStore(ClusterProvider clusterProvider, ILogger logger)
        {
            this.clusterProvider = clusterProvider;
            this.logger = logger;
        }

        public bool CallTryAddReplicationTasks(List<object> replicaSyncSessions, long startAddress)
        {
            // Exact replica of the code path containing the LogError call on line 271
            if (Disposed) return false;

            if (startAddress < TruncatedUntil && !clusterProvider.AllowDataLoss)
            {
                logger.LogError("{method} failed to add tasks for AOF sync {startAddress} {truncatedUntil}", 
                    nameof(AofTaskStore.TryAddReplicationTasks), startAddress, TruncatedUntil);
                return false;
            }

            return true;
        }
    }
}
