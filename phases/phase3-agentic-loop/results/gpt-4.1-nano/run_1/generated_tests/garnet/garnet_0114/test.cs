using System;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrateSessionTests
    {
        private readonly Mock<ILogger<MigrateSession>> _loggerMock;
        private readonly Mock<ILogger> _loggerExtensionMock;
        private readonly Mock<IMigrateOperation> _migrateOperationMock;
        private readonly Mock<IClusterProvider> _clusterProviderMock;
        private readonly Mock<IStoreWrapper> _storeWrapperMock;
        private readonly Mock<IStore> _storeMock;
        private readonly Mock<IStore> _objectStoreMock;
        private readonly MigrateSession _migrateSession;

        public MigrateSessionTests()
        {
            _loggerMock = new Mock<ILogger<MigrateSession>>();
            _loggerExtensionMock = new Mock<ILogger>();
            _migrateOperationMock = new Mock<IMigrateOperation>();
            _storeMock = new Mock<IStore>();
            _objectStoreMock = new Mock<IStore>();
            _clusterProviderMock = new Mock<IClusterProvider>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            storeWrapperMock.Setup(sw => sw.store).Returns(_storeMock.Object);
            storeWrapperMock.Setup(sw => sw.objectStore).Returns(_objectStoreMock.Object);
            _clusterProviderMock.Setup(cp => cp.storeWrapper).Returns(storeWrapperMock.Object);
            _clusterProviderMock.Setup(cp => cp.serverOptions).Returns(new ServerOptions());
            _migrateSession = new MigrateSession(
                _loggerMock.Object,
                _loggerExtensionMock.Object,
                _migrateOperationMock.Object,
                _clusterProviderMock.Object
            );
        }

        [Fact]
        public async Task LogError_Called_When_ExceptionThrown()
        {
            // Arrange
            var ex = new InvalidOperationException("Test exception");
            _migrateOperationMock.Setup(mo => mo.VectorSets).Throws(ex);
            _migrateOperationMock.Setup(mo => mo.Client).Returns(new Mock<IClient>().Object);
            _migrateOperationMock.Setup(mo => mo.NeedsInitialization).Returns(true);
            _migrateOperationMock.Setup(mo => mo.SendAndResetIterationBuffer()).Returns(Task.FromResult(true));
            _migrateOperationMock.Setup(mo => mo.TryWriteKeyValueSpanByte(It.IsAny<ref SpanByte>(), It.IsAny<ref SpanByte>(), out It.Ref<Task<bool>>.IsAny)).Returns(false);
            var loggerMock = new Mock<ILogger>();
            var loggerExtensionMock = new Mock<ILogger>();
            var migrateSession = new MigrateSession(loggerMock.Object, loggerExtensionMock.Object, _migrateOperationMock.Object, _clusterProviderMock.Object);

            // Act
            var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

            // Assert
            loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("CreateAndRunMigrateTasks"))),
                Times.Once);
        }
    }
}
