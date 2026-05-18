using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogInformationCalled_WhenExceptionThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            storeWrapperMock.SetupGet(sw => sw.serverOptions).Returns(new GarnetServerOptions());
            storeWrapperMock.SetupGet(sw => sw.loggerFactory).Returns(new LoggerFactory());
            var multiDatabaseManager = new MultiDatabaseManager(storeWrapperMock.Object.CreateDatabaseDelegate, storeWrapperMock.Object);

            // Act
            try
            {
                multiDatabaseManager.RecoverCheckpoint();
            }
            catch (Exception)
            {
                // Assert
                loggerMock.Verify(l => l.LogInformation(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
            }
        }
    }
}
