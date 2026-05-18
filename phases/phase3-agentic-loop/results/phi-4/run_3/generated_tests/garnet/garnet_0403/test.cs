using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public void RecoverCheckpoint_LogsInformationOnRecoveryError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var storeWrapperMock = new Mock<StoreWrapper>();
            var multiDatabaseManager = new MultiDatabaseManager(
                dbId => new GarnetDatabase(), 
                storeWrapperMock.Object);

            storeWrapperMock.Setup(s => s.serverOptions.FailOnRecoveryError).Returns(false);

            // Simulate an exception during recovery
            var exception = new Exception("Test exception");
            multiDatabaseManager.Logger = loggerMock.Object;

            // Act
            try
            {
                multiDatabaseManager.RecoverCheckpoint();
            }
            catch (Exception)
            {
                // Expected exception
            }

            // Assert
            loggerMock.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.Is<Exception>(ex => ex == exception),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
