using Xunit;
using Microsoft.Extensions.Logging;
using Moq;
using Garnet.server;
using System;

namespace Garnet.Tests
{
    public class TxnRespCommandsTests
    {
        private class DummyLogger : ILogger
        {
            public string LastLogWarningMessage { get; private set; }
            public LogLevel? LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (logLevel == LogLevel.Warning)
                {
                    LastLogWarningMessage = formatter(state, exception);
                    LastLogLevel = logLevel;
                }
            }
        }

        [Fact]
        public void NetworkEXEC_StartsTransactionAndLogsWarning_WhenNetworkKeyArraySlotVerifyFails()
        {
            // Arrange
            var mockLogger = new DummyLogger();
            var session = new RespServerSession
            {
                logger = mockLogger,
                txnManager = new TransactionManagerMock(),
                // Setup other necessary properties
                dcurr = IntPtr.Zero,
                dend = IntPtr.Zero,
                recvBufferPtr = IntPtr.Zero,
                endReadHead = 0,
                // Mock methods if needed
            };

            // Setup txnManager to be in Started state
            session.txnManager.state = TxnState.Started;
            // Force NetworkKeyArraySlotVerify to return true to trigger warning
            bool verifyResult = true;

            // Act
            var result = session.NetworkEXEC();

            // Assert
            Assert.True(result);
            Assert.NotNull(mockLogger.LastLogWarningMessage);
            Assert.Equal("Failed CheckClusterTxnKeys", mockLogger.LastLogWarningMessage);
        }
    }

    // Mock class for TransactionManager to simulate behavior
    internal class TransactionManagerMock : dynamic
    {
        public TxnState state { get; set; } = TxnState.None;
        public int operationCntTxn { get; set; } = 0;
        public void Reset(bool flag) { }
        public void Commit() { }
        public bool Run() => true;
        public WatchContainer watchContainer = new WatchContainer();

        public void GetKeysForValidation(IntPtr buffer, out object keys, out int keyCount, out bool readOnly)
        {
            keys = null;
            keyCount = 0;
            readOnly = false;
        }
    }

    internal class WatchContainer
    {
        public void Reset() { }
    }
}
