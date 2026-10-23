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
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockDatabaseManager = new Mock<SingleDatabaseManager>(
                (id) => new GarnetDatabase(id),
                new StoreWrapper(new Mock<StoreWrapper.DatabaseCreatorDelegate>().Object, new Mock<StoreWrapper>().Object)
            );

            mockDatabaseManager.Setup(x => x.TaskCheckpointBasedOnAofSizeLimitAsync(It.IsAny<long>(), It.IsAny<CancellationToken>(), It.IsAny<ILogger>()))
                .Returns(Task.CompletedTask);

            // Act
            await mockDatabaseManager.Object.TaskCheckpointBasedOnAofSizeLimitAsync(100, CancellationToken.None, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
