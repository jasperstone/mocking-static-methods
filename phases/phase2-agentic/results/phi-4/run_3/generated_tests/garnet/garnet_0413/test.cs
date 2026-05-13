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
            var appendOnlyFileMock = new Mock<AppendOnlyFile>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var singleDatabaseManager = new SingleDatabaseManager(
                dbId => new GarnetDatabase(dbId, null, true),
                storeWrapperMock.Object);

            // Simulate AOF size exceeding the limit
            appendOnlyFileMock.SetupGet(a => a.TailAddress).Returns(1001L);
            appendOnlyFileMock.SetupGet(a => a.BeginAddress).Returns(1000L);
            singleDatabaseManager.AppendOnlyFile = appendOnlyFileMock.Object;

            long aofSizeLimit = 1000L;
            CancellationToken token = CancellationToken.None;

            // Act
            await singleDatabaseManager.TaskCheckpointBasedOnAofSizeLimitAsync(aofSizeLimit, token, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(
                    It.Is<string>(s => s.Contains("Enforcing AOF size limit currentAofSize: {aofSize} >  AofSizeLimit: {aofSizeLimit}")),
                    It.Is<long>(aofSize => aofSize == 1001),
                    It.Is<long>(aofSizeLimit => aofSizeLimit == 1000)),
                Times.Once);
        }
    }
}
