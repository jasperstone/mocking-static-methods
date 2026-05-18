using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System.Threading.Tasks;
using System.Reflection;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogsInformation()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<ReplicaSyncSession>>();
        var replicaSyncSession = new ReplicaSyncSession(
            storeWrapper: null,
            clusterProvider: null,
            logger: mockLogger.Object);

        // Act
        var method = typeof(ReplicaSyncSession).GetMethod("AcquireCheckpointEntryAsync", BindingFlags.NonPublic | BindingFlags.Instance);
        await (Task)method.Invoke(replicaSyncSession, null);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("AcquireCheckpointEntry iteration")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.AtLeastOnce);
    }
}
