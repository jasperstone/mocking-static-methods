using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogInformationCalled_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var singleDatabaseManager = new SingleDatabaseManager(null, storeWrapperMock.Object, false);
            singleDatabaseManager.DefaultDatabase = new GarnetDatabase(0, null, true);
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile = new AppendOnlyFile(null, null);
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.TailAddress = 100;
            singleDatabaseManager.DefaultDatabase.AppendOnlyFile.BeginAddress = 0;

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(50, logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
