using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests
{
    public class MigrationDriverTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public Exception LastException { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogMessage = formatter(state, exception);
                LastLogLevel = logLevel;
                LastException = exception;
            }
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_Should_LogError_When_TrySetSlotRangesAsync_Fails()
        {
            // Arrange
            var mockClusterProvider = new Mock<ClusterProvider>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockStore = new Mock<Store>();
            var mockClient = new Mock<IRedisClient>();
            var mockVectorManager = new Mock<VectorManager>();
            var mockClusterManager = new Mock<ClusterManager>();

            var logger = new DummyLogger();

            var session = new MigrateSession
            {
                logger = logger,
                clusterProvider = mockClusterProvider.Object,
                Status = MigrateState.INIT,
                _sslots = new int[] { 1, 2, 3 },
                _timeout = TimeSpan.FromSeconds(10),
                _cts = new CancellationTokenSource(),
                GetSourceNodeId = "node1",
                GetSlots = new int[] { 1, 2, 3 },
                _namespaces = null,
                clusterProvider = new ClusterProvider
                {
                    storeWrapper = new StoreWrapper
                    {
                        store = new Store
                        {
                            PauseRevivification = (timeout, token) => { }
                        },
                        DefaultDatabase = new Database
                        {
                            VectorManager = mockVectorManager.Object
                        }
                    },
                    clusterManager = mockClusterManager.Object
                }
            };

            // Setup TrySetSlotRangesAsync to return false to trigger error logging
            mockClusterProvider.Setup(cp => cp.TrySetSlotRangesAsync(It.IsAny<string>(), It.IsAny<MigrateState>()))
                .ReturnsAsync(false);

            // Act
            await session.BeginAsyncMigrationTaskAsync();

            // Assert
            Assert.Equal(MigrateState.FAIL, session.Status);
            Assert.Contains("Failed to set remote slots", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }
    }
}
