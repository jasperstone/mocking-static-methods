using Xunit;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var singleDatabaseManager = new SingleDatabaseManager(null, storeWrapperMock.Object);
            var aofSizeLimit = 100;
            var aofSize = aofSizeLimit + 1;

            storeWrapperMock.SetupGet(sw => sw.AppendOnlyFile).Returns(new Mock<TsavoriteLog>().Object);
            storeWrapperMock.SetupGet(sw => sw.AppendOnlyFile.TailAddress).Returns(aofSize);
            storeWrapperMock.SetupGet(sw => sw.AppendOnlyFile.BeginAddress).Returns(0);

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(flv => flv.ToString().Contains($"Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}")),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Once);
        }
    }
}
