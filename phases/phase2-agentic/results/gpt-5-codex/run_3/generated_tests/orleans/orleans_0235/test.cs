using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class SimpleAzureStorageExceptionInjectorTests
    {
        [Fact]
        public void AfterStore_WhenInjectionEnabled_LogsInformationAndThrows()
        {
            var logger = new TestLogger<SimpleAzureStorageExceptionInjector>();
            var injector = new SimpleAzureStorageExceptionInjector(logger)
            {
                InjectAfterStore = true
            };

            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.AfterStore());

            Assert.Equal("Storage exception thrown after store, thrown total 1", exception.Message);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Equal("Storage exception thrown after store, thrown total 1", entry.Message);

            Assert.False(injector.InjectAfterStore);
        }

        [Fact]
        public void BeforeStore_WhenInjectionEnabled_LogsInformationAndThrows()
        {
            var logger = new TestLogger<SimpleAzureStorageExceptionInjector>();
            var injector = new SimpleAzureStorageExceptionInjector(logger)
            {
                InjectBeforeStore = true
            };

            var exception = Assert.Throws<SimpleAzureStorageException>(() => injector.BeforeStore());

            Assert.Equal("Storage exception thrown before store. Thrown total 1", exception.Message);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.Level);
            Assert.Equal("Storage exception thrown before store. Thrown total 1", entry.Message);

            Assert.False(injector.InjectBeforeStore);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable? BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
                Entries.Add(new LogEntry(logLevel, message));
            }
        }

        private readonly record struct LogEntry(LogLevel Level, string Message);

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }
}
