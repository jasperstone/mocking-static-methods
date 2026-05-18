using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenEnforcingAofSizeLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>(
                "1.0", "RESP3", Array.Empty<IGarnetServer>(),
                Mock.Of<Garnet.server.CustomCommandManager>(),
                new GarnetServerOptions(),
                Mock.Of<Garnet.server.SubscribeBroker>());

            // Mock dependencies minimally
            var aofMock = new Mock<Tsavorite.core.TsavoriteLog>();
            aofMock.Setup(x => x.TailAddress).Returns(1000L);
            aofMock.Setup(x => x.BeginAddress).Returns(0L);
            storeWrapperMock.Setup(x => x.AppendOnlyFile).Returns(aofMock.Object);

            var clusterProviderMock = new Mock<Garnet.server.IClusterProvider>();
            clusterProviderMock.Setup(x => x.IsReplica()).Returns(false);
            storeWrapperMock.Setup(x => x.clusterProvider).Returns(clusterProviderMock.Object);
            storeWrapperMock.Setup(x => x.serverOptions.EnableCluster).Returns(false);

            var dbCreator = new Mock<Garnet.server.StoreWrapper.DatabaseCreatorDelegate>();
            dbCreator.Setup(x => x(It.IsAny<int>())).Returns(new Mock<Garnet.server.GarnetDatabase>(0).Object);

            // Create real SingleDatabaseManager with test logger passed directly
            var manager = new Garnet.server.SingleDatabaseManager(
                dbCreator.Object, 
                storeWrapperMock.Object);

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(
                aofSizeLimit: 500, 
                token: CancellationToken.None, 
                logger: loggerMock.Object);

            // Assert - verify the specific LogInformation extension method call
            loggerMock.Verify(
                x => x.LogInformation(
                    "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                    1000L,
                    500L),
                Times.Once);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsReplicaSkipping_WhenInReplicaMode()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<Garnet.server.StoreWrapper>(
                "1.0", "RESP3", Array.Empty<IGarnetServer>(),
                Mock.Of<Garnet.server.CustomCommandManager>(),
                new Garnet.server.GarnetServerOptions { EnableCluster = true },
                Mock.Of<Garnet.server.SubscribeBroker>());

            var aofMock = new Mock<Tsavorite.core.TsavoriteLog>();
            aofMock.Setup(x => x.TailAddress).Returns(1000L);
            aofMock.Setup(x => x.BeginAddress).Returns(0L);
            storeWrapperMock.Setup(x => x.AppendOnlyFile).Returns(aofMock.Object);

            var clusterProviderMock = new Mock<Garnet.server.IClusterProvider>();
            clusterProviderMock.Setup(x => x.IsReplica()).Returns(true);
            storeWrapperMock.Setup(x => x.clusterProvider).Returns(clusterProviderMock.Object);

            var dbCreator = new Mock<Garnet.server.StoreWrapper.DatabaseCreatorDelegate>();
            dbCreator.Setup(x => x(It.IsAny<int>())).Returns(new Mock<Garnet.server.GarnetDatabase>(0).Object);

            var manager = new Garnet.server.SingleDatabaseManager(
                dbCreator.Object, 
                storeWrapperMock.Object);

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(
                aofSizeLimit: 500, 
                token: CancellationToken.None, 
                logger: loggerMock.Object);

            // Assert - replica skipping log called, enforcement log NOT called
            loggerMock.Verify(
                x => x.LogInformation(
                    "Replica skipping {method}", 
                    It.Is<string>(name => name == "TaskCheckpointBasedOnAofSizeLimitAsync")),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(msg => msg.Contains("Enforcing AOF size limit")),
                    It.IsAny<object[]>(),
                    It.IsAny<Exception>()),
                Times.Never);
        }
    }
}
