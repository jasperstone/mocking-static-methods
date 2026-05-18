using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class TxnRespCommandsTests
    {
        private Mock<ILogger<RespServerSession>> _loggerMock;
        private Mock<TxnManager> _txnManagerMock;
        private RespServerSession _session;

        public TxnRespCommandsTests()
        {
            _loggerMock = new Mock<ILogger<RespServerSession>>();
            _txnManagerMock = new Mock<TxnManager>();
            _session = new RespServerSession
            {
                logger = _loggerMock.Object,
                txnManager = _txnManagerMock.Object,
                // Initialize other necessary fields if needed
            };
        }

        [Fact]
        public void NetworkEXEC_ShouldLogWarningAndReset_WhenTxnStateIsStarted()
        {
            // Arrange
            _txnManagerMock.Setup(tm => tm.state).Returns(TxnState.Started);
            _txnManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<IntPtr>(), out var _, out var _, out var _))
                .Callback<IntPtr, out var keys, out int keyCount, out bool readOnly>((ptr, out var k, out var c, out var r)) =>
                {
                    k = null;
                    c = 0;
                    r = false;
                };
            _txnManagerMock.Setup(tm => tm.Run()).Returns(true);
            _txnManagerMock.Setup(tm => tm.Reset(It.IsAny<bool>()));
            _txnManagerMock.Setup(tm => tm.watchContainer.Reset());

            // Act
            var result = _session.NetworkEXEC();

            // Assert
            _txnManagerMock.Verify(tm => tm.Reset(false), Times.Once);
            _txnManagerMock.Verify(tm => tm.watchContainer.Reset(), Times.Once);
            _loggerMock.Verify(
                x => x.LogWarning("Failed CheckClusterTxnKeys"),
                Times.Once);
            Assert.True(result);
        }
    }
}
