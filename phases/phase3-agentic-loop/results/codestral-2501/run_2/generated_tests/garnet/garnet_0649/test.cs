using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;

public class RecoveryTests
{
    [Fact]
    public async Task InternalRecoverAsync_LogsInformation_WhenLogIsNotEmpty()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<Recovery>>();
        var recovery = new Recovery(mockLogger.Object);

        var recoveredICInfo = new IndexCheckpointInfo();
        var recoveredHLCInfo = new HybridLogCheckpointInfo();
        var infoProperty = typeof(HybridLogCheckpointInfo).GetProperty("info", BindingFlags.NonPublic | BindingFlags.Instance);
        var info = infoProperty.GetValue(recoveredHLCInfo);
        info.GetType().GetField("nextVersion").SetValue(info, 1);
        info.GetType().GetField("finalLogicalAddress").SetValue(info, 100);
        info.GetType().GetField("flushedLogicalAddress").SetValue(info, 50);
        info.GetType().GetField("startLogicalAddress").SetValue(info, 0);
        info.GetType().GetField("useSnapshotFile").SetValue(info, 0);

        var cancellationToken = new CancellationToken();

        // Act
        var internalRecoverAsyncMethod = typeof(Recovery).GetMethod("InternalRecoverAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)internalRecoverAsyncMethod.Invoke(recovery, new object[] { recoveredICInfo, recoveredHLCInfo, 0, false, 0, cancellationToken });

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recovery called on non-empty log - resetting to empty state first. Make sure store is quiesced before calling Recover on a running store.")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);
    }
}
