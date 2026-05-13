using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Threading.Tasks;
using System.Threading;

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
            var mockDatabaseCreatorDelegate = new Mock<StoreWrapper.DatabaseCreatorDelegate>();
            var mockGarnetDatabase = new Mock<GarnetDatabase>();
            var mockAppendOnlyFile = new Mock<TsavoriteLog>();

            mockStoreWrapper.Setup(sw => sw.loggerFactory).Returns(Mock.Of<ILoggerFactory>());
            mockDatabaseCreatorDelegate.Setup(d => d(It.IsAny<int>())).Returns(mockGarnetDatabase.Object);
            mockGarnetDatabase.Setup(db => db.AppendOnlyFile).Returns(mockAppendOnlyFile.Object);
            mockAppendOnlyFile.Setup(aof => aof.TailAddress).Returns(100);
            mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(0);

            var singleDatabaseManager = new SingleDatabaseManager(mockDatabaseCreatorDelegate.Object, mockStoreWrapper.Object);

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(50, CancellationToken.None, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit currentAofSize: 100 >  AofSizeLimit: 50")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
