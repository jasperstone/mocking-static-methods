using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Cluster.Server.Migration.Tests
{
    public class MigrationDriverLoggerExtensionsTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsCompletedTraceOnSuccess()
        {
            var importedState = MigrationDriver.MigrateState.IMPORT;
            var timeout = TimeSpan.FromSeconds(5);

            var clientMock = new Mock<IMigrateClient>(MockBehavior.Strict);
            clientMock
                .Setup(c => c.SetSlotRange(MigrationDriver.IMPORTING, "node-123", It.IsAny<ReadOnlyMemory<int>>()))
                .ReturnsAsync("OK");

            var migrateOperation = new[]
            {
                new MigrationOperation { Client = clientMock.Object }
            };

            var loggerMock = new Mock<ILogger>(MockBehavior.Strict);
            loggerMock.Setup(l => l.IsEnabled(LogLevel.Trace)).Returns(true);

            loggerMock.Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("Sending CLUSTER SETSLOTRANGE")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            loggerMock.Setup(l => l.Log(
                LogLevel.Trace,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) =>
                    state.ToString()!
                        .Contains("[Completed] SETSLOT")
                        && state.ToString()!.Contains("IMPORT")
                        && state.ToString()!.Contains("node-123")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()));

            var driver = new MigrationDriver(
                migrateOperation,
                timeout,
                CancellationToken.None,
                loggerMock.Object);

            var success = await driver.TrySetSlotRangesAsync("node-123", importedState);

            Assert.True(success);
            loggerMock.VerifyAll();
        }

        private sealed class MigrationOperation
        {
            public IMigrateClient Client { get; set; } = default!;
        }

        private interface IMigrateClient
        {
            ValueTask<string> SetSlotRange(ReadOnlyMemory<byte> state, string? nodeId, ReadOnlyMemory<int> slotRanges);
        }
    }
}
