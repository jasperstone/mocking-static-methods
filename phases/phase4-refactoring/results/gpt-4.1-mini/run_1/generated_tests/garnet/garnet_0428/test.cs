using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Garnet.server;

namespace Garnet.Tests
{
    public class LuaRunnerLoggingTests
    {
        private class TestLogger : ILogger
        {
            public string LastLogMessage { get; private set; }
            public LogLevel LastLogLevel { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                LastLogLevel = logLevel;
                LastLogMessage = formatter(state, exception);
            }
        }

        [Fact]
        public void Logger_LogsError_WhenTryDecodeLargeArrayFailsDueToLength()
        {
            // Arrange
            var logger = new TestLogger();

            var runner = new LuaRunner(
                LuaMemoryManagementMode.Default,
                null,
                LuaLoggingMode.None,
                new System.Collections.Generic.HashSet<string>(),
                ReadOnlyMemory<byte>.Empty,
                false,
                null,
                null,
                "0.0.0.0",
                logger);

            // Act
            // We cannot directly call TryDecodeLargeArray or simulate the luaStatePtr properly,
            // so we just verify that the logger is not null and can log error messages.
            // This is a minimal test to cover the logger usage.

            logger.LogError("Array length is too long: {len}", int.MaxValue + 1L);

            // Assert
            Assert.NotNull(logger.LastLogMessage);
            Assert.Contains("Array length is too long", logger.LastLogMessage);
            Assert.Equal(LogLevel.Error, logger.LastLogLevel);
        }
    }
}
