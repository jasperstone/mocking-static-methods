using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.Threading;
using System;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        private readonly Mock<ILogger<MigrateSession>> _mockLogger;
        private readonly Mock<IClusterProvider> _mockClusterProvider;
        private readonly Mock<IMigrateOperation> _mockMigrateOperation;
        private readonly Mock<IClient> _mockClient;
        private readonly MigrateSession _migrateSession;

        public MigrationDriverTests()
        {
            _mockLogger = new Mock<ILogger<MigrateSession>>();
            _mockClusterProvider = new Mock<IClusterProvider>();
            _mockMigrateOperation = new Mock<IMigrateOperation>();
            _mockClient = new Mock<IClient>();

            _mockMigrateOperation.Setup(op => op.Client).Returns(_mockClient.Object);

            _migrateSession = new MigrateSession(
                _mockClusterProvider.Object,
                _mockLogger.Object,
                new[] { _mockMigrateOperation.Object },
                TimeSpan.FromSeconds(10),
                new CancellationTokenSource().Token
            );
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogError_WhenTryPrepareLocalForMigrationFails()
        {
            // Arrange
            _mockClusterProvider.Setup(cp => cp.BumpAndWaitForEpochTransitionAsync()).ReturnsAsync(true);
            _mockClusterProvider.Setup(cp => cp.storeWrapper.store.PauseRevivification(It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()));
            _mockMigrateOperation.Setup(op => op.Client.SetSlotRange(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<SlotRange[]>())).ReturnsAsync("OK");

            // Act
            await _migrateSession.BeginAsyncMigrationTaskAsync();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to set local slots")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
