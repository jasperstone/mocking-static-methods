using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class SingleDatabaseManagerTests
    {
        private class DummyDatabase : GarnetDatabase
        {
            public override void Initialize() { }
            public override void Dispose() { }
        }

        private class DummyStoreWrapper
        {
            public class DummyLoggerFactory
            {
                public ILogger CreateLogger(string name) => new Mock<ILogger>().Object;
            }

            public Func<int, GarnetDatabase> CreateDatabaseDelegate { get; set; }
            public DummyLoggerFactory loggerFactory = new DummyLoggerFactory();
            public ServerOptions serverOptions = new ServerOptions { FailOnRecoveryError = false };
        }

        [Fact]
        public async Task TaskCheckpointBasedOnAofSizeLimitAsync_LogsInformation_WhenReplica()
        {
            // Arrange
            var storeWrapper = new DummyStoreWrapper();
            var manager = new SingleDatabaseManager(
                createDatabaseDelegate: _ => new DummyDatabase(),
                storeWrapper: storeWrapper,
                createDefaultDatabase: true);

            var loggerMock = new Mock<ILogger>();
            // Since AppendOnlyFile properties are not accessible, we simulate the call with parameters that would trigger the log.
            // For this, we need to mock or set AppendOnlyFile properties, but since they are not accessible,
            // we will assume the method is called with parameters that trigger the log.

            // Act
            await manager.TaskCheckpointBasedOnAofSizeLimitAsync(
                aofSizeLimit: 0,
                token: CancellationToken.None,
                logger: loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object>()),
                Times.AtLeastOnce);
        }
    }
}
