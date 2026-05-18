using Xunit;
using Moq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Garnet.cluster;

public class ReplicaSyncSessionTests
{
    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogInformation_Called()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);
        var storeWrapperMock = new Mock<StoreWrapper>(MockBehavior.Strict);
        var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

        // Act
        await replicaSyncSession.AcquireCheckpointEntryAsync();

        // Assert
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task AcquireCheckpointEntryAsync_LogInformation_NotCalled_WhenExceptionThrown()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var clusterProviderMock = new Mock<ClusterProvider>(MockBehavior.Strict);
        var storeWrapperMock = new Mock<StoreWrapper>(MockBehavior.Strict);
        var replicaSyncSession = new ReplicaSyncSession(storeWrapperMock.Object, clusterProviderMock.Object, logger: loggerMock.Object);

        // Act and Assert
        await Assert.ThrowsAsync<Exception>(() => replicaSyncSession.AcquireCheckpointEntryAsync());
        loggerMock.Verify(l => l.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
    }
}
