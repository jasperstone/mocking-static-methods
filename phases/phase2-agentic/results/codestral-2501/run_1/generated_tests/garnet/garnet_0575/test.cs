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
        var mockLogger = new Mock<ILogger<TxnRespCommands>>();
        var txnManager = new Mock<TransactionManager>(MockBehavior.Strict, null, null, null, null, null, false, null, 0);
        txnManager.Setup(tm => tm.state).Returns(TxnState.Started);
        txnManager.Setup(tm => tm.txnStartHead).Returns(0);
        txnManager.Setup(tm => tm.GetKeysForValidation(It.IsAny<byte*>(), out It.Ref<byte*[]>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny, out It.Ref<bool>.IsAny)).Returns(true);
        txnManager.Setup(tm => tm.Reset(false)).Verifiable();
        txnManager.Setup(tm => tm.watchContainer.Reset()).Verifiable();

        var txnRespCommands = new TxnRespCommands(txnManager.Object, mockLogger.Object);

        // Act
        txnRespCommands.NetworkEXEC();

        // Assert
        mockLogger.Verify(logger => logger.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        txnManager.Verify(tm => tm.Reset(false), Times.Once);
        txnManager.Verify(tm => tm.watchContainer.Reset(), Times.Once);
    }
}
