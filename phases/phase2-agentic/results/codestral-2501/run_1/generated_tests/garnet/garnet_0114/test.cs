using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;
using System;
using System.Threading.Tasks;

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<MigrateSession>>();
        var migrateSession = new MigrateSession(
            clusterSession: null,
            clusterProvider: null,
            _targetAddress: "127.0.0.1",
            _targetPort: 6379,
            _targetNodeId: "targetNode",
            _username: "user",
            _passwd: "pass",
            _sourceNodeId: "sourceNode",
            _copyOption: false,
            _replaceOption: false,
            _timeout: 1000,
            _slots: new System.Collections.Generic.HashSet<int> { 1 },
            sketch: null,
            transferOption: TransferOption.SLOTS
        );

        var exception = new Exception("Test exception");
        migrateSession.migrateOperation[0] = new MigrateOperation(migrateSession);

        // Act
        var result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10);

        // Assert
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Test exception")),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);

        Assert.False(result);
    }
}
