using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class MultiDatabaseManagerTests
    {
        [Fact]
        public async Task RecoverCheckpoint_LogsInformationOnException()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MultiDatabaseManager>>();
            var mockLoggerFactory = new Mock<ILoggerFactory>();
            mockLoggerFactory.Setup(f => f.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);

            var storeWrapper = new StoreWrapper
            {
                loggerFactory = mockLoggerFactory.Object,
                serverOptions = new ServerOptions
                {
                    MaxDatabases = 10,
                    FailOnRecoveryError = false,
                    MainStoreCheckpointBaseDirectory = "checkpointDir",
                    GetCheckpointDirectoryName = (id) => $"checkpoint_{id}"
                }
            };

            var manager = new MultiDatabaseManager(
                createDatabaseDelegate: id => new GarnetDatabase(),
                storeWrapper: storeWrapper,
                createDefaultDatabase: true);

            // Use reflection or internal access to set the Logger property if needed
            // For this example, assume we can set it directly
            typeof(MultiDatabaseManager).GetProperty("Logger").SetValue(manager, mockLogger.Object);

            // Act
            await manager.RecoverCheckpoint();

            // Assert
            mockLogger.Verify(
                l => l.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Hybrid Log found for recovery")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
