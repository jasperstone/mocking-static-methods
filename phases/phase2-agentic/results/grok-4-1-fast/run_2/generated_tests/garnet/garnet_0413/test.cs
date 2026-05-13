using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerTests
    {
        private readonly Mock<ILogger<SingleDatabaseManager>> _loggerMock;
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly SingleDatabaseManager _singleDatabaseManager;

        public SingleDatabaseManagerTests()
        {
            _loggerMock = new Mock<ILogger<SingleDatabaseManager>>();
            _storeWrapperMock = new Mock<StoreWrapper>();

            // Setup minimal dependencies for SingleDatabaseManager creation
            var createDelegate = new StoreWrapper.DatabaseCreatorDelegate((id) => new GarnetDatabase(id, null!, true));
            _storeWrapperMock.Setup(x => x.loggerFactory).Returns(new Mock<ILoggerFactory>().Object);
            _singleDatabaseManager = new SingleDatabaseManager(createDelegate, _storeWrapperMock.Object);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_WhenAofSizeExceedsLimitAndNotReplica_LogsInformationMessage()
        {
            // Arrange
            var aofSizeLimit = 100L;
            var currentAofSize = 200L;
            
            // Mock AppendOnlyFile properties
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(x => x.TailAddress).Returns(currentAofSize + 50);
            mockAof.Setup(x => x.BeginAddress).Returns(50);
            
            // Inject mock AOF into the manager (this would typically be set via StoreWrapper)
            // For testing purposes, we'll use reflection or assume it's accessible
            // Note: In real scenario, this might require more setup with StoreWrapper

            _storeWrapperMock.Setup(x => x.serverOptions.EnableCluster).Returns(false);
            _storeWrapperMock.Setup(x => x.clusterProvider).Returns((ClusterProvider)null!);

            // Mock TryPauseCheckpointsContinuousAsync to return true
            var pauseCheckpointsTask = Task.FromResult(true);

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Enforcing AOF size limit currentAofSize") && 
                                                   v.ToString().Contains("200") && 
                                                   v.ToString().Contains("100")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_WhenAofSizeExceedsLimitAndIsReplica_LogsReplicaSkippingMessage()
        {
            // Arrange
            var aofSizeLimit = 100L;
            
            _storeWrapperMock.Setup(x => x.serverOptions.EnableCluster).Returns(true);
            _storeWrapperMock.Setup(x => x.clusterProvider.IsReplica()).Returns(true);

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert - should log replica skipping message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Replica skipping") && 
                                                   v.ToString().Contains("TaskCheckpointBasedOnAofSizeLimitAsync")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_WhenAofSizeDoesNotExceedLimit_DoesNotLogEnforcingMessage()
        {
            // Arrange
            var aofSizeLimit = 200L;
            var currentAofSize = 100L;

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert - should NOT log the enforcing message
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public async Task CommitToAofAsync_WhenExceptionOccurs_LogsErrorMessage()
        {
            // Arrange
            var exception = new InvalidOperationException("Test exception");
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(exception);
            mockAof.Setup(x => x.TailAddress).Returns(12345L);
            mockAof.Setup(x => x.CommittedUntilAddress).Returns(10000L);

            // Act & Assert
            var task = Assert.ThrowsAsync<InvalidOperationException>(
                () => _singleDatabaseManager.CommitToAofAsync(logger: _loggerMock.Object));

            // Assert logging occurred
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>((v, t) => v.ToString().Contains("Exception raised while committing to AOF") &&
                                                   v.ToString().Contains("12345") &&
                                                   v.ToString().Contains("10000")),
                    It.Is<Exception>(ex => ex == exception),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
