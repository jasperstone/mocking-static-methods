using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }
            public EventId LastEventId { get; private set; }
            public Exception LastException { get; private set; }
            public object[] LastArgs { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastEventId = eventId;
                LastException = exception;
                LastLogMessage = formatter(state, exception);
                if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                {
                    LastArgs = new object[kvps.Count];
                    for (int i = 0; i < kvps.Count; i++)
                    {
                        LastArgs[i] = kvps[i].Value;
                    }
                }
            }
        }

        private class DummyCountdownWrapper
        {
            public bool IsCompleted { get; set; }
            public int DecrementCallCount { get; private set; }
            public void Decrement() => DecrementCallCount++;
            public void Wait() { }
        }

        private class TestTsavoriteBase : TsavoriteBase
        {
            public ILogger Logger
            {
                get => logger;
                set => logger = value;
            }

            public DummyCountdownWrapper DummyCountdown { get; } = new DummyCountdownWrapper();

            public void CallAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
            {
                // Set the recoveryCountdown to dummy for test
                recoveryCountdown = new CountdownWrapperForTest(DummyCountdown);
                AsyncPageReadCallback(errorCode, numBytes, overlap);
            }

            private class CountdownWrapperForTest : CountdownWrapper
            {
                private readonly DummyCountdownWrapper dummy;
                public CountdownWrapperForTest(DummyCountdownWrapper dummy)
                {
                    this.dummy = dummy;
                }
                public override bool IsCompleted => dummy.IsCompleted;
                public override void Decrement() => dummy.Decrement();
                public override void Wait() => dummy.Wait();
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var sut = new TestTsavoriteBase();
            var testLogger = new TestLogger();
            sut.Logger = testLogger;
            sut.DummyCountdown.IsCompleted = false;

            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = null;

            // Act
            sut.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            Assert.Equal(LogLevel.Error, testLogger.LastLogLevel);
            Assert.Contains("AsyncPageReadCallback error:", testLogger.LastLogMessage);
            Assert.Contains(errorCode.ToString(), testLogger.LastLogMessage);
            Assert.Equal(1, sut.DummyCountdown.DecrementCallCount);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var sut = new TestTsavoriteBase();
            var testLogger = new TestLogger();
            sut.Logger = testLogger;
            sut.DummyCountdown.IsCompleted = false;

            uint errorCode = 0;
            uint numBytes = 0;
            object overlap = null;

            // Act
            sut.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            Assert.Null(testLogger.LastLogMessage);
            Assert.Equal(1, sut.DummyCountdown.DecrementCallCount);
        }
    }
}
