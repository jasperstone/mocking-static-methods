using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class MigrateSessionTests
{
    [Fact]
    public void LogError_ShouldBeCalled_WhenExceptionIsThrown()
    {
        // Arrange
        var mockLogger = new Mock<ILogger>();
        var migrateSession = new MigrateSession
        {
            logger = mockLogger.Object,
            migrateOperation = new MigrateOperation[1] { new MigrateOperation() },
            _sourceNodeId = 1,
            _replaceOption = true,
            _cts = new System.Threading.CancellationTokenSource(),
            clusterProvider = new ClusterProvider
            {
                serverOptions = new ServerOptions
                {
                    ParallelMigrateTaskCount = 1
                }
            }
        };

        var exception = new Exception("Test exception");

        // Act
        var result = migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 10).GetAwaiter().GetResult();

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "{CreateAndRunMigrateTasksAsync}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(MigrateSession.CreateAndRunMigrateTasksAsync),
                StoreType.Main,
                0,
                100,
                10),
            Times.Once);
        
        Assert.False(result);
    }
}
