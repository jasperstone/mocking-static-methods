using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.Databases
{
    public class LoggerExtensionsTests
    {
        private const long AofBeginAddress = 10;
        private const long AofTailAddress = 110;
        private const long AofSizeLimit = 50;

        public class TestAppendOnlyFile
        {
            public long BeginAddress { get; set; } = AofBeginAddress;
            public long TailAddress { get; set; } = AofTailAddress;

            public Task CommitAsync(CancellationToken token = default) => Task.CompletedTask;
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var checkpointManagerMock = new Mock<ICheckpointManager>();

            checkpointManagerMock
                .Setup(cm => cm.TryPauseCheckpointsContinuousAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            checkpointManagerMock
                .Setup(cm => cm.TakeCheckpointAsync(It.IsAny<Guid>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Guid id, ILogger logger, CancellationToken token) =>
                    (new Guid("11111111-1111-1111-1111-111111111111"),
                     new Guid("22222222-2222-2222-2222-222222222222")));

            var loggerExtensions = new LoggerExtensions(checkpointManagerMock.Object)
            {
                AppendOnlyFile = new TestAppendOnlyFile(),
                StoreWrapper = new StoreWrapper(new ServerOptions(), new ClusterProvider())
            };

            // Act
            await loggerExtensions.TaskCheckpointBasedOnAofSizeLimitAsync(AofSizeLimit, CancellationToken.None, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                    It.Is<object[]>(args =>
                        args.Length == 2 &&
                        Convert.ToInt64(args[0]) == AofTailAddress - AofBeginAddress &&
                        Convert.ToInt64(args[1]) == AofSizeLimit)),
                Times.Once);
        }

        // Stubbed components to support the test scenario
        internal class LoggerExtensions : SingleDatabaseManager
        {
            public TestAppendOnlyFile AppendOnlyFile { get; set; }
            public StoreWrapper StoreWrapper { get; set; }

            public LoggerExtensions(ICheckpointManager checkpointManager)
                : base(checkpointManager)
            {
                AppendOnlyFile = new TestAppendOnlyFile();
                StoreWrapper = new StoreWrapper(new ServerOptions(), new ClusterProvider());
            }

            protected override Task<(Guid? storeTailAddress, Guid? objectStoreTailAddress)> TakeCheckpointAsync(
                DatabaseState defaultDatabase,
                ILogger logger,
                CancellationToken token)
            {
                return Task.FromResult<(Guid?, Guid?)>((Guid.Empty, Guid.Empty));
            }
        }

        internal abstract class SingleDatabaseManager
        {
            private readonly ICheckpointManager _checkpointManager;

            protected DatabaseState defaultDatabase = new DatabaseState();

            protected SingleDatabaseManager(ICheckpointManager checkpointManager)
            {
                _checkpointManager = checkpointManager;
            }

            protected virtual Task<(Guid? storeTailAddress, Guid? objectStoreTailAddress)> TakeCheckpointAsync(
                DatabaseState defaultDatabase,
                ILogger logger,
                CancellationToken token)
            {
                return Task.FromResult<(Guid?, Guid?)>((null, null));
            }

            protected Task<bool> TryPauseCheckpointsContinuousAsync(Guid id, CancellationToken token)
            {
                return _checkpointManager.TryPauseCheckpointsContinuousAsync(id, token);
            }

            protected void ResumeCheckpoints(Guid id)
            {
                _checkpointManager.ResumeCheckpoints(id);
            }

            public abstract Task TaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit, CancellationToken token, ILogger logger);

            protected class DatabaseState
            {
                public Guid Id { get; set; } = Guid.NewGuid();
            }
        }

        internal interface ICheckpointManager
        {
            Task<bool> TryPauseCheckpointsContinuousAsync(Guid id, CancellationToken token);
            Task<(Guid? storeTailAddress, Guid? objectStoreTailAddress)> TakeCheckpointAsync(Guid id, ILogger logger, CancellationToken token);
            void ResumeCheckpoints(Guid id);
        }

        internal class StoreWrapper
        {
            public ServerOptions serverOptions { get; }
            public ClusterProvider clusterProvider { get; }

            public StoreWrapper(ServerOptions serverOptions, ClusterProvider clusterProvider)
            {
                this.serverOptions = serverOptions;
                this.clusterProvider = clusterProvider;
            }
        }

        internal class ServerOptions
        {
            public bool EnableCluster { get; set; } = false;
        }

        internal class ClusterProvider
        {
            public bool IsReplica() => false;
        }
    }
}
