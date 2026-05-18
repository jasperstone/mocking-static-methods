using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;
using System.Threading.Tasks;
using System.Threading;
using Tsavorite.core;

namespace Garnet.Tests
{
    public class TestableSingleDatabaseManager : SingleDatabaseManager
    {
        public TestableSingleDatabaseManager(StoreWrapper.DatabaseCreatorDelegate createDatabaseDelegate, StoreWrapper storeWrapper, bool createDefaultDatabase = true)
            : base(createDatabaseDelegate, storeWrapper, createDefaultDatabase)
        {
        }

        public new TsavoriteLog AppendOnlyFile => base.AppendOnlyFile;

        public new Task TaskCheckpointBasedOnAofSizeLimitAsync(long aofSizeLimit, CancellationToken token = default, ILogger logger = null)
        {
            return base.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, token, logger);
        }
    }

    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_ShouldLogInformation_WhenAofSizeExceedsLimit()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var storeWrapper = new Mock<StoreWrapper>();
            var singleDatabaseManager = new TestableSingleDatabaseManager((id) => new GarnetDatabase(id), storeWrapper.Object);

            long aofSizeLimit = 100;
            long aofSize = 200;

            // Mock the AppendOnlyFile.TailAddress and AppendOnlyFile.BeginAddress
            singleDatabaseManager.AppendOnlyFile.TailAddress = aofSize;
            singleDatabaseManager.AppendOnlyFile.BeginAddress = 0;

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, CancellationToken.None, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Enforcing AOF size limit")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
