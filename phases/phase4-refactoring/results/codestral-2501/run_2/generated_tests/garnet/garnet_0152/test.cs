using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.client;
using Garnet.cluster;
using System;
using System.Threading.Tasks;
using System.Reflection;

public class AofSyncTaskInfoTests
{
    private readonly Mock<GarnetClientSession> _garnetClientMock;
    private readonly Mock<ILogger> _loggerMock;
    private readonly Mock<ClusterProvider> _clusterProviderMock;
    private readonly AofTaskStore _aofTaskStore;
    private readonly AofSyncTaskInfo _aofSyncTaskInfo;

    public AofSyncTaskInfoTests()
    {
        _garnetClientMock = new Mock<GarnetClientSession>();
        _loggerMock = new Mock<ILogger>();
        _clusterProviderMock = new Mock<ClusterProvider>();
        _aofTaskStore = GetInternalInstance<AofTaskStore>();

        _aofSyncTaskInfo = GetInternalInstance<AofSyncTaskInfo>(
            _clusterProviderMock.Object,
            _aofTaskStore,
            "localNodeId",
            "remoteNodeId",
            _garnetClientMock.Object,
            0,
            _loggerMock.Object);
    }

    private T GetInternalInstance<T>(params object[] args) where T : class
    {
        var constructor = typeof(T).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            CallingConventions.Standard,
            args.Select(a => a.GetType()).ToArray(),
            null);

        if (constructor == null)
        {
            throw new InvalidOperationException($"No suitable constructor found for {typeof(T).FullName}");
        }

        return (T)constructor.Invoke(args);
    }

    [Fact]
    public void Consume_ShouldLogInformation()
    {
        // Arrange
        byte[] payload = new byte[10];
        fixed (byte* payloadPtr = payload)
        {
            // Act
            _aofSyncTaskInfo.Consume(payloadPtr, payload.Length, 0, 1, true);

            // Assert
            _loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }

    [Fact]
    public void Throttle_ShouldThrowException_WhenNotConnected()
    {
        // Arrange
        _garnetClientMock.Setup(client => client.IsConnected).Returns(false);

        // Act & Assert
        Assert.Throws<GarnetException>(() => _aofSyncTaskInfo.Throttle());
    }

    [Fact]
    public async Task ReplicaSyncTaskAsync_ShouldLogInformation()
    {
        // Arrange
        _garnetClientMock.Setup(client => client.IsConnected).Returns(true);
        _garnetClientMock.Setup(client => client.Connect()).Verifiable();
        _clusterProviderMock.Setup(provider => provider.storeWrapper.appendOnlyFile.ScanSingle(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<ILogger>()))
            .Returns(new TsavoriteLogScanSingleIterator());

        // Act
        await _aofSyncTaskInfo.ReplicaSyncTaskAsync();

        // Assert
        _loggerMock.Verify(
            logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }
}
