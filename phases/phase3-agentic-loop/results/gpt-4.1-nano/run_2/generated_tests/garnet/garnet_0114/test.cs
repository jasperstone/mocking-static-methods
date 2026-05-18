using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        private readonly Mock<ILogger<MigrateSession>> _loggerMock;
        private readonly Mock<IGenerateMigrateOperation> _migrateOperationMock;
        private readonly Mock<IClusterProvider> _clusterProviderMock;
        private readonly MigrateSession _migrateSession;

        public MigrateSessionTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _migrateOperationMock = new Mock<IGenerateMigrateOperation>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            _migrateSession = new MigrateSession(
                _loggerMock.Object,
                _migrateOperationMock.Object,
                _clusterProviderMock.Object);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_LogErrorAndReturnFalse_OnException()
        {
            // Arrange
            var neededContexts = 2;
            var exception = new Exception("Test exception");
            _migrateOperationMock.Setup(m => m[0].Client.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(exception);
            _migrateSession._namespaces = new ulong[] { 0, 1, 2, 3 };
            _migrateSession._targetNodeId = 123UL;

            // Act
            var result = await _migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to reserve")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_ReturnTrue_When_Successful()
        {
            // Arrange
            var reservedCtxs = new[] { "100", "101" };
            _migrateOperationMock.Setup(m => m[0].Client.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(reservedCtxs);
            _migrateSession._namespaces = new ulong[] { 0, 1, 2, 3 };
            _migrateSession._targetNodeId = 123UL;

            // Act
            var result = await _migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.True(result);
            Assert.NotNull(_migrateSession._namespaceMap);
            Assert.Equal(4, _migrateSession._namespaceMap.Count);
        }

        [Fact]
        public async Task ReserveDestinationVectorSetsAsync_Should_ReturnFalse_When_Exception()
        {
            // Arrange
            _migrateOperationMock.Setup(m => m[0].Client.ExecuteForArrayAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception("fail"));

            // Act
            var result = await _migrateSession.ReserveDestinationVectorSetsAsync();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task MigrateSlotsDriverInlineAsync_Should_ReturnFalse_If_CreateAndRunMigrateTasksAsync_Fails()
        {
            // Arrange
            _clusterProviderMock.Setup(c => c.storeWrapper.store.Log.BeginAddress).Returns(0);
            _clusterProviderMock.Setup(c => c.storeWrapper.store.Log.TailAddress).Returns(100);
            _clusterProviderMock.Setup(c => c.serverOptions.PageSizeBits()).Returns(4);
            _clusterProviderMock.Setup(c => c.serverOptions.DisableObjects).Returns(false);
            _clusterProviderMock.Setup(c => c.storeWrapper.objectStore.Log.BeginAddress).Returns(0);
            _clusterProviderMock.Setup(c => c.storeWrapper.objectStore.Log.TailAddress).Returns(100);
            _clusterProviderMock.Setup(c => c.serverOptions.ObjectStorePageSizeBits()).Returns(4);
            _clusterProviderMock.Setup(c => c.serverOptions.ParallelMigrateTaskCount).Returns(2);
            // Setup CreateAndRunMigrateTasksAsync to return false to simulate failure
            var mock = new Mock<MigrateSession>(_loggerMock.Object, _migrateOperationMock.Object, _clusterProviderMock.Object);
            mock.Setup(m => m.CreateAndRunMigrateTasksAsync(It.IsAny<StoreType>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<int>()))
                .ReturnsAsync(false);
            var migrateInstance = mock.Object;

            // Act
            var result = await migrateInstance.MigrateSlotsDriverInlineAsync();

            // Assert
            Assert.False(result);
        }
    }
}
