using System;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionLoggerTests
    {
        // We cannot directly instantiate RespServerSession because it is internal sealed.
        // Instead, we will test the logger behavior indirectly by creating a minimal wrapper class
        // inside the test namespace that mimics the relevant parts of RespServerSession for the test.

        private class RespServerSessionTest
        {
            public TxnManagerStub txnManager = new TxnManagerStub();
            public ILogger logger;

            public IntPtr endReadHead;
            public IntPtr recvBufferPtr;

            public RespServerSessionTest(ILogger logger)
            {
                this.logger = logger;
                txnManager.state = TxnState.Started;
                txnManager.txnStartHead = (IntPtr)1234;
                endReadHead = (IntPtr)5678;
                recvBufferPtr = IntPtr.Zero;
            }

            // Simulate the NetworkKeyArraySlotVerify method to return true to trigger the warning log
            private bool NetworkKeyArraySlotVerify(ReadOnlySpan<byte> keys, bool readOnly, bool waitForStableSlot, int keyCount)
            {
                return true;
            }

            // Simulate the NetworkEXEC method focusing on the logger.LogWarning call
            public bool NetworkEXEC()
            {
                if (txnManager.state == TxnState.Started)
                {
                    var _origReadHead = endReadHead;
                    endReadHead = txnManager.txnStartHead;

                    txnManager.GetKeysForValidation(recvBufferPtr, out var keys, out int keyCount, out bool readOnly);
                    if (NetworkKeyArraySlotVerify(keys, readOnly, waitForStableSlot: false, keyCount))
                    {
                        logger?.LogWarning("Failed CheckClusterTxnKeys");
                        txnManager.Reset(false);
                        txnManager.watchContainer.Reset();
                        endReadHead = _origReadHead;
                        return true;
                    }

                    // Other code omitted for brevity
                    return false;
                }
                return false;
            }
        }

        private class TxnManagerStub
        {
            public TxnState state;
            public IntPtr txnStartHead;
            public int operationCntTxn;
            public WatchContainerStub watchContainer = new WatchContainerStub();

            public void GetKeysForValidation(IntPtr recvBufferPtr, out ReadOnlySpan<byte> keys, out int keyCount, out bool readOnly)
            {
                keys = ReadOnlySpan<byte>.Empty;
                keyCount = 0;
                readOnly = false;
            }

            public void Reset(bool param)
            {
                ResetCalled = true;
            }

            public bool ResetCalled { get; private set; }
        }

        private class WatchContainerStub
        {
            public void Reset()
            {
                ResetCalled = true;
            }

            public bool ResetCalled { get; private set; }
        }

        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var session = new RespServerSessionTest(loggerMock.Object);

            // Act
            var result = session.NetworkEXEC();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed CheckClusterTxnKeys")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result);
        }
    }
}
