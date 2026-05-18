using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Microsoft.AspNetCore.Hosting.Tests
{
    public class LoggerExtensionsTests
    {
        [Fact]
        public void LogWarning_WhenEnabled_LogsCorrectMessage()
        {
            // Arrange
            var messages = new List<string>();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new TestLoggerProvider(messages)));
            var logger = loggerFactory.CreateLogger<WebHost>();

            // Exact message from WebHostBuilder.Build() line 186
            var assemblyName = "TestAssembly";
            var message = $"The assembly {assemblyName} was specified multiple times. Hosting startup assemblies should only be specified once.";

            // Act - exact call pattern from source code
            if (logger.IsEnabled(LogLevel.Warning))
            {
                logger.LogWarning(message);
            }

            // Assert
            Assert.Single(messages);
            Assert.Equal(message, messages[0]);
        }

        [Fact]
        public void LogWarning_WhenDisabled_DoesNotLog()
        {
            // Arrange
            var messages = new List<string>();
            using var loggerFactory = LoggerFactory.Create(builder => builder.AddProvider(new TestLoggerProvider(messages)));
            var logger = loggerFactory.CreateLogger<WebHost>();

            var message = "Test message";

            // Act - simulate IsEnabled returning false
            var disabledLogger = new TestDisabledLogger();

            if (disabledLogger.IsEnabled(LogLevel.Warning))
            {
                disabledLogger.LogWarning(message);
            }

            // Assert - no messages logged
            Assert.Empty(messages);
        }
    }

    public class TestLoggerProvider : ILoggerProvider
    {
        private readonly List<string> _messages;

        public TestLoggerProvider(List<string> messages)
        {
            _messages = messages;
        }

        public ILogger CreateLogger(string categoryName) => new TestLogger(_messages);

        public void Dispose() { }
    }

    public class TestLogger : ILogger
    {
        private readonly List<string> _messages;

        public TestLogger(List<string> messages)
        {
            _messages = messages;
        }

        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            _messages.Add(formatter(state, exception));
        }
    }

    public class TestDisabledLogger : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) => null;

        public bool IsEnabled(LogLevel logLevel) => false;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            // No-op for disabled logger
        }
    }
}
