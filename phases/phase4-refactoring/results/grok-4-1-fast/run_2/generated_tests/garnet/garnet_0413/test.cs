using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging.Abstractions;

namespace Garnet.server.Databases
{
    public class SingleDatabaseManagerLoggerTests
    {
        private readonly Mock<ILogger> _mockLogger;
        private readonly Mock<StoreWrapper> _mockStoreWrapper;

        public SingleDatabaseManagerLoggerTests()
        {
            _mockLogger = new Mock<ILogger>();
            _mockLogger.Setup(x => x.IsEnabled(LogLevel.Information)).Returns(true);
            _mockLogger.Setup(x => x.IsEnabled(LogLevel.Error)).Returns(true);

            _mockStoreWrapper = new Mock<StoreWrapper>();
            _mockStoreWrapper.Setup(x => x.serverOptions.EnableCluster).Returns(false);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsAofSizeLimitInfo_WhenSizeExceedsLimit()
        {
            // Arrange
            _mockStoreWrapper.Setup(x => x.serverOptions.EnableCluster).Returns(false);
            var mockClusterProvider = new Mock<object>();
            _mockStoreWrapper.Setup(x => x.clusterProvider).Returns((object)null);

            var testableManager = new TestableSingleDatabaseManager(
                () => new Mock<GarnetDatabase>(0).Object, 
                _mockStoreWrapper.Object);

            // Act
            await testableManager.TaskCheckpointBasedOnAofSizeLimitAsync(500L, logger: _mockLogger.Object);

            // Assert - verify the specific LogInformation call on line 226
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string, object[]>>(state => 
                        state.ToString().Contains("Enforcing AOF size limit currentAofSize") &&
                        state.ToString().Contains("1000") &&
                        state.ToString().Contains("500")),
                    null,
                    It.IsAny<Func<It.IsAnyFormat<string, object[]>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsReplicaSkipInfo_WhenInClusterReplicaMode()
        {
            // Arrange
            _mockStoreWrapper.Setup(x => x.serverOptions.EnableCluster).Returns(true);
            var mockClusterProvider = new Mock<object>();
            mockClusterProvider.Setup(x => x.IsReplica()).Returns(true); // Using dynamic mock
            _mockStoreWrapper.Setup(x => x.clusterProvider).Returns(mockClusterProvider.Object);

            var testableManager = new TestableSingleDatabaseManager(
                () => new Mock<GarnetDatabase>(0).Object, 
                _mockStoreWrapper.Object);

            // Act
            await testableManager.TaskCheckpointBasedOnAofSizeLimitAsync(1000L, logger: _mockLogger.Object);

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat<string, object[]>>(state => 
                        state.ToString().Contains("Replica skipping") &&
                        state.ToString().Contains("TaskCheckpointBasedOnAofSizeLimitAsync")),
                    null,
                    It.IsAny<Func<It.IsAnyFormat<string, object[]>, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task CommitToAofAsync_LogsError_WhenCommitThrowsException()
        {
            // Arrange
            var mockAppendOnlyFile = new Mock<object>();
            var testableManager = new TestableSingleDatabaseManager(
                () => new Mock<GarnetDatabase>(0).Object, 
                _mockStoreWrapper.Object,
                mockAppendOnlyFile: mockAppendOnlyFile.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => testableManager.CommitToAofAsync(logger: _mockLogger.Object));

            // Assert logging occurred
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat<string, object[]>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyFormat<string, object[]>, Exception?, string>>()),
                Times.Once);
        }
    }

    // Test double that makes protected members accessible and injects dependencies
    internal class TestableSingleDatabaseManager : SingleDatabaseManager
    {
        public TestableSingleDatabaseManager(
            StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate,
            StoreWrapper storeWrapper,
            object mockAppendOnlyFile = null)
            : base(createDatabaseDelegate, storeWrapper)
        {
            if (mockAppendOnlyFile != null)
            {
                // Inject mock AppendOnlyFile using reflection
                var appendOnlyFileField = typeof(DatabaseManagerBase).GetField("AppendOnlyFile", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                appendOnlyFileField?.SetValue(this, mockAppendOnlyFile);
            }
        }

        // Expose protected method for testing
        public new Task TaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit, 
            CancellationToken token = default, ILogger logger = null) 
            => base.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, token, logger);

        public new Task CommitToAofAsync(CancellationToken token = default, ILogger logger = null)
            => base.CommitToAofAsync(token, logger);

        // Mock AOF properties for test control
        protected override long GetCurrentAofSize() => 1000L;
    }
}
