using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.server;

public class TxnRespCommandsTests
{
    [Fact]
    public void NetworkEXEC_LogsWarning_WhenKeyVerificationFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger>();
        var txnManagerMock = new Mock<ITxnManager>();
        var respWriteUtilsMock = new Mock<IRespWriteUtils>();
        var watchContainerMock = new Mock<IWatchContainer>();

        txnManagerMock.Setup(tm => tm.state).Returns(TxnState.Started);
        txnManagerMock.Setup(tm => tm.txnStartHead).Returns(0);
        txnManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<int>(), out _, out _, out _))
                      .Callback<int, out int, out int, out bool>((_, out var keys, out var keyCount, out var readOnly) =>
                      {
                          keys = 0;
                          keyCount = 1;
                          readOnly = false;
                      });
        txnManagerMock.Setup(tm => tm.Run()).Returns(false);

        var session = new RespServerSession(
            loggerMock.Object,
            txnManagerMock.Object,
            respWriteUtilsMock.Object,
            null, // Mock other dependencies as needed
            null,
            watchContainerMock.Object,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null
        );

        // Act
        session.NetworkEXEC();

        // Assert
        loggerMock.Verify(l => l.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        watchContainerMock.Verify(wc => wc.Reset(), Times.Once);
    }
}
