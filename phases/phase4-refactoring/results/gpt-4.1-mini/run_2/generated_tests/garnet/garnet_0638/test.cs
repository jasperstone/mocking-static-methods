using System;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tsavorite.core;
using Xunit;

namespace Tsavorite.Tests
{
    public class TsavoriteBaseTests
    {
        private class LoggerMock : ILogger
        {
            public bool LogErrorCalled { get; private set; }
            public string LoggedMessage { get; private set; }
            public object[] LoggedArgs { get; private set; }

            public IDisposable BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Error)
                {
                    LogErrorCalled = true;
                    LoggedMessage = formatter(state, exception);
                    if (state is IReadOnlyList<KeyValuePair<string, object>> kvps)
                    {
                        foreach (var kvp in kvps)
                        {
                            if (kvp.Key == "errorCode")
                            {
                                LoggedArgs = new object[] { kvp.Value! };
                                break;
                            }
                        }
                    }
                }
            }
        }

        [Fact]
        public void AsyncPageReadCallback_LogsError_WhenErrorCodeIsNonZero()
        {
            // Arrange
            var tsavorite = (TsavoriteBase)Activator.CreateInstance(typeof(TsavoriteBase), nonPublic: true)!;
            var loggerMock = new LoggerMock();

            // Use reflection to set private readonly field 'logger'
            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavorite, loggerMock);

            // Use reflection to invoke private method AsyncPageReadCallback
            MethodInfo method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(tsavorite, new object[] { 123u, 0u, null! });

            // Assert
            Assert.True(loggerMock.LogErrorCalled);
            Assert.Contains("AsyncPageReadCallback error", loggerMock.LoggedMessage);
            Assert.NotNull(loggerMock.LoggedArgs);
            Assert.Single(loggerMock.LoggedArgs);
            Assert.Equal(123u, loggerMock.LoggedArgs[0]);
        }

        [Fact]
        public void AsyncPageReadCallback_DoesNotLogError_WhenErrorCodeIsZero()
        {
            // Arrange
            var tsavorite = (TsavoriteBase)Activator.CreateInstance(typeof(TsavoriteBase), nonPublic: true)!;
            var loggerMock = new LoggerMock();

            var loggerField = typeof(TsavoriteBase).GetField("logger", BindingFlags.NonPublic | BindingFlags.Instance);
            loggerField.SetValue(tsavorite, loggerMock);

            MethodInfo method = typeof(TsavoriteBase).GetMethod("AsyncPageReadCallback", BindingFlags.NonPublic | BindingFlags.Instance)!;

            // Act
            method.Invoke(tsavorite, new object[] { 0u, 0u, null! });

            // Assert
            Assert.False(loggerMock.LogErrorCalled);
        }
    }
}
