using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task TryStartMigrationTaskAsync_LogsError_WhenSetRemoteSlotsToImportStateFails()
        {
            var loggerMock = new Mock<ILogger>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            loggerFactoryMock.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(loggerMock.Object);

            // Create a MigrateSession with a clusterProvider that will cause TrySetSlotRangesAsync to fail
            var clusterProvider = new TestClusterProvider(loggerFactoryMock.Object);

            var migrateSession = new MigrateSession(
                clusterSession: null,
                clusterProvider: clusterProvider,
                _targetAddress: "127.0.0.1",
                _targetPort: 6379,
                _targetNodeId: "targetNode",
                _username: null,
                _passwd: null,
                _sourceNodeId: "sourceNode",
                _copyOption: false,
                _replaceOption: false,
                _timeout: 1000,
                _slots: new HashSet<int> { 1, 2, 3 },
                sketch: null,
                transferOption: TransferOption.SLOTS);

            // Set the clusterProvider to fail TrySetSlotRangesAsync for import state
            clusterProvider.SetTrySetSlotRangesResult(false);

            // Call TryStartMigrationTaskAsync to trigger BeginAsyncMigrationTaskAsync in background
            await migrateSession.TryStartMigrationTaskAsync();

            // Wait a bit for the background task to run
            await Task.Delay(200);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set remote slots")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }

        private class TestClusterProvider : ClusterProvider
        {
            private bool _trySetSlotRangesResult = true;
            private readonly ILogger _logger;

            public TestClusterProvider(ILoggerFactory loggerFactory) : base(loggerFactory)
            {
                _logger = loggerFactory.CreateLogger("TestClusterProvider");
            }

            public void SetTrySetSlotRangesResult(bool result)
            {
                _trySetSlotRangesResult = result;
            }

            public override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                return Task.FromResult(_trySetSlotRangesResult);
            }
        }
    }
}
