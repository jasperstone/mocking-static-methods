using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

public class MigrateSessionTests
{
    [Fact]
    public void LogError_ShouldBeCalled_WhenExceptionIsThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var migrateSession = new MigrateSession
        {
            logger = loggerMock.Object,
            migrateOperation = new MigrateOperation[1] { new MigrateOperation() },
            _sourceNodeId = 1,
            _replaceOption = 0,
            _cts = new System.Threading.CancellationTokenSource(),
            clusterProvider = new ClusterProvider
            {
                serverOptions = new ServerOptions
                {
                    ParallelMigrateTaskCount = 1
                },
                storeWrapper = new StoreWrapper
                {
                    store = new Store
                    {
                        Log = new Log
                        {
                            BeginAddress = 0,
                            TailAddress = 100
                        }
                    }
                }
            }
        };

        // Act
        var result = migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 1).Result;

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "{CreateAndRunMigrateTasksAsync}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(MigrateSession.CreateAndRunMigrateTasksAsync),
                StoreType.Main,
                0,
                100,
                1),
            Times.Once);
    }
}
