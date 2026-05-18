using Xunit;
using Moq;
using Microsoft.Extensions.Logging;

namespace Garnet.server
{
    public class RespServerSessionTests
    {
        [Fact]
        public void NetworkEXEC_LogsWarningWhenKeyVerificationFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var transactionManagerMock = new Mock<TransactionManager>(Mock.Of<StoreWrapper>(), Mock.Of<RespServerSession>(), Mock.Of<BasicGarnetApi>(), Mock.Of<LockableGarnetApi>(), Mock.Of<StorageSession>(), Mock.Of<ScratchBufferAllocator>(), true, loggerMock.Object);
            var respServerSession = new RespServerSession(loggerMock.Object, transactionManagerMock.Object);

            transactionManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<byte*>(), out It.Ref<byte[]>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny))
                .Callback((byte* ptr, out byte[] keys, out int keyCount, out bool readOnly) =>
                {
                    keys = new byte[0];
                    keyCount = 0;
                    readOnly = false;
                });

            // Act
            respServerSession.NetworkEXEC();

            // Assert
            loggerMock.Verify(l => l.LogWarning("Failed CheckClusterTxnKeys"), Times.Once);
        }
    }
}
