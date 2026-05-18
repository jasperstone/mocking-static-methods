using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class TxnRespCommandsTests
    {
        private readonly Mock<ILogger<RespServerSession>> _loggerMock;
        private readonly RespServerSession _session;

        public TxnRespCommandsTests()
        {
            _loggerMock = new Mock<ILogger<RespServerSession>>();
            _session = new RespServerSession
            {
                logger = _loggerMock.Object,
                // Initialize other dependencies if needed
            };
        }

        [Fact]
        public void NetworkEXEC_Should_LogWarning_When_TxnManagerStateIsStarted()
        {
            // Arrange
            var mockTxnManager = new Mock<ITxnManager>();
            mockTxnManager.SetupGet(m => m.state).Returns(TxnState.Started);
            _session.txnManager = mockTxnManager.Object;

            // Setup other necessary properties
            _session.dcurr = new byte[10];
            _session.dend = new byte[10];
            _session.endReadHead = 0;
            _session.recvBufferPtr = IntPtr.Zero;

            // Act
            var result = _session.NetworkEXEC();

            // Assert
            Assert.True(result);
            _loggerMock.Verify(
                x => x.LogWarning("Failed CheckClusterTxnKeys"),
                Times.Once);
        }
    }
}
