using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Threading.Tasks;
using System.Threading;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_ShouldLogInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockDatabase = new Mock<GarnetDatabase>();
            var mockAppendOnlyFile = new Mock<TsavoriteLog>();

            mockStoreWrapper.Setup(sw => sw.DefaultDatabase).Returns(mockDatabase.Object);
            mockStoreWrapper.Setup(sw => sw.appendOnlyFile).Returns(mockAppendOnlyFile.Object);

            var singleDatabaseManager = new SingleDatabaseManager((id) => new GarnetDatabase(id), mockStoreWrapper.Object);

            long aofSizeLimit = 100;
            long aofSize = 200;

            // Mock the AppendOnlyFile.TailAddress and AppendOnlyFile.BeginAddress
            mockAppendOnlyFile.Setup(aof => aof.TailAddress).Returns(aofSize);
            mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(0);

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, CancellationToken.None, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    "Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}",
                    aofSize, aofSizeLimit),
                Times.Once);
        }
    }
}
