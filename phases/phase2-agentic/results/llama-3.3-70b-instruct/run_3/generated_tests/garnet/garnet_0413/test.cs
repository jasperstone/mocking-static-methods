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
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var singleDatabaseManager = new SingleDatabaseManager(null, storeWrapperMock.Object);
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.TailAddress = 100;
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.BeginAddress = 0;
            var aofSizeLimit = 50;

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString().Contains("Enforcing AOF size limit currentAofSize: 100 >  AofSizeLimit: 50")),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_DoesNotLogInformation_WhenAofSizeDoesNotExceedLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var singleDatabaseManager = new SingleDatabaseManager(null, storeWrapperMock.Object);
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.TailAddress = 50;
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.BeginAddress = 0;
            var aofSizeLimit = 100;

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<FormattedLogValues>(v => v.ToString().Contains("Enforcing AOF size limit currentAofSize: 50 >  AofSizeLimit: 100")),
                It.IsAny<Exception>(),
                It.IsAny<Func<FormattedLogValues, Exception, string>>()),
                Times.Never);
        }
    }
}
