using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Garnet.server.Tests
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkEXEC_LogsWarning_WhenKeyArraySlotVerificationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new Mock<ITxnManager>();
            var respWriteUtilsMock = new Mock<IRespWriteUtils>();

            txnManagerMock.Setup(tm => tm.state).Returns(TxnState.Started);
            txnManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<int>(), out _, out _, out _))
                .Verifiable();
            txnManagerMock.Setup(tm => tm.Run()).Returns(true);

            // Simulate NetworkKeyArraySlotVerify returning false
            var session = new RespServerSession(
                loggerMock.Object,
                txnManagerMock.Object,
                respWriteUtilsMock.Object);

            // Act
            session.NetworkEXEC();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        }
    }
}
