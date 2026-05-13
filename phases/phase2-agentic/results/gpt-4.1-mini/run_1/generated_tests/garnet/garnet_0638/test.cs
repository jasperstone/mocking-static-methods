using System;
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
            public LogLevel? LastLogLevel { get; private set; }
            public EventId? LastEventId { get; private set; }
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
                    var args = new System.Collections.Generic.List<object>();
                    foreach (var kvp in kvps)
                    {
                        if (kvp.Key == "{OriginalFormat}") continue;
                        args.Add(kvp.Value);
                    }
                    LastArgs = args.ToArray();
                }
                else
                {
                    LastArgs = null;
                }
            }
        }

        private class DummyCountdownWrapper : CountdownWrapper
        {
            public bool IsCompletedOverride { get; set; }
            public int DecrementCallCount { get; private set; }
            public bool WaitCalled { get; private set; }

            public DummyCountdownWrapper(int count, bool isAsync) : base(count, isAsync)
            {
                IsCompletedOverride = false;
                DecrementCallCount = 0;
                WaitCalled = false;
            }

            public override bool IsCompleted => IsCompletedOverride;

            public override void Decrement()
            {
                DecrementCallCount++;
            }

            public override void Wait()
            {
                WaitCalled = true;
            }
        }

        private class TestTsavoriteBase : TsavoriteBase
        {
            public new ILogger logger;
            public new DummyCountdownWrapper recoveryCountdown;

            public TestTsavoriteBase()
            {
                logger = null;
                recoveryCountdown = null;
            }

            public void CallAsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
            {
                // Set logger and recoveryCountdown fields for testing
                base.logger = logger;
                base.recoveryCountdown = recoveryCountdown;

                // Call the private method via reflection or make it public for testing
                AsyncPageReadCallback(errorCode, numBytes, overlap);
            }

            // Expose the private AsyncPageReadCallback for testing
            private void AsyncPageReadCallback(uint errorCode, uint numBytes, object overlap)
            {
                if (errorCode != 0)
                {
                    logger?.LogError($"{nameof(AsyncPageReadCallback)} error: {{errorCode}}", errorCode);
                }
                recoveryCountdown.Decrement();
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeNonZero()
        {
            // Arrange
            var testLogger = new TestLogger();
            var countdown = new DummyCountdownWrapper(1, false);
            countdown.IsCompletedOverride = false;
            var sut = new TestTsavoriteBase
            {
                logger = testLogger,
                recoveryCountdown = countdown
            };

            uint errorCode = 123;
            uint numBytes = 0;
            object overlap = null;

            // Act
            sut.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            Assert.NotNull(testLogger.LastLogMessage);
            Assert.Contains("AsyncPageReadCallback error:", testLogger.LastLogMessage);
            Assert.Contains(errorCode.ToString(), testLogger.LastLogMessage);
            Assert.Equal(LogLevel.Error, testLogger.LastLogLevel);
            Assert.Equal(1, countdown.DecrementCallCount);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeZero()
        {
            // Arrange
            var testLogger = new TestLogger();
            var countdown = new DummyCountdownWrapper(1, false);
            countdown.IsCompletedOverride = false;
            var sut = new TestTsavoriteBase
            {
                logger = testLogger,
                recoveryCountdown = countdown
            };

            uint errorCode = 0;
            uint numBytes = 0;
            object overlap = null;

            // Act
            sut.CallAsyncPageReadCallback(errorCode, numBytes, overlap);

            // Assert
            Assert.Null(testLogger.LastLogMessage);
            Assert.Equal(1, countdown.DecrementCallCount);
        }
    }
}
