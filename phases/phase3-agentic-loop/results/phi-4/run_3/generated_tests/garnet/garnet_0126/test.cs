using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.cluster.Tests
{
    public class MigrationDriverTests
    {
        [Fact]
        public async Task TrySetSlotRangesAsync_LogsError_WhenSetSlotRangeFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var migrationDriver = new MigrationDriver(loggerMock.Object);

            // Simulate SetSlotRange failure
            migrationDriver.SetSlotRangeResult = "ERROR";

            // Act
            var result = await migrationDriver.TrySetSlotRangesAsync("nodeId", MigrateState.IMPORT);

            // Assert
            Assert.False(result);
            loggerMock.Verify(
                x => x.LogError(
                    It.Is<string>(s => s.Contains("SetSlotRange error:")),
                    It.Is<object>(o => o.Equals("ERROR"))
                ),
                Times.Once
            );
        }
    }

    // Mocked MigrationDriver class for testing purposes
    internal sealed partial class MigrationDriver
    {
        public ILogger Logger { get; }
        public string SetSlotRangeResult { get; set; }

        public MigrationDriver(ILogger logger)
        {
            Logger = logger;
        }

        public async Task<bool> TrySetSlotRangesAsync(string nodeid, MigrateState state)
        {
            // Simulate the SetSlotRange call
            var result = await SetSlotRange(new byte[0], nodeid, new object());

            if (!result.Equals("OK", StringComparison.Ordinal))
            {
                Logger.LogError("SetSlotRange error: {error}", result);
                return false;
            }

            return true;
        }

        private Task<bool> CheckConnectionAsync(object client) => Task.FromResult(true);

        private Task<object> SetSlotRange(byte[] stateBytes, string nodeid, object slotRanges)
        {
            // Return the result as an object to match the expected return type
            return Task.FromResult((object)SetSlotRangeResult);
        }
    }

    // Mocked MigrateState enum for testing purposes
    internal enum MigrateState
    {
        IMPORT,
        STABLE,
        NODE,
        FAIL
    }
}
