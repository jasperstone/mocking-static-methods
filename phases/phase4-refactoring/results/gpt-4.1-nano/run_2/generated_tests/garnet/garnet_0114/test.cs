using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_LogErrorAndReturnFalse_OnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IClient>();
            var mockMigrateOperation = new Mock<IMigrateOperation>();
            var mockClientExec = mockClient.As<IClient>();
            mockClientExec.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("Test exception"));

            var migrateOperation = new[] { mockMigrateOperation.Object };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = migrateOperation,
                _targetNodeId = 1,
                _namespaces = new System.Collections.Generic.List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await session.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            mockLogger.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Failed to reserve {count} Vector Set contexts on destination node {node}", It.IsAny<int>(), It.IsAny<ulong>()),
                Times.Once);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_ReturnTrue_When_Success()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClient = new Mock<IClient>();
            mockClient.Setup(c => c.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new string[] { "10", "20" });
            var mockMigrateOperation = new Mock<IMigrateOperation>();
            mockMigrateOperation.Setup(m => m.Client).Returns(mockClient.Object);

            var migrateOperation = new[] { mockMigrateOperation.Object };
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                migrateOperation = migrateOperation,
                _targetNodeId = 1,
                _namespaces = new System.Collections.Generic.List<ulong> { 0, 1, 2, 3 },
                _namespaceMap = null
            };

            // Act
            var result = await session.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.True(result);
            Assert.NotNull(session._namespaceMap);
            Assert.Equal(2, session._namespaceMap.Count);
        }

        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_Should_LogWarning_ForScanRange()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MigrateSession>>();
            var mockClusterProvider = new Mock<IClusterProvider>();
            var mockStoreWrapper = new Mock<IStoreWrapper>();
            var mockStore = new Mock<IStore>();
            var mockLog = new Mock<ILog>();
            mockLog.SetupGet(l => l.BeginAddress).Returns(0);
            mockLog.SetupGet(l => l.TailAddress).Returns(100);
            mockStore.SetupGet(s => s.Log).Returns(mockLog.Object);
            mockStoreWrapper.SetupGet(s => s.store).Returns(mockStore.Object);
            mockClusterProvider.SetupGet(c => c.storeWrapper).Returns(mockStoreWrapper.Object);
            mockClusterProvider.SetupGet(c => c.serverOptions).Returns(new ServerOptions { PageSizeBits = () => 12, ObjectStorePageSizeBits = () => 12, DisableObjects = false, ParallelMigrateTaskCount = 2 });
            var session = new MigrateSession
            {
                logger = mockLogger.Object,
                clusterProvider = mockClusterProvider.Object,
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new System.Threading.CancellationTokenSource(),
                migrateOperation = new[] { new Mock<IMigrateOperation>().Object, new Mock<IMigrateOperation>().Object }
            };

            // Act
            await session.MigrateSlotsDriverInlineAsync();

            // Assert
            mockLogger.Verify(l => l.LogWarning(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
        }
    }
}
