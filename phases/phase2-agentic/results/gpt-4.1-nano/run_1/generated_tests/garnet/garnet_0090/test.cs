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
    public class MigrateOperationTests
    {
        private class DummySession : MigrateSession
        {
            public DummySession() : base(
                null, null, "127.0.0.1", 6379, "sourceNode", "user", "pass", "targetNode",
                false, false, 1000, new HashSet<int>(), null, TransferOption.SLOTS)
            {
            }

            public override Task<bool> CheckConnectionAsync(GarnetClientSession client)
            {
                return Task.FromResult(true);
            }

            public override Task<bool> WriteOrSendMainStoreKeyValuePairAsync(GarnetClientSession gcs, LocalServerSession localServer, ref SpanByte key, ref RawStringInput input, ref SpanByteAndMemory o, out GarnetStatus status)
            {
                status = GarnetStatus.OK;
                return Task.FromResult(true);
            }

            public override Task<bool> HandleMigrateTaskResponseAsync(Task<bool> sendTask)
            {
                return sendTask;
            }
        }

        [Fact]
        public async Task LogWarning_IsCalled_OnTransmitSlotsFailure()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new DummySession();
            var migrateOp = new MigrateSession.MigrateOperation(session);
            migrateOp.sketch.SetStatus(SketchStatus.TRANSMITTING);
            migrateOp.sketch.argSliceVector.Add(new Tuple<SpanByte, bool>(new SpanByte(1), false));
            migrateOp.session = session;

            // Mock the TransmitSlotsAsync to return false to trigger LogWarning
            var mock = new Mock<MigrateSession.MigrateOperation>(session) { CallBase = true };
            mock.Setup(m => m.TransmitSlotsAsync(It.IsAny<StoreType>())).ReturnsAsync(false);
            var logger = new Mock<ILogger>();
            mock.Object.logger = logger.Object;

            // Act
            var result = await mock.Object.TransmitSlotsAsync(StoreType.Main);

            // Assert
            Assert.False(result);
            logger.VerifyLog(LogLevel.Warning, "<MainStore> migrate keys (namespaces) scan range");
        }
    }

    public static class LoggerExtensions
    {
        public static void VerifyLog(this Mock<ILogger> loggerMock, LogLevel level, string messagePart)
        {
            loggerMock.Verify(
                x => x.Log(
                    level,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains(messagePart)),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
