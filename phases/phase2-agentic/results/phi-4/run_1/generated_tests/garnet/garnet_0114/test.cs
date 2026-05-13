using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.cluster;

public class MigrateSessionTests
{
    [Fact]
    public async void LogError_ShouldBeCalled_WhenExceptionOccursInCreateAndRunMigrateTasksAsync()
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
        bool result = await migrateSession.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 1 << migrateSession.clusterProvider.serverOptions.PageSizeBits());

        // Assert
        mockLogger.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                "{CreateAndRunMigrateTasksAsync}: {storeType} {beginAddress} {tailAddress} {pageSize}",
                nameof(MigrateSession.CreateAndRunMigrateTasksAsync),
                StoreType.Main,
                0,
                100,
                1 << migrateSession.clusterProvider.serverOptions.PageSizeBits()),
            Times.Once);
        
        Assert.False(result);
    }
}
