using System;
using System.Threading;
using System.Threading.Tasks;
using Garnet.server;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformationWhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            // Mock AppendOnlyFile with TailAddress and BeginAddress
            var appendOnlyFileMock = new Mock<TsavoriteLog>();
            appendOnlyFileMock.SetupGet(a => a.TailAddress).Returns(20L);
            appendOnlyFileMock.SetupGet(a => a.BeginAddress).Returns(0L);

            // Create a GarnetDatabase mock with AppendOnlyFile property
            var garnetDatabaseMock = new Mock<GarnetDatabase>();
            garnetDatabaseMock.SetupGet(db => db.AppendOnlyFile).Returns(appendOnlyFileMock.Object);
            garnetDatabaseMock.SetupProperty(db => db.LastSaveStoreTailAddress);
            garnetDatabaseMock.SetupProperty(db => db.LastSaveObjectStoreTailAddress);
            garnetDatabaseMock.SetupProperty(db => db.LastSaveTime);

            // Create StoreWrapper mock with serverOptions and clusterProvider
            var serverOptions = new GarnetServerOptions { EnableCluster = false };
            var clusterProviderMock = new Mock<IClusterProvider>();
            clusterProviderMock.Setup(cp => cp.IsReplica()).Returns(false);

            var storeWrapperMock = new Mock<StoreWrapper>(
                "version",
                "redisProtocolVersion",
                Array.Empty<IGarnetServer>(),
                null,
                serverOptions,
                null,
                null,
                null,
                null,
                null,
                null);

            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(serverOptions);
            storeWrapperMock.SetupGet(sw => sw.clusterProvider).Returns(clusterProviderMock.Object);

            // Create SingleDatabaseManager instance with delegate returning our GarnetDatabase mock
            StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate = id => garnetDatabaseMock.Object;
            var singleDbManager = new SingleDatabaseManager(createDatabaseDelegate, storeWrapperMock.Object);

            // We need to override TryPauseCheckpointsContinuousAsync to always return true
            // Since SingleDatabaseManager is internal, we cannot inherit, so we use reflection to patch the method
            // Instead, we will mock TryPauseCheckpointsContinuousAsync by using a partial mock of SingleDatabaseManager via Moq
            var singleDbManagerMock = new Mock<SingleDatabaseManager>(createDatabaseDelegate, storeWrapperMock.Object) { CallBase = true };
            singleDbManagerMock.Setup(m => m.TryPauseCheckpointsContinuousAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            singleDbManagerMock.Setup(m => m.TakeCheckpointAsync(It.IsAny<GarnetDatabase>(), It.IsAny<ILogger>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((null, null));
            singleDbManagerMock.Setup(m => m.ResumeCheckpoints(It.IsAny<int>()));

            // Act
            await singleDbManagerMock.Object.TaskCheckpointBasedOnAofSizeLimitAsync(10, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
