using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Transactions.TestKit;
using Xunit;

namespace Orleans.Transactions.TestKit.Tests
{
    public class RemoteCommitServiceTests
    {
        [Fact]
        public async Task Fail_ShouldLogInformationWithTemplateAndValues()
        {
            var logger = new TestLogger<RemoteCommitService>();
            var service = new RemoteCommitService(logger);
            var transactionId = Guid.NewGuid();
            var data = "failed payload";

            var result = await service.Fail(transactionId, data);

            Assert.False(result);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.LogLevel);
            Assert.Equal(0, entry.EventId.Id);
            Assert.Null(entry.Exception);

            var formattedMessage = entry.Formatter(entry.State, entry.Exception);
            Assert.Equal($"Transaction {transactionId} Failed with data: {data}", formattedMessage);

            var stateValues = Assert.IsAssignableFrom<IEnumerable<KeyValuePair<string, object>>>(entry.State);
            var stateDictionary = stateValues.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            Assert.Equal("Transaction {TransactionId} Failed with data: {Data}", stateDictionary["{OriginalFormat}"]);
            Assert.Equal(transactionId, stateDictionary["TransactionId"]);
            Assert.Equal(data, stateDictionary["Data"]);
        }

        [Fact]
        public async Task Throw_ShouldLogInformationBeforeThrowing()
        {
            var logger = new TestLogger<RemoteCommitService>();
            var service = new RemoteCommitService(logger);
            var transactionId = Guid.NewGuid();
            var data = "throw payload";

            var exception = await Assert.ThrowsAsync<ApplicationException>(() => service.Throw(transactionId, data));
            Assert.Equal("Transaction {transactionId} Threw with data: {data}", exception.Message);

            var entry = Assert.Single(logger.Entries);
            Assert.Equal(LogLevel.Information, entry.LogLevel);

            var formattedMessage = entry.Formatter(entry.State, entry.Exception);
            Assert.Equal($"Transaction {transactionId} Threw with data: {data}", formattedMessage);
        }

        private sealed class TestLogger<T> : ILogger<T>
        {
            public List<LogEntry> Entries { get; } = new();

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Entries.Add(new LogEntry(logLevel, eventId, state!, exception, (s, ex) => formatter((TState)s, ex)));
            }
        }

        private sealed class LogEntry
        {
            public LogEntry(LogLevel logLevel, EventId eventId, object state, Exception exception, Func<object, Exception, string> formatter)
            {
                LogLevel = logLevel;
                EventId = eventId;
                State = state;
                Exception = exception;
                Formatter = formatter;
            }

            public LogLevel LogLevel { get; }
            public EventId EventId { get; }
            public object State { get; }
            public Exception Exception { get; }
            public Func<object, Exception, string> Formatter { get; }
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new NullScope();
            private NullScope() { }
            public void Dispose() { }
        }
    }
}
