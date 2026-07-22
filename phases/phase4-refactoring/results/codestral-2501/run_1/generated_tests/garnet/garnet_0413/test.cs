using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Threading;
using System.Threading.Tasks;

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

        mockAppendOnlyFile.Setup(aof => aof.TailAddress).Returns(100);
        mockAppendOnlyFile.Setup(aof => aof.BeginAddress).Returns(0);

        var databaseManager = new SingleDatabaseManager(
            (id) => mockDatabase.Object,
            mockStoreWrapper.Object
        )
        {
            AppendOnlyFile = mockAppendOnlyFile.Object
        };

        // Act
        await databaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(50, CancellationToken.None, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            logger => logger.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
