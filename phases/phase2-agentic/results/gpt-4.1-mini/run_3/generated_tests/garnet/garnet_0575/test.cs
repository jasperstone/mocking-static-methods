using System;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public unsafe class RespServerSessionTests
    {
        // We need to test the NetworkEXEC method, especially the branch where logger?.LogWarning is called.
        // This happens when txnManager.state == TxnState.Started and NetworkKeyArraySlotVerify returns true.

        // To test this, we will create a derived class to expose NetworkEXEC for testing and allow us to set the txnManager state and mock dependencies.

        private class TestRespServerSession : RespServerSession
        {
            public ILogger LoggerMock { get; set; }
            public TxnManagerMock TxnManagerMock { get; set; }
            public bool NetworkKeyArraySlotVerifyReturn { get; set; }
            public bool RunReturn { get; set; }
            public bool WriteNullArrayCalled { get; private set; }

            public TestRespServerSession()
            {
                // Initialize pointers to dummy values to avoid null pointer issues
                dcurr = (byte*)0x1;
                dend = (byte*)0x10;
                recvBufferPtr = (byte*)0x20;
                readHead = (byte*)0x30;
                endReadHead = (byte*)0x40;
                logger = null;
                txnManager = null;
            }

            public new bool NetworkEXEC()
            {
                // Override to use mocks and test the code path
                // Setup logger
                logger = LoggerMock;

                // Setup txnManager
                txnManager = TxnManagerMock;

                // Setup NetworkKeyArraySlotVerify to return the configured value
                return base.NetworkEXEC();
            }

            // Override NetworkKeyArraySlotVerify to return the configured value
            protected override bool NetworkKeyArraySlotVerify(ReadOnlySpan<byte> keys, bool readOnly, bool waitForStableSlot, int keyCount)
            {
                return NetworkKeyArraySlotVerifyReturn;
            }

            // Override WriteNullArray to track if called
            protected override void WriteNullArray()
            {
                WriteNullArrayCalled = true;
            }

            // Override SendAndReset to do nothing (avoid infinite loops)
            protected override void SendAndReset()
            {
                // no-op
            }
        }

        // Mock TxnManager with minimal implementation for the test
        public class TxnManagerMock : TxnManager
        {
            public TxnState state;
            public int operationCntTxn;
            public byte* txnStartHead;
            public bool RunReturn;
            public bool ResetCalled;
            public bool WatchContainerResetCalled;
            public bool CommitCalled;

            public override TxnState State => state;

            public override void GetKeysForValidation(byte* recvBufferPtr, out ReadOnlySpan<byte> keys, out int keyCount, out bool readOnly)
            {
                keys = new byte[0];
                keyCount = 0;
                readOnly = false;
            }

            public override bool Run()
            {
                return RunReturn;
            }

            public override void Reset(bool abort)
            {
                ResetCalled = true;
            }

            public override void Commit()
            {
                CommitCalled = true;
            }

            public override WatchContainer WatchContainer => new WatchContainerMock(this);

            public class WatchContainerMock : WatchContainer
            {
                private readonly TxnManagerMock parent;
                public WatchContainerMock(TxnManagerMock parent)
                {
                    this.parent = parent;
                }
                public override void Reset()
                {
                    parent.WatchContainerResetCalled = true;
                }
            }
        }

        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new TxnManagerMock
            {
                state = TxnState.Started,
                txnStartHead = (byte*)0x50,
                RunReturn = true
            };

            var session = new TestRespServerSession
            {
                LoggerMock = loggerMock.Object,
                TxnManagerMock = txnManagerMock,
                NetworkKeyArraySlotVerifyReturn = true
            };

            // Act
            var result = session.NetworkEXEC();

            // Assert
            Assert.True(result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Failed CheckClusterTxnKeys"),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(txnManagerMock.ResetCalled);
            Assert.True(txnManagerMock.WatchContainerResetCalled);
            Assert.Equal((byte*)0x40, session.endReadHead); // endReadHead restored to original
        }
    }
}
