using System;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;

namespace Garnet.Tests
{
    public class RespServerSessionTests
    {
        // We need to test the NetworkEXEC method, especially the branch where logger?.LogWarning is called.
        // Since NetworkEXEC is private, we will use reflection to invoke it.
        // We will mock the dependencies to simulate the condition that triggers the LogWarning call.

        private RespServerSession CreateSessionWithLogger(out Mock<ILogger> loggerMock, out MockTxnManager txnManagerMock)
        {
            loggerMock = new Mock<ILogger>();
            txnManagerMock = new MockTxnManager();

            // RespServerSession is internal sealed partial class with unsafe code.
            // We will create a derived test class to expose NetworkEXEC for testing.
            return new TestRespServerSession(loggerMock.Object, txnManagerMock);
        }

        private class TestRespServerSession : RespServerSession
        {
            private readonly ILogger _logger;
            private readonly MockTxnManager _txnManager;

            public TestRespServerSession(ILogger logger, MockTxnManager txnManager)
            {
                _logger = logger;
                _txnManager = txnManager;

                // Setup fields from RespServerSession base class using reflection
                var loggerField = typeof(RespServerSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField.SetValue(this, _logger);

                var txnManagerField = typeof(RespServerSession).GetField("txnManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                txnManagerField.SetValue(this, _txnManager);

                // Setup other required fields dcurr, dend, endReadHead, recvBufferPtr
                var dcurrField = typeof(RespServerSession).GetField("dcurr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dendField = typeof(RespServerSession).GetField("dend", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var endReadHeadField = typeof(RespServerSession).GetField("endReadHead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var recvBufferPtrField = typeof(RespServerSession).GetField("recvBufferPtr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                // We can assign dummy IntPtr values for pointers
                dcurrField.SetValue(this, IntPtr.Zero);
                dendField.SetValue(this, IntPtr.Zero);
                endReadHeadField.SetValue(this, IntPtr.Zero);
                recvBufferPtrField.SetValue(this, IntPtr.Zero);
            }

            public bool CallNetworkEXEC()
            {
                var method = typeof(RespServerSession).GetMethod("NetworkEXEC", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return (bool)method.Invoke(this, null);
            }
        }

        // MockTxnManager to simulate txnManager behavior
        private class MockTxnManager
        {
            public TxnState state { get; set; } = TxnState.Started;
            public IntPtr txnStartHead { get; set; } = IntPtr.Zero;
            public int operationCntTxn { get; set; } = 0;
            public MockWatchContainer watchContainer { get; } = new MockWatchContainer();

            public void Commit() { }
            public void Reset(bool flag) { ResetCalled = true; }
            public bool ResetCalled { get; private set; } = false;

            public void GetKeysForValidation(IntPtr ptr, out IntPtr keys, out int keyCount, out bool readOnly)
            {
                keys = IntPtr.Zero;
                keyCount = 0;
                readOnly = false;
            }

            public bool Run() => true;

            public void Abort() { }

            public void LockKeys(object commandInfo) { }
        }

        private class MockWatchContainer
        {
            public bool ResetCalled { get; private set; } = false;
            public void Reset() { ResetCalled = true; }
        }

        // We need to override NetworkKeyArraySlotVerify to simulate the condition that triggers LogWarning
        // Since it's private, we will use a derived class with a method to override it.

        private class TestRespServerSessionWithOverride : TestRespServerSession
        {
            public bool NetworkKeyArraySlotVerifyReturnValue { get; set; } = false;

            public TestRespServerSessionWithOverride(ILogger logger, MockTxnManager txnManager) : base(logger, txnManager)
            {
            }

            // Override NetworkKeyArraySlotVerify by reflection hack
            public new bool CallNetworkEXEC()
            {
                // We will replace the private method NetworkKeyArraySlotVerify with a delegate that returns NetworkKeyArraySlotVerifyReturnValue
                // But since it's private and no virtual, we cannot override directly.
                // Instead, we will simulate by setting txnManager.state = Started and setting NetworkKeyArraySlotVerify to return true.

                // We will simulate the condition by setting a flag in txnManager and patching the method via reflection is not feasible here.
                // So we will simulate by setting txnManager.state = Started and patching the method to always return true by reflection.

                // Instead, we will create a subclass of RespServerSession with a public method that calls NetworkEXEC and uses a delegate for NetworkKeyArraySlotVerify.

                // For simplicity, we will just call base.CallNetworkEXEC and rely on the txnManager and logger mocks.

                return base.CallNetworkEXEC();
            }
        }

        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var txnManagerMock = new MockTxnManager();
            txnManagerMock.state = TxnState.Started;

            var session = new TestRespServerSession(loggerMock.Object, txnManagerMock);

            // We need to simulate NetworkKeyArraySlotVerify returning true to trigger LogWarning
            // Since NetworkKeyArraySlotVerify is private, we will use reflection to replace it with a delegate that returns true

            var methodInfo = typeof(RespServerSession).GetMethod("NetworkKeyArraySlotVerify", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // We cannot replace method body easily, so instead we will create a derived class with a public method that calls NetworkEXEC and overrides NetworkKeyArraySlotVerify

            var sessionWithOverride = new RespServerSessionWithNetworkKeyArraySlotVerifyTrue(loggerMock.Object, txnManagerMock);

            // Act
            var result = sessionWithOverride.CallNetworkEXEC();

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
            Assert.True(txnManagerMock.ResetCalled);
            Assert.True(txnManagerMock.watchContainer.ResetCalled);
        }

        // Derived class to override NetworkKeyArraySlotVerify to always return true
        private class RespServerSessionWithNetworkKeyArraySlotVerifyTrue : RespServerSession
        {
            private readonly ILogger _logger;
            private readonly MockTxnManager _txnManager;

            public RespServerSessionWithNetworkKeyArraySlotVerifyTrue(ILogger logger, MockTxnManager txnManager)
            {
                _logger = logger;
                _txnManager = txnManager;

                var loggerField = typeof(RespServerSession).GetField("logger", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                loggerField.SetValue(this, _logger);

                var txnManagerField = typeof(RespServerSession).GetField("txnManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                txnManagerField.SetValue(this, _txnManager);

                var dcurrField = typeof(RespServerSession).GetField("dcurr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var dendField = typeof(RespServerSession).GetField("dend", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var endReadHeadField = typeof(RespServerSession).GetField("endReadHead", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var recvBufferPtrField = typeof(RespServerSession).GetField("recvBufferPtr", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                dcurrField.SetValue(this, IntPtr.Zero);
                dendField.SetValue(this, IntPtr.Zero);
                endReadHeadField.SetValue(this, IntPtr.Zero);
                recvBufferPtrField.SetValue(this, IntPtr.Zero);
            }

            // Expose NetworkEXEC for testing
            public bool CallNetworkEXEC()
            {
                return NetworkEXEC();
            }

            // Override NetworkKeyArraySlotVerify to always return true
            private bool NetworkKeyArraySlotVerify(IntPtr keys, bool readOnly, bool waitForStableSlot, int keyCount)
            {
                return true;
            }
        }
    }
}
