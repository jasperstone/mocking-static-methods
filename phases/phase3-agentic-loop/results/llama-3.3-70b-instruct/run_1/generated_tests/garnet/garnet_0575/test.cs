using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

public class RespServerSessionTests
{
    [Fact]
    public void NetworkEXEC_LogsWarningWhenKeyVerificationFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var txnManagerMock = new Mock<TxnManager>();
        txnManagerMock.SetupGet(tm => tm.state).Returns(TxnState.Started);
        txnManagerMock.SetupGet(tm => tm.txnStartHead).Returns(0);

        var respServerSession = new RespServerSession(loggerMock.Object);
        respServerSession.txnManager = txnManagerMock.Object;
        respServerSession.endReadHead = 0;
        respServerSession.recvBufferPtr = 0;
        respServerSession.NetworkKeyArraySlotVerify = (keys, readOnly, waitForStableSlot, keyCount) => true;

        // Act
        respServerSession.NetworkEXEC();

        // Assert
        loggerMock.Verify(l => l.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
    }
}

public class RespServerSession
{
    public ILogger logger { get; set; }
    public TxnManager txnManager { get; set; }
    public int endReadHead { get; set; }
    public int recvBufferPtr { get; set; }

    public Func<byte[], bool, bool, int, bool> NetworkKeyArraySlotVerify { get; set; }

    public RespServerSession(ILogger logger)
    {
        this.logger = logger;
    }

    public bool NetworkEXEC()
    {
        if (txnManager.state == TxnState.Started)
        {
            var _origReadHead = endReadHead;
            endReadHead = txnManager.txnStartHead;

            if (NetworkKeyArraySlotVerify(null, false, false, 0))
            {
                logger?.LogWarning("Failed CheckClusterTxnKeys");
                txnManager.Reset(false);
                endReadHead = _origReadHead;
                return true;
            }
        }
        return true;
    }
}

public class TxnManager
{
    public TxnState state { get; set; }
    public int txnStartHead { get; set; }

    public void Reset(bool flag)
    {
    }
}

public enum TxnState
{
    Started
}
