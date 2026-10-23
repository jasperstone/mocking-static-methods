using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var respServerSession = new RespServerSession(loggerMock.Object);
            respServerSession.txnManager.state = TxnState.Started;
            respServerSession.txnManager.txnStartHead = 0;
            respServerSession.endReadHead = 0;
            respServerSession.recvBufferPtr = 0;

            // Act
            respServerSession.NetworkEXEC();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        }
    }
}
