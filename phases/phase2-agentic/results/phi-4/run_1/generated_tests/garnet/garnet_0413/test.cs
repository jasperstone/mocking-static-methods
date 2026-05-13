using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformationWhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var singleDatabaseManager = new SingleDatabaseManager(
                createDatabaseDelegate: id => new GarnetDatabase(id),
                storeWrapper: new StoreWrapper()
            );

            // Mock the necessary properties and methods
            singleDatabaseManager.AppendOnlyFile = new AppendOnlyFile
            {
                TailAddress = 200,
                BeginAddress = 100
            };

            singleDatabaseManager.StoreWrapper = new StoreWrapper
            {
                serverOptions = new ServerOptions
                {
                    EnableCluster = false
                },
                clusterProvider = new ClusterProvider
                {
                    IsReplica = () => false
                }
            };

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(150, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}")),
                    It.Is<object[]>(args => (long)args[0] == 100 && (long)args[1] == 150),
                    It.IsAny<CancellationToken>()
                ),
                Times.Once
            );
        }
    }
}
