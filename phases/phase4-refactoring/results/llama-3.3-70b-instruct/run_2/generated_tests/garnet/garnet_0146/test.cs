using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using Garnet.cluster;
using System.Threading;
using System.Diagnostics;
using Garnet.common;
using Garnet.server;
using Tsavorite.core;
using Microsoft.Extensions.Logging.Abstractions;

namespace GarnetTest
{
    public class CheckpointStoreTests
    {
        [Fact]
        public void PurgeAllCheckpointsExceptEntry_LogTraceCalled()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<CheckpointStore>>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var clusterProviderMock = new Mock<IClusterProvider>();
            var checkpointStore = new CheckpointStore(storeWrapperMock.Object, clusterProviderMock.Object, false, loggerMock.Object);

            // Act
            checkpointStore.PurgeAllCheckpointsExceptEntry();

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.IsAny<object>(),
                It.IsAny<Exception>(),
                (state, exception) => true),
                Times.AtLeastOnce);
        }
    }
}
