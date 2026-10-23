using System;
using System.Collections.Generic;
using System.Threading;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace CheckpointStoreTests
{
    public class CheckpointStoreLoggingTests
    {
        private class TestCheckpointEntry : CheckpointEntry
        {
            public override string ToString() => "TestCheckpointEntry";
        }

        private class TestCheckpointStore : CheckpointStore
        {
            private readonly CheckpointEntry _testEntry;

            public TestCheckpointStore(CheckpointEntry testEntry, ILogger logger)
                : base(
                    storeWrapper: null,
                    clusterProvider: new Mock<ClusterProvider>().Object,
                    safelyRemoveOutdated: false,
                    logger: logger)
            {
                _testEntry = testEntry;
            }

            public override CheckpointEntry GetLatestCheckpointEntryFromDisk()
            {
                return _testEntry;
            }
        }

        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogsTrace()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var testEntry = new TestCheckpointEntry();
            var store = new TestCheckpointStore(testEntry, mockLogger.Object);

            // Act
            store.PurgeAllCheckpointsExceptEntry();

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("PurgeAllCheckpointsExceptEntry")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
