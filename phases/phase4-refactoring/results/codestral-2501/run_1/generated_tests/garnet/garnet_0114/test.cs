using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Garnet.cluster;
using Garnet.server;

public class MigrateSessionWrapper
{
    private readonly MigrateSession _migrateSession;

    public MigrateSessionWrapper()
    {
        _migrateSession = new MigrateSession();
    }

    public async Task<bool> CreateAndRunMigrateTasksAsync(StoreType storeType, long beginAddress, long tailAddress, int pageSize)
    {
        return await _migrateSession.CreateAndRunMigrateTasksAsync(storeType, beginAddress, tailAddress, pageSize);
    }
}

public class MigrateSessionSlotsTests
{
    [Fact]
    public async Task CreateAndRunMigrateTasksAsync_LogsErrorOnException()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MigrateSession>>();
        var migrateSessionWrapper = new MigrateSessionWrapper();
        var exception = new Exception("Test exception");

        // Act
        var result = await migrateSessionWrapper.CreateAndRunMigrateTasksAsync(StoreType.Main, 0, 100, 16);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.IsAny<Exception>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>(),
                It.IsAny<object>()),
            Times.Once);
    }
}
