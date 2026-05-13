using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class TxnRespCommandsTests
    {
        private Mock<ILogger<RespServerSession>> _loggerMock;
        private RespServerSession _session;

        public TxnRespCommandsTests()
        {
            _loggerMock = new Mock<ILogger<RespServerSession>>();
            _session = new RespServerSession
            {
                logger = _loggerMock.Object,
                // Initialize other necessary fields or dependencies if needed
            };
        }

        [Fact]
        public void NetworkEXEC_ShouldLogWarning_WhenNetworkKeyArraySlotVerifyFails()
        {
            // Arrange
            // Setup the txnManager to be in Started state
            _session.txnManager = new TransactionManager
            {
                state = TxnState.Started,
                // Setup other properties if needed
            };
            // Setup the verify method to return true to simulate failure
            // Since NetworkKeyArraySlotVerify is a method, we need to mock or override it
            // For simplicity, assume we can override or set a delegate if possible
            // Alternatively, we can test the method in isolation if refactored

            // Act
            // Call NetworkEXEC
            var result = _session.NetworkEXEC();

            // Assert
            // Verify that LogWarning was called with the expected message
            _loggerMock.Verify(
                x => x.LogWarning("Failed CheckClusterTxnKeys"),
                Times.Once);
        }
    }
}
