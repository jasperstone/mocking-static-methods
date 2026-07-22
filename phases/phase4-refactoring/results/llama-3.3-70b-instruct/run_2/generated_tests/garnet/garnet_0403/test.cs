using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogInformationCalledOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<IStoreWrapper>();
            var multiDatabaseManager = new MultiDatabaseManager(
                (id) => new GarnetDatabase(),
                storeWrapperMock.Object,
                createDefaultDatabase: false);
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            try
            {
                multiDatabaseManager.RecoverCheckpoint();
            }
            catch (Exception)
            {
                // Expected
            }

            // Assert
            loggerMock.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
