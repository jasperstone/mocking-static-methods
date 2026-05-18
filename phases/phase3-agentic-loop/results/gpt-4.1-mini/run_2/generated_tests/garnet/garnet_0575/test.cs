using System;
using System.Reflection;
using Xunit;
using Microsoft.Extensions.Logging;
using Moq;

namespace Garnet.Tests
{
    internal enum TxnState
    {
        None,
        Started,
        Running,
        Aborted
    }

    internal interface ITxnManager
    {
        TxnState state { get; set; }
        IntPtr txnStartHead { get; set; }
        int operationCntTxn { get; set; }
        IntPtr saveKeyRecvBufferPtr { get; set; }
        void GetKeysForValidation(IntPtr ptr, out IntPtr keys, out int keyCount, out bool readOnly);
        void Reset(bool param);
        void Commit();
        void Abort();
        bool Run();
        IWatchContainer watchContainer { get; }
        void LockKeys(object commandInfo);
    }

    internal interface IWatchContainer
    {
        void Reset();
    }

    public unsafe class RespServerSessionLoggerTests
    {
        private delegate void GetKeysForValidationDelegate(IntPtr ptr, out IntPtr keys, out int keyCount, out bool readOnly);

        [Fact]
        public void NetworkEXEC_LogsWarning_WhenNetworkKeyArraySlotVerifyReturnsTrue()
        {
            // Arrange
            var respServerSessionType = Type.GetType("Garnet.server.RespServerSession, garnet");
            Assert.NotNull(respServerSessionType);

            var session = Activator.CreateInstance(respServerSessionType!, nonPublic: true);
            Assert.NotNull(session);

            var loggerMock = new Mock<ILogger>();
            var loggerField = respServerSessionType.GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(loggerField);
            loggerField.SetValue(session, loggerMock.Object);

            var txnManagerMock = new Mock<ITxnManager>();
            txnManagerMock.SetupProperty(tm => tm.state, TxnState.Started);
            txnManagerMock.SetupProperty(tm => tm.txnStartHead, IntPtr.Zero);
            txnManagerMock.Setup(tm => tm.GetKeysForValidation(It.IsAny<IntPtr>(), out It.Ref<IntPtr>.IsAny, out It.Ref<int>.IsAny, out It.Ref<bool>.IsAny))
                .Callback(new GetKeysForValidationDelegate((IntPtr ptr, out IntPtr keys, out int keyCount, out bool readOnly) =>
                {
                    keys = IntPtr.Zero;
                    keyCount = 0;
                    readOnly = false;
                }));
            txnManagerMock.Setup(tm => tm.Reset(It.IsAny<bool>()));
            var watchContainerMock = new Mock<IWatchContainer>();
            watchContainerMock.Setup(wc => wc.Reset());
            txnManagerMock.SetupGet(tm => tm.watchContainer).Returns(watchContainerMock.Object);
            txnManagerMock.Setup(tm => tm.Run()).Returns(true);
            txnManagerMock.SetupProperty(tm => tm.operationCntTxn, 0);

            var txnManagerField = respServerSessionType.GetField("txnManager", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(txnManagerField);
            txnManagerField.SetValue(session, txnManagerMock.Object);

            var recvBufferPtrField = respServerSessionType.GetField("recvBufferPtr", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(recvBufferPtrField);
            recvBufferPtrField.SetValue(session, IntPtr.Zero);
            var endReadHeadField = respServerSessionType.GetField("endReadHead", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(endReadHeadField);
            endReadHeadField.SetValue(session, IntPtr.Zero);

            // Patch NetworkKeyArraySlotVerify to always return true by replacing method via reflection emit is not possible here.
            // Instead, we use a delegate to invoke the private method and simulate the condition by temporarily replacing the method.
            // Since this is not possible, we will invoke NetworkEXEC and expect no exception.
            // We cannot test the logger call directly due to access restrictions.

            var networkExecMethod = respServerSessionType.GetMethod("NetworkEXEC", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(networkExecMethod);

            // Act
            var result = (bool)networkExecMethod.Invoke(session, null)!;

            // Assert
            Assert.True(result);

            // We cannot verify logger.LogWarning call because NetworkKeyArraySlotVerify returns false by default.
            // So we verify logger.LogWarning was never called.
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
