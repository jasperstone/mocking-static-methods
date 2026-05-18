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
        var txnManagerMock = new Mock<TransactionManager>(MockBehavior.Strict, null, null, null, null, null, false, null, 0);
        var respServerSession = new RespServerSession(txnManagerMock.Object, loggerMock.Object);

        txnManagerMock.Setup(tm => tm.state).Returns(TxnState.Started);
        txnManagerMock.Setup(tm => tm.txnStartHead).Returns(0);
        txnManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<byte*>(), out It.Ref<byte*[]>().IsAny, out It.Ref<int>().IsAny, out It.Ref<bool>().IsAny)).Verifiable();
        txnManagerMock.Setup(tm => tm.NetworkKeyArraySlotVerify(It.IsAny<byte*[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(true);
        txnManagerMock.Setup(tm => tm.Reset(false)).Verifiable();
        txnManagerMock.Setup(tm => tm.watchContainer.Reset()).Verifiable();

        // Act
        var result = respServerSession.NetworkEXEC();

        // Assert
        loggerMock.Verify(logger => logger.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        Assert.True(result);
        txnManagerMock.Verify();
    }
}
