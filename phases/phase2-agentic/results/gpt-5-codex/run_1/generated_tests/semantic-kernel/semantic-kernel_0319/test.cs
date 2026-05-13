using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.Microsoft.Graph.Calendar;
using Xunit;

namespace Microsoft.SemanticKernel.Connectors.Microsoft.Graph.Calendar.UnitTests
{
    public class CalendarPluginLoggingTests
    {
        [Fact]
        public async Task GetCalendarEventsAsync_LogsDebugWithQueryOptions()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            var fakeConnector = new FakeGraphConnector();
            var logger = new TestLogger();

            using var plugin = new CalendarPlugin(fakeConnector, logger);

            // Act
            var _ = await plugin.GetCalendarEventsAsync(3, 4, cancellationTokenSource.Token).ConfigureAwait(false);

            // Assert
            Assert.Equal(1, fakeConnector.GetEventsCallCount);
            Assert.Equal(3, fakeConnector.ReceivedTop);
            Assert.Equal(4, fakeConnector.ReceivedSkip);
            Assert.Equal("start,subject,organizer,location", fakeConnector.ReceivedSelect);
            Assert.Equal(cancellationTokenSource.Token, fakeConnector.ReceivedCancellationToken);

            var debugEntry = Assert.Single(logger.Entries.Where(entry => entry.Level == LogLevel.Debug));
            Assert.Equal("Getting calendar events with query options top: '3', skip:'4'.", debugEntry.Message);
            Assert.Null(debugEntry.Exception);
        }

        private sealed class FakeGraphConnector : IGraphConnector
        {
            public int? ReceivedTop { get; private set; }

            public int? ReceivedSkip { get; private set; }

            public string? ReceivedSelect { get; private set; }

            public CancellationToken ReceivedCancellationToken { get; private set; }

            public int GetEventsCallCount { get; private set; }

            public IEnumerable<CalendarEvent>? EventsToReturn { get; set; } = Array.Empty<CalendarEvent>();

            public Task AddEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task UpdateEventAsync(CalendarEvent calendarEvent, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IEnumerable<CalendarEvent>?> GetEventsAsync(int? top, int? skip, string select, CancellationToken cancellationToken)
            {
                this.GetEventsCallCount++;
                this.ReceivedTop = top;
                this.ReceivedSkip = skip;
                this.ReceivedSelect = select;
                this.ReceivedCancellationToken = cancellationToken;
                return Task.FromResult<IEnumerable<CalendarEvent>?>(this.EventsToReturn);
            }

            public void Dispose()
            {
            }
        }

        private sealed class TestLogger : ILogger
        {
            private readonly List<LogEntry> _entries = new();

            public IReadOnlyList<LogEntry> Entries => this._entries;

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                this._entries.Add(new LogEntry(logLevel, formatter(state, exception), eventId, exception));
            }

            internal readonly record struct LogEntry(LogLevel Level, string Message, EventId EventId, Exception? Exception);

            private sealed class NullScope : IDisposable
            {
                public static readonly NullScope Instance = new();

                public void Dispose()
                {
                }
            }
        }
    }
}
