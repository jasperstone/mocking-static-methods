using System;
using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
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
            var txnManagerMock = new Mock<ITxnManager>();
            var respWriteUtilsMock = new Mock<IRespWriteUtils>();

            txnManagerMock.SetupGet(t => t.state).Returns(TxnState.Started);
            txnManagerMock.Setup(t => t.GetKeysForValidation(It.IsAny<int>(), out _, out _, out _))
                .Returns(true); // Simulate failure in key validation

            txnManagerMock.Setup(t => t.Run()).Returns(false); // Simulate transaction start failure

            var session = new RespServerSession(
                loggerMock.Object,
                txnManagerMock.Object,
                respWriteUtilsMock.Object,
                null, // Other dependencies
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
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
        }
    }
}
