using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerLoggerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsAofSizeLimitInfo_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var manager = new TestableSingleDatabaseManager();
            manager.SetAofSize(1000L, 0L); // aofSize = 1000 > limit 500
            manager.SetTryPauseResult(true);
            manager.SetClusterMode(false, false);

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(500, logger: logger.Object);

            // Assert
            logger.VerifyAll();
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsReplicaSkippingInfo_WhenInClusterReplicaMode()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Replica skipping")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var manager = new TestableSingleDatabaseManager();
            manager.SetAofSize(1000L, 0L);
            manager.SetTryPauseResult(true);
            manager.SetClusterMode(true, true); // enableCluster=true, isReplica=true

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(500, logger: logger.Object);

            // Assert
            logger.VerifyAll();
        }

        [Fact]
        public async Task CommitToAofAsync_LogsError_OnException()
        {
            // Arrange
            var logger = new Mock<ILogger>();
            logger.Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var manager = new TestableSingleDatabaseManager();
            manager.SetAofProperties(1234L, 567L);
            manager.SetCommitThrows(new InvalidOperationException("Test exception"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => manager.CommitToAofAsync(logger: logger.Object));

            // Assert
            logger.VerifyAll();
        }
    }

    public class TestableSingleDatabaseManager
    {
        private long _aofTailAddress = 0;
        private long _aofBeginAddress = 0;
        private long _aofCommittedUntilAddress = 0;
        private bool _tryPauseResult = false;
        private bool _enableCluster = false;
        private bool _isReplica = false;
        private Exception? _commitException;

        public void SetAofSize(long tailAddress, long beginAddress)
        {
            _aofTailAddress = tailAddress;
            _aofBeginAddress = beginAddress;
        }

        public void SetAofProperties(long tailAddress, long committedUntilAddress)
        {
            _aofTailAddress = tailAddress;
            _aofCommittedUntilAddress = committedUntilAddress;
        }

        public void SetTryPauseResult(bool result) => _tryPauseResult = result;

        public void SetClusterMode(bool enableCluster, bool isReplica)
        {
            _enableCluster = enableCluster;
            _isReplica = isReplica;
        }

        public void SetCommitThrows(Exception ex) => _commitException = ex;

        public async Task TaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit,
            CancellationToken token = default, ILogger logger = null)
        {
            var aofSize = _aofTailAddress - _aofBeginAddress;
            if (aofSize <= aofSizeLimit) return;

            if (!await TryPauseCheckpointsContinuousAsync(0, token: token).ConfigureAwait(false))
                return;

            try
            {
                // Checkpoint will be triggered from AOF replay
                if (_enableCluster && _isReplica)
                {
                    logger?.LogInformation("Replica skipping {method}", nameof(TaskCheckpointBasedOnAofSizeLimitAsync));
                    return;
                }

                logger?.LogInformation("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                    aofSize, aofSizeLimit);

                // Simulate the rest of the method
                await TaskCheckpointRestAsync();
            }
            finally
            {
                ResumeCheckpoints(0);
            }
        }

        public async Task CommitToAofAsync(CancellationToken token = default, ILogger logger = null)
        {
            try
            {
                await CommitAsync(token: token).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Exception raised while committing to AOF. AOF tail address = {tailAddress}; AOF committed until address = {commitAddress}; ",
                    _aofTailAddress, _aofCommittedUntilAddress);
                throw;
            }
        }

        private Task<bool> TryPauseCheckpointsContinuousAsync(int dbId, CancellationToken token = default)
            => Task.FromResult(_tryPauseResult);

        private Task CommitAsync(CancellationToken token = default)
        {
            if (_commitException != null)
                throw _commitException;
            return Task.CompletedTask;
        }

        private Task TaskCheckpointRestAsync() => Task.CompletedTask;
        private void ResumeCheckpoints(int dbId) { }
    }
}
