using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

public class TxnRespCommandsTests
{
    [Fact]
    public void NetworkEXEC_LogsWarning_WhenCheckClusterTxnKeysFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var txnManagerMock = new Mock<TxnManager>();
        txnManagerMock.Setup(t => t.state).Returns(TxnState.Started);
        txnManagerMock.Setup(t => t.txnStartHead).Returns(0);
        txnManagerMock.Setup(t => t.GetKeysForValidation(It.IsAny<byte[]>(), out It.Ref<int[]>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny)).Verifiable();
        txnManagerMock.Setup(t => t.watchContainer).Returns(new WatchContainer());

        var respServerSession = new RespServerSessionWrapper(loggerMock.Object, txnManagerMock.Object);

        // Act
        respServerSession.NetworkEXEC();

        // Assert
        loggerMock.Verify(logger => logger.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
    }

    [Fact]
    public void NetworkEXEC_ResetsTransaction_WhenCheckClusterTxnKeysFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var txnManagerMock = new Mock<TxnManager>();
        txnManagerMock.Setup(t => t.state).Returns(TxnState.Started);
        txnManagerMock.Setup(t => t.txnStartHead).Returns(0);
        txnManagerMock.Setup(t => t.GetKeysForValidation(It.IsAny<byte[]>(), out It.Ref<int[]>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny)).Verifiable();
        txnManagerMock.Setup(t => t.watchContainer).Returns(new WatchContainer());

        var respServerSession = new RespServerSessionWrapper(loggerMock.Object, txnManagerMock.Object);

        // Act
        respServerSession.NetworkEXEC();

        // Assert
        txnManagerMock.Verify(t => t.Reset(false), Times.Once);
        txnManagerMock.Verify(t => t.watchContainer.Reset(), Times.Once);
    }

    private class RespServerSessionWrapper : RespServerSession
    {
        public RespServerSessionWrapper(ILogger logger, TxnManager txnManager)
            : base(logger, txnManager)
        {
        }

        public new bool NetworkEXEC()
        {
            return base.NetworkEXEC();
        }
    }
}
