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
            _storeWrapperMock.Setup(sw => sw.loggerFactory).Returns(new Mock<ILoggerFactory>().Object);
            _singleDatabaseManager = new SingleDatabaseManager(createDelegate, _storeWrapperMock.Object);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_EnforcesAofSizeLimit_LogsInformation()
        {
            // Arrange
            var aofSizeLimit = 100L;
            var currentAofSize = 200L; // Larger than limit to trigger logging

            // Mock AppendOnlyFile to return large AOF size
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(aof => aof.TailAddress).Returns(250L);
            mockAof.Setup(aof => aof.BeginAddress).Returns(50L);
            _singleDatabaseManager.GetType().GetField("AppendOnlyFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_singleDatabaseManager, mockAof.Object);

            // Mock TryPauseCheckpointsContinuousAsync to succeed
            var pauseMethod = typeof(SingleDatabaseManager).GetMethod("TryPauseCheckpointsContinuousAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pauseMethod?.CreateDelegate(typeof(Func<int, CancellationToken, Task<bool>>), 
                (int dbId, CancellationToken token) => Task.FromResult(true))();

            // Mock StoreWrapper properties for cluster check
            _storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { EnableCluster = false });
            _storeWrapperMock.Setup(sw => sw.clusterProvider).Returns((ClusterProvider)null);

            // Mock TakeCheckpointAsync to return valid result
            var takeCheckpointMethod = typeof(SingleDatabaseManager).GetMethod("TakeCheckpointAsync", 
                new[] { typeof(GarnetDatabase), typeof(ILogger), typeof(CancellationToken) });
            takeCheckpointMethod?.CreateDelegate(typeof(Func<GarnetDatabase, ILogger, CancellationToken, Task<(long?, long?)>>), 
                (db, logger, token) => Task.FromResult((100L, (long?)null)))();

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                    It.Is<long>(size => size == currentAofSize),
                    It.Is<long>(limit => limit == aofSizeLimit)
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_ClusterReplica_SkipsAndLogsInformation()
        {
            // Arrange
            var aofSizeLimit = 100L;

            // Mock AOF size to exceed limit
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(aof => aof.TailAddress).Returns(250L);
            mockAof.Setup(aof => aof.BeginAddress).Returns(50L);
            _singleDatabaseManager.GetType().GetField("AppendOnlyFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_singleDatabaseManager, mockAof.Object);

            // Mock pause to succeed
            var pauseMethod = typeof(SingleDatabaseManager).GetMethod("TryPauseCheckpointsContinuousAsync", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            pauseMethod?.CreateDelegate(typeof(Func<int, CancellationToken, Task<bool>>), 
                (int dbId, CancellationToken token) => Task.FromResult(true))();

            // Mock cluster replica scenario
            _storeWrapperMock.Setup(sw => sw.serverOptions).Returns(new ServerOptions { EnableCluster = true });
            var clusterProviderMock = new Mock<ClusterProvider>();
            clusterProviderMock.Setup(cp => cp.IsReplica()).Returns(true);
            _storeWrapperMock.Setup(sw => sw.clusterProvider).Returns(clusterProviderMock.Object);

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert - should log replica skipping message, NOT the AOF size enforcement message
            _loggerMock.Verify(
                logger => logger.LogInformation("Replica skipping {method}", It.IsAny<string>()),
                Times.Once
            );

            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(msg => msg.Contains("Enforcing AOF size limit")),
                    It.IsAny<object[]>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_AofSizeBelowLimit_NoLogging()
        {
            // Arrange
            var aofSizeLimit = 300L;
            var currentAofSize = 100L; // Below limit, should return early

            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(aof => aof.TailAddress).Returns(150L);
            mockAof.Setup(aof => aof.BeginAddress).Returns(50L);
            _singleDatabaseManager.GetType().GetField("AppendOnlyFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_singleDatabaseManager, mockAof.Object);

            // Act
            await _singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: _loggerMock.Object);

            // Assert - no LogInformation calls for AOF enforcement
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(msg => msg.Contains("Enforcing AOF size limit")),
                    It.IsAny<object[]>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task CommitToAofAsync_Exception_LogsError()
        {
            // Arrange
            var mockAof = new Mock<AppendOnlyFile>();
            mockAof.Setup(aof => aof.CommitAsync(It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Test exception"));
            
            mockAof.Setup(aof => aof.TailAddress).Returns(1000L);
            mockAof.Setup(aof => aof.CommittedUntilAddress).Returns(900L);

            _singleDatabaseManager.GetType().GetField("AppendOnlyFile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_singleDatabaseManager, mockAof.Object);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _singleDatabaseManager.CommitToAofAsync(logger: _loggerMock.Object));

            Assert.Equal("Test exception", exception.Message);

            // Verify error logging with correct parameters
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Exception raised while committing to AOF. AOF tail address = {tailAddress}; AOF committed until address = {commitAddress}; ",
                    1000L, 900L
                ),
                Times.Once
            );
        }
    }
}
