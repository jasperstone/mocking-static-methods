using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.TemplateEngine;
using Xunit;

namespace Microsoft.SemanticKernel.UnitTests.TemplateEngine.Blocks
{
    public class VarBlockTests
    {
        [Fact]
        public void ConstructorLogsErrorWhenVariableNameIsEmpty()
        {
            var loggerFactory = new TestLoggerFactory();

            _ = new VarBlock("$", loggerFactory);

            Assert.Contains(
                loggerFactory.Logger.LogEntries,
                entry => entry.Level == LogLevel.Error &&
                         string.Equals(entry.Message, "The variable name is empty", StringComparison.Ordinal));
        }

        [Fact]
        public void ConstructorWithValidVariableNameDoesNotLogError()
        {
            var loggerFactory = new TestLoggerFactory();

            var block = new VarBlock("$name", loggerFactory);

            Assert.Equal("name", block.Name);
            Assert.DoesNotContain(
                loggerFactory.Logger.LogEntries,
                entry => entry.Level == LogLevel.Error);
        }

        private sealed class TestLoggerFactory : ILoggerFactory
        {
            public TestLogger Logger { get; } = new();

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public ILogger CreateLogger(string categoryName) => Logger;

            public void Dispose()
            {
            }
        }

        private sealed class TestLogger : ILogger
        {
            public List<LogEntry> LogEntries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                LogEntries.Add(new LogEntry(logLevel, message));
            }

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new NullScope();

                private NullScope()
                {
                }

                public void Dispose()
                {
                }
            }
        }

        private sealed record LogEntry(LogLevel Level, string Message);
    }
}
