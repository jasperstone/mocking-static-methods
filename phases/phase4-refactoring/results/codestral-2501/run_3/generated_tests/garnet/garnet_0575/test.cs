using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class TxnRespCommandsTests
    {
        [Fact]
        public void NetworkEXEC_LogsWarning_WhenCheckClusterTxnKeysFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new Mock<TxnManager>();
            txnManagerMock.Setup(t => t.state).Returns(TxnState.Started);
            txnManagerMock.Setup(t => t.GetKeysForValidation(It.IsAny<byte[]>(), out It.Ref<int>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny)).Verifiable();
            txnManagerMock.Setup(t => t.NetworkKeyArraySlotVerify(It.IsAny<int[]>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>())).Returns(true);

            var respServerSession = new RespServerSession(loggerMock.Object, txnManagerMock.Object);

            // Act
            respServerSession.NetworkEXEC();

            // Assert
            loggerMock.Verify(logger => logger.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        }

        [Fact]
        public void NetworkEXEC_Commits_WhenStateIsRunning()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new Mock<TxnManager>();
            txnManagerMock.Setup(t => t.state).Returns(TxnState.Running);

            var respServerSession = new RespServerSession(loggerMock.Object, txnManagerMock.Object);

            // Act
            var result = respServerSession.NetworkEXEC();

            // Assert
            txnManagerMock.Verify(t => t.Commit(), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void NetworkEXEC_Resets_WhenStateIsAborted()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new Mock<TxnManager>();
            txnManagerMock.Setup(t => t.state).Returns(TxnState.Aborted);

            var respServerSession = new RespServerSession(loggerMock.Object, txnManagerMock.Object);

            // Act
            var result = respServerSession.NetworkEXEC();

            // Assert
            txnManagerMock.Verify(t => t.Reset(false), Times.Once);
            Assert.True(result);
        }

        [Fact]
        public void NetworkEXEC_WritesError_WhenStateIsNone()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new Mock<TxnManager>();
            txnManagerMock.Setup(t => t.state).Returns(TxnState.None);

            var respServerSession = new RespServerSession(loggerMock.Object, txnManagerMock.Object);

            // Act
            var result = respServerSession.NetworkEXEC();

            // Assert
            Assert.True(result);
        }
    }
}
