using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class CheckpointStoreTests
    {
        private class DummyCheckpointEntry : CheckpointEntry
        {
            public override CheckpointEntry next { get; set; }
            public override CheckpointEntry previous { get; set; }
            public override string ToString() => "DummyCheckpointEntry";
        }

        private class DummyClusterProvider : ClusterProvider
        {
            public override IReplicationLogCheckpointManager GetReplicationLogCheckpointManager(StoreType storeType)
            {
                var mock = new Mock<IReplicationLogCheckpointManager>();
                mock.Setup(m => m.GetLogCheckpointTokens()).Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
                mock.Setup(m => m.GetIndexCheckpointTokens()).Returns(new List<Guid> { Guid.NewGuid(), Guid.NewGuid() });
                mock.Setup(m => m.DeleteLogCheckpoint(It.IsAny<Guid>())).Verifiable();
                mock.Setup(m => m.DeleteIndexCheckpoint(It.IsAny<Guid>())).Verifiable();
                return mock.Object;
            }

            public override bool TryAcquireSettledMetadataForMainStore(CheckpointEntry entry, out object a, out object b)
            {
                a = b = null;
                return true;
            }

            public override bool TryAcquireSettledMetadataForObjectStore(CheckpointEntry entry, out object a, out object b)
            {
                a = b = null;
                return true;
            }
        }

        private class DummyLogger : ILogger
        {
            public List<string> LogMessages = new List<string>();
            public List<(LogLevel, string, object)> LogTraceMessages = new List<(LogLevel, string, object)>();
            public List<(LogLevel, string, CheckpointEntry)> LogCheckpointEntries = new List<(LogLevel, string, CheckpointEntry)>();

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LogMessages.Add(formatter(state, exception));
            }

            public void LogCheckpointEntry(LogLevel level, string method, CheckpointEntry entry)
            {
                LogCheckpointEntries.Add((level, method, entry));
            }

            public void LogTrace(string message, params object[] args)
            {
                LogTraceMessages.Add((LogLevel.Trace, message, args));
            }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_ShouldLogTrace_WhenCalled()
        {
            // Arrange
            var mockLogger = new DummyLogger();
            var storeWrapper = new Mock<StoreWrapper>().Object;
            var clusterProvider = new DummyClusterProvider();
            var store = new CheckpointStore(storeWrapper, clusterProvider, false, mockLogger);

            var entry = new DummyCheckpointEntry
            {
                metadata = new CheckpointEntry.CheckpointMetadata
                {
                    storeHlogToken = Guid.NewGuid(),
                    storeIndexToken = Guid.NewGuid(),
                    objectStoreHlogToken = Guid.NewGuid(),
                    objectStoreIndexToken = Guid.NewGuid()
                }
            };

            // Act
            store.PurgeAllCheckpointsExceptEntry(entry);

            // Assert
            Assert.Contains(mockLogger.LogCheckpointEntries, e => e.Item2 == "PurgeAllCheckpointsExceptEntry");
            Assert.Contains(mockLogger.LogCheckpointEntries, e => e.Item2 == "Deleting log token");
            Assert.Contains(mockLogger.LogCheckpointEntries, e => e.Item2 == "Deleting index token");
        }
    }
}
