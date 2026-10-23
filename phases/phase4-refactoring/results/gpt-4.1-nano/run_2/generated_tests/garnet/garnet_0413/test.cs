using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        [Fact]
        public async Task RecoverCheckpoint_LogsInformation_WhenTsavoriteNoHybridLogExceptionOccurs()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var mockStoreWrapper = new Mock<StoreWrapper>();
            var mockServerOptions = new Mock<StoreWrapper.DummyServerOptions>();
            mockServerOptions.SetupGet(o => o.FailOnRecoveryError).Returns(false);
            mockStoreWrapper.SetupGet(s => s.serverOptions).Returns(mockServerOptions.Object);
            mockStoreWrapper.Setup(s => s.loggerFactory).Returns(new DummyLoggerFactory());

            var manager = new SingleDatabaseManager(
                (id) => new GarnetDatabase(id, null, false),
                mockStoreWrapper.Object,
                createDefaultDatabase: true);

            // Inject the mock logger
            typeof(SingleDatabaseManager).GetProperty("Logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(manager, mockLogger.Object);

            // Act
            await manager.RecoverCheckpoint(replicaRecover: true);

            // Assert
            mockLogger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("No Hybrid Log found for recovery")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.AtLeastOnce);
        }
    }
}
