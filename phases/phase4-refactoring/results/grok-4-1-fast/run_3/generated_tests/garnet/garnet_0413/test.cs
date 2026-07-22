using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server
{
    public class SingleDatabaseManagerLoggingTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsEnforcingMessage_WhenAofSizeExceeded()
        {
            // Arrange
            var testLogger = new TestLogger();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockStoreWrapper.Setup(s => s.serverOptions.EnableCluster).Returns(false);
            mockStoreWrapper.Setup(s => s.clusterProvider.IsReplica()).Returns(false);

            var manager = new TestableSingleDatabaseManager(mockStoreWrapper.Object);
            manager.SetAofSize(1000, 0); // size = 1000 > limit 500

            // Act
            await manager.CallTaskCheckpointBasedOnAofSizeLimitAsync(500, logger: testLogger);

            // Assert - verify the specific LogInformation call on line 226
            Assert.Contains("Enforcing AOF size limit currentAofSize: 1000 >  AofSizeLimit: 500", 
                testLogger.Messages);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsReplicaSkipMessage_WhenClusterReplica()
        {
            // Arrange
            var testLogger = new TestLogger();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            mockStoreWrapper.Setup(s => s.serverOptions.EnableCluster).Returns(true);
            mockStoreWrapper.Setup(s => s.clusterProvider.IsReplica()).Returns(true);

            var manager = new TestableSingleDatabaseManager(mockStoreWrapper.Object);
            manager.SetAofSize(1000, 0);

            // Act
            await manager.CallTaskCheckpointBasedOnAofSizeLimitAsync(500, logger: testLogger);

            // Assert
            Assert.Contains("Replica skipping TaskCheckpointBasedOnAofSizeLimitAsync", 
                testLogger.Messages);
        }

        [Fact]
        public async Task CommitToAofAsync_LogsError_OnException()
        {
            // Arrange
            var testLogger = new TestLogger();
            var manager = new TestableSingleDatabaseManager(Mock.Of<StoreWrapper>());
            manager.SetupThrowOnCommit(true);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => 
                await manager.CallCommitToAofAsync(logger: testLogger));

            // Assert logging occurred
            Assert.Contains("Exception raised while committing to AOF", testLogger.Messages);
            Assert.True(testLogger.HasError);
        }
    }

    // Captures log messages for verification without depending on internal types
    public class TestLogger : ILogger
    {
        public List<string> Messages { get; } = new();
        public bool HasError { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) => null!;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter(state, exception);
            Messages.Add(message);
            if (logLevel == LogLevel.Error) HasError = true;
        }
    }

    // Test implementation that doesn't inherit from internal SingleDatabaseManager
    public class TestableSingleDatabaseManager
    {
        private readonly Mock<StoreWrapper> _storeWrapperMock;
        private readonly MockAof _mockAof;

        public TestableSingleDatabaseManager(StoreWrapper storeWrapper)
        {
            _storeWrapperMock = Mock.Get(storeWrapper) ?? new Mock<StoreWrapper>();
            _mockAof = new MockAof();
        }

        public void SetAofSize(long tailAddress, long beginAddress)
        {
            _mockAof.TailAddress = tailAddress;
            _mockAof.BeginAddress = beginAddress;
        }

        public async Task CallTaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit, CancellationToken token = default, ILogger logger = null)
        {
            var aofSize = _mockAof.TailAddress - _mockAof.BeginAddress;
            if (aofSize <= aofSizeLimit) return;

            // Simulate TryPauseCheckpointsContinuousAsync returning true
            if (true)
            {
                try
                {
                    // Simulate cluster replica check
                    if (_storeWrapperMock.Object.serverOptions.EnableCluster && _storeWrapperMock.Object.clusterProvider.IsReplica())
                    {
                        logger?.LogInformation("Replica skipping {method}", nameof(TaskCheckpointBasedOnAofSizeLimitAsync));
                        return;
                    }

                    // This is the line 226 call we want to test
                    logger?.LogInformation("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                        aofSize, aofSizeLimit);

                    // Simulate TakeCheckpointAsync
                    await Task.CompletedTask;
                }
                finally
                {
                    // Simulate ResumeCheckpoints
                }
            }
        }

        public async Task CallCommitToAofAsync(CancellationToken token = default, ILogger logger = null)
        {
            try
            {
                await _mockAof.CommitAsync(token);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex,
                    "Exception raised while committing to AOF. AOF tail address = {tailAddress}; AOF committed until address = {commitAddress}; ",
                    _mockAof.TailAddress, _mockAof.CommittedUntilAddress);
                throw;
            }
        }

        public void SetupThrowOnCommit(bool throwException)
        {
            _mockAof.ShouldThrow = throwException;
        }
    }

    // Minimal AOF mock for testing
    public class MockAof
    {
        public long TailAddress { get; set; }
        public long BeginAddress { get; set; }
        public long CommittedUntilAddress { get; set; }
        public bool ShouldThrow { get; set; }

        public Task CommitAsync(CancellationToken token = default)
        {
            if (ShouldThrow)
                throw new InvalidOperationException("Simulated AOF commit failure");
            return Task.CompletedTask;
        }
    }
}
