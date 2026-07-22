using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class TxnRespCommandsTests
{
    [Fact]
    public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyFails()
    {
        // Arrange
        var mockTxnManager = new Mock<ITxnManager>();
        mockTxnManager.Setup(tm => tm.State).Returns(TxnState.Started);
        mockTxnManager.Setup(tm => tm.TxnStartHead).Returns(0);
        mockTxnManager.Setup(tm => tm.GetKeysForValidation(It.IsAny<byte*>(), out It.Ref<byte*[]>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny)).Verifiable();
        mockTxnManager.Setup(tm => tm.Reset(false)).Verifiable();
        mockTxnManager.Setup(tm => tm.WatchContainer.Reset()).Verifiable();

        var mockLogger = new Mock<ILogger>();

        var respServerSession = new RespServerSession(mockTxnManager.Object, mockLogger.Object);

        // Act
        var result = respServerSession.NetworkEXEC();

        // Assert
        mockLogger.Verify(logger => logger.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void NetworkEXEC_ReturnsTrue_WhenTxnStateIsRunning()
    {
        // Arrange
        var mockTxnManager = new Mock<ITxnManager>();
        mockTxnManager.Setup(tm => tm.State).Returns(TxnState.Running);
        mockTxnManager.Setup(tm => tm.Commit()).Verifiable();

        var mockLogger = new Mock<ILogger>();

        var respServerSession = new RespServerSession(mockTxnManager.Object, mockLogger.Object);

        // Act
        var result = respServerSession.NetworkEXEC();

        // Assert
        mockTxnManager.Verify(tm => tm.Commit(), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void NetworkEXEC_ReturnsTrue_WhenTxnStateIsAborted()
    {
        // Arrange
        var mockTxnManager = new Mock<ITxnManager>();
        mockTxnManager.Setup(tm => tm.State).Returns(TxnState.Aborted);
        mockTxnManager.Setup(tm => tm.Reset(false)).Verifiable();

        var mockLogger = new Mock<ILogger>();

        var respServerSession = new RespServerSession(mockTxnManager.Object, mockLogger.Object);

        // Act
        var result = respServerSession.NetworkEXEC();

        // Assert
        mockTxnManager.Verify(tm => tm.Reset(false), Times.Once);
        Assert.True(result);
    }

    [Fact]
    public void NetworkEXEC_ReturnsTrue_WhenTxnStateIsNone()
    {
        // Arrange
        var mockTxnManager = new Mock<ITxnManager>();
        mockTxnManager.Setup(tm => tm.State).Returns(TxnState.None);

        var mockLogger = new Mock<ILogger>();

        var respServerSession = new RespServerSession(mockTxnManager.Object, mockLogger.Object);

        // Act
        var result = respServerSession.NetworkEXEC();

        // Assert
        Assert.True(result);
    }
}

public interface ITxnManager
{
    TxnState State { get; }
    int TxnStartHead { get; }
    WatchedKeysContainer WatchContainer { get; }
    void GetKeysForValidation(byte* recvBufferPtr, out byte*[] keys, out int keyCount, out bool readOnly);
    void Reset(bool value);
    void Commit();
}

public class TxnManagerWrapper : ITxnManager
{
    private readonly TransactionManager _txnManager;

    public TxnManagerWrapper(TransactionManager txnManager)
    {
        _txnManager = txnManager;
    }

    public TxnState State => _txnManager.state;
    public int TxnStartHead => _txnManager.txnStartHead;
    public WatchedKeysContainer WatchContainer => _txnManager.watchContainer;

    public void GetKeysForValidation(byte* recvBufferPtr, out byte*[] keys, out int keyCount, out bool readOnly)
    {
        _txnManager.GetKeysForValidation(recvBufferPtr, out keys, out keyCount, out readOnly);
    }

    public void Reset(bool value)
    {
        _txnManager.Reset(value);
    }

    public void Commit()
    {
        _txnManager.Commit();
    }
}
