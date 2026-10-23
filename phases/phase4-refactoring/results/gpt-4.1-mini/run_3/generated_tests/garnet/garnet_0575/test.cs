using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public unsafe class RespServerSessionTests
    {
        // Helper subclass to expose NetworkEXEC for testing and override NetworkKeyArraySlotVerify
        private class TestRespServerSession : RespServerSession
        {
            public bool NetworkKeyArraySlotVerifyReturnValue { get; set; }
            public bool NetworkEXECPublic() => NetworkEXEC();

            protected override bool NetworkKeyArraySlotVerify(ReadOnlySpan<byte> keys, bool readOnly, bool waitForStableSlot, int keyCount)
            {
                return NetworkKeyArraySlotVerifyReturnValue;
            }

            // Expose txnManager for setup
            public dynamic TxnManager => typeof(RespServerSession)
                .GetField("txnManager", BindingFlags.NonPublic | BindingFlags.Instance)
                .GetValue(this);

            // Expose logger for setup
            public ILogger Logger
            {
                get => (ILogger)typeof(RespServerSession)
                    .GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                set => typeof(RespServerSession)
                    .GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, value);
            }

            // Expose endReadHead for verification
            public int EndReadHead
            {
                get => (int)typeof(RespServerSession)
                    .GetField("endReadHead", BindingFlags.NonPublic | BindingFlags.Instance)
                    .GetValue(this);
                set => typeof(RespServerSession)
                    .GetField("endReadHead", BindingFlags.NonPublic | BindingFlags.Instance)
                    .SetValue(this, value);
            }
        }

        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyReturnsTrue()
        {
            var session = (TestRespServerSession)Activator.CreateInstance(typeof(RespServerSession), nonPublic: true);
            var loggerMock = new Mock<ILogger>();
            session.Logger = loggerMock.Object;

            // Setup txnManager state to Started
            dynamic txnManager = session.TxnManager;
            txnManager.state = Enum.Parse(txnManager.state.GetType(), "Started");
            txnManager.txnStartHead = 123;
            txnManager.watchContainer = new MockWatchContainer();
            txnManager.Reset = new Action<bool>((bool b) => { txnManager.state = Enum.Parse(txnManager.state.GetType(), "None"); });
            txnManager.GetKeysForValidation = new Action<IntPtr, out ReadOnlySpan<byte>, out int, out bool>((IntPtr ptr, out ReadOnlySpan<byte> keys, out int count, out bool readOnly) =>
            {
                keys = ReadOnlySpan<byte>.Empty;
                count = 0;
                readOnly = false;
            });
            txnManager.Run = new Func<bool>(() => true);

            // Set endReadHead initial value
            session.EndReadHead = 999;

            // Make NetworkKeyArraySlotVerify return true to trigger the warning log
            session.NetworkKeyArraySlotVerifyReturnValue = true;

            // Call NetworkEXEC
            var result = session.NetworkEXECPublic();

            // Assert that LogWarning was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Failed CheckClusterTxnKeys")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            Assert.True(result);
            // Assert that endReadHead was restored to original value
            Assert.Equal(999, session.EndReadHead);
        }

        // Minimal mock for watchContainer.Reset
        private class MockWatchContainer
        {
            public bool ResetCalled { get; private set; }
            public void Reset()
            {
                ResetCalled = true;
            }
        }
    }
}
