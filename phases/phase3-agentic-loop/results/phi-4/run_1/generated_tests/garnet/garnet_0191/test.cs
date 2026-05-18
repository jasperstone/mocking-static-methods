using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server; // Ensure this is correct for StoreWrapper

public interface ICheckpointEntry
{
    // Define necessary properties and methods
}

public interface IReplicaSyncSession
{
    void SimulateLogError(long syncFromAofAddress, long beginAddress);
}

public class MockCheckpointEntry : ICheckpointEntry
{
    // Implement necessary properties and methods
}

public class MockReplicaSyncSession : IReplicaSyncSession
{
    private readonly ILogger _logger;

    public MockReplicaSyncSession(ILogger logger)
    {
        _logger = logger;
    }

    public void SimulateLogError(long syncFromAofAddress, long beginAddress)
    {
        if (syncFromAofAddress < beginAddress)
        {
            _logger.LogError("syncFromAofAddress: {syncFromAofAddress} < beginAofAddress: {beginAddress}", syncFromAofAddress, beginAddress);
        }
    }
}

public class ReplicaSyncSessionTests
{
    [Fact]
    public void LogError_ShouldBeCalled_WhenSyncFromAofAddressIsLessThanBeginAddress()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var storeWrapperMock = new Mock<StoreWrapper>();

        storeWrapperMock.Setup(s => s.appendOnlyFile.BeginAddress).Returns(100L);

        var replicaSyncSession = new MockReplicaSyncSession(loggerMock.Object);

        long syncFromAofAddress = 50L;

        // Act
        replicaSyncSession.SimulateLogError(syncFromAofAddress, storeWrapperMock.Object.appendOnlyFile.BeginAddress);

        // Assert
        loggerMock.Verify(
            x => x.LogError(
                It.Is<string>(s => s.Contains("syncFromAofAddress: 50 < beginAofAddress: 100")),
                It.Is<long>(a => a == 50),
                It.Is<long>(b => b == 100)),
            Times.Once);
    }
}
