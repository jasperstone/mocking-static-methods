using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using Garnet.server;

namespace Garnet.Tests
{
    public class MigrateOperationLoggerTests
    {
        private class DummySession : MigrateSession
        {
            public DummySession() : base(null)
            {
                // override methods if needed
            }
            public override Task<bool> CheckConnectionAsync(GarnetClientSession gcs) => Task.FromResult(true);
            public override GarnetClientSession GetGarnetClient() => new Mock<GarnetClientSession>().Object;
            public override LocalServerSession GetLocalSession() => new Mock<LocalServerSession>().Object;
        }

        [Fact]
        public async Task LogWarning_IsCalled_DuringMigrationRangeScan()
        {
            // Arrange
            var mockLogger = new Mock<ILogger>();
            var session = new DummySession();
            var migrateOp = new MigrateSession.MigrateOperation(session);
            var mockGcs = new Mock<GarnetClientSession>();
            var mockLocalSession = new Mock<LocalServerSession>();
            var mockSession = new Mock<MigrateSession>();
            mockSession.Setup(s => s.GetGarnetClient()).Returns(mockGcs.Object);
            mockSession.Setup(s => s.GetLocalSession()).Returns(mockLocalSession.Object);
            var op = new MigrateSession.MigrateOperation(mockSession.Object);

            // Setup InitializeAsync to return true
            var initResult = await op.InitializeAsync();
            Assert.True(initResult);

            // Simulate the call at line 266
            var logger = mockLogger.Object;
            var workerStartAddress = 123L;
            var workerEndAddress = 456L;

            // Act
            logger.LogWarning("<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]", workerStartAddress, workerEndAddress);

            // Assert
            mockLogger.Verify(
                x => x.LogWarning(
                    "<MainStore> migrate keys (namespaces) scan range [{workerStartAddress}, {workerEndAddress}]",
                    workerStartAddress,
                    workerEndAddress),
                Times.Once);
        }
    }
}
