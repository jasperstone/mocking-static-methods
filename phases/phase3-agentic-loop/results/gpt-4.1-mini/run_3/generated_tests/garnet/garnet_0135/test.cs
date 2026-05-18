using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.cluster;

namespace Garnet.Tests.cluster
{
    public class MigrateSessionTests
    {
        // We will test the logging of the error on the line:
        // logger?.LogError("Failed to relinquish ownership from source node:({srcNode}) to target node: ({tgtNode})", GetSourceNodeId, GetTargetNodeId);
        // This happens in BeginAsyncMigrationTaskAsync when RelinquishOwnership returns false.

        // To do this, we need to create a MigrateSession instance, mock dependencies, and cause the code path to hit that line.

        // Since the class is internal sealed partial, we assume we can instantiate it or use reflection or internal access.
        // For this test, we will create a derived test class to override methods to simulate the conditions.

        private class TestMigrateSession : MigrateSession
        {
            public Mock<ILogger> LoggerMock { get; }
            public bool RelinquishOwnershipReturnValue { get; set; } = true;
            public bool TrySetSlotRangesReturnValue { get; set; } = true;
            public bool MigrateSlotsDriverInlineReturnValue { get; set; } = true;
            public bool ReserveDestinationVectorSetsReturnValue { get; set; } = true;
            public bool TryPrepareLocalForMigrationReturnValue { get; set; } = true;
            public bool BumpAndWaitForEpochTransitionReturnValue { get; set; } = true;

            public TestMigrateSession()
            {
                LoggerMock = new Mock<ILogger>();
                // Setup logger property
                this.logger = LoggerMock.Object;
            }

            // Override methods to simulate behavior
            protected override Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
            {
                return Task.FromResult(TrySetSlotRangesReturnValue);
            }

            protected override bool TryPrepareLocalForMigration()
            {
                return TryPrepareLocalForMigrationReturnValue;
            }

            protected override Task<bool> ReserveDestinationVectorSetsAsync()
            {
                return Task.FromResult(ReserveDestinationVectorSetsReturnValue);
            }

            protected override Task<bool> MigrateSlotsDriverInlineAsync()
            {
                return Task.FromResult(MigrateSlotsDriverInlineReturnValue);
            }

            protected override bool RelinquishOwnership()
            {
                return RelinquishOwnershipReturnValue;
            }

            protected override Task<bool> BumpAndWaitForEpochTransitionAsync()
            {
                return Task.FromResult(BumpAndWaitForEpochTransitionReturnValue);
            }

            // Expose BeginAsyncMigrationTaskAsync for testing
            public Task RunBeginAsyncMigrationTaskAsync() => BeginAsyncMigrationTaskAsync();

            // Provide dummy values for required properties
            public override string GetSourceNodeId => "sourceNode";
            public override string GetTargetNodeId => "targetNode";
            public override string GetTargetEndpoint => "endpoint";

            // Provide dummy slots for logging
            public override int[] GetSlots => new int[] { 1, 2, 3 };
            public override int[] _sslots => new int[] { 1, 2, 3 };
        }

        [Fact]
        public async Task BeginAsyncMigrationTaskAsync_LogsError_WhenRelinquishOwnershipFails()
        {
            // Arrange
            var session = new TestMigrateSession
            {
                TrySetSlotRangesReturnValue = true,
                TryPrepareLocalForMigrationReturnValue = true,
                BumpAndWaitForEpochTransitionReturnValue = true,
                ReserveDestinationVectorSetsReturnValue = true,
                MigrateSlotsDriverInlineReturnValue = true,
                RelinquishOwnershipReturnValue = false // Cause failure here
            };

            // Act
            await session.RunBeginAsyncMigrationTaskAsync();

            // Assert
            session.LoggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed to relinquish ownership from source node")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
