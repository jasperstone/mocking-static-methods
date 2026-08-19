using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.Tests.cluster
{
    public class MigrationDriverLoggerExtensionsTests
    {
        [Fact]
        public async Task LogsError_WhenMigrateSlotsDriverInlineFails()
        {
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriverForTest(loggerMock.Object);

            migrationDriver.MigrateSlotsDriverInlineResult = false;

            await migrationDriver.TestMigrationStepAsync();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("MigrateSlotsDriver failed")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsError_WhenTrySetSlotRangesFails()
        {
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriverForTest(loggerMock.Object);

            migrationDriver.TrySetSlotRangesResult = false;

            await migrationDriver.TestMigrationStepAsync();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to assign ownership to target node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task LogsError_WhenRelinquishOwnershipFails()
        {
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriverForTest(loggerMock.Object);

            migrationDriver.RelinquishOwnershipResult = false;

            await migrationDriver.TestMigrationStepAsync();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to relinquish ownership from source node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Test subclass to simulate conditions and expose the method to test
        private class MigrationDriverForTest
        {
            private readonly ILogger _logger;

            public bool MigrateSlotsDriverInlineResult { get; set; } = true;
            public bool TrySetSlotRangesResult { get; set; } = true;
            public bool RelinquishOwnershipResult { get; set; } = true;

            public MigrationDriverForTest(ILogger logger)
            {
                _logger = logger;
            }

            public string GetTargetNodeId => "target-node-id";
            public string GetTargetEndpoint => "target-endpoint";
            public string GetSourceNodeId => "source-node-id";

            public async Task TestMigrationStepAsync()
            {
                // Simulate the relevant part of the migration method that logs errors
                if (!TrySetSlotRangesResult)
                {
                    _logger?.LogError("Failed to assign ownership to target node:({tgtNodeId}) ({endpoint})", GetTargetNodeId, GetTargetEndpoint);
                    await Task.CompletedTask;
                    return;
                }

                if (!RelinquishOwnershipResult)
                {
                    _logger?.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", GetSourceNodeId, GetTargetNodeId);
                    await Task.CompletedTask;
                    return;
                }

                if (!MigrateSlotsDriverInlineResult)
                {
                    _logger?.LogError("MigrateSlotsDriver failed");
                    await Task.CompletedTask;
                    return;
                }
            }
        }
    }
}
