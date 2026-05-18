using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsInformation_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new GarnetServerOptions());
            var multiDatabaseManager = new MultiDatabaseManager(() => new GarnetDatabase(), storeWrapperMock.Object);

            // Act
            multiDatabaseManager.RecoverCheckpoint();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
