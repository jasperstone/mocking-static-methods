using System;
using Microsoft.Extensions.Logging;
using Xunit;
using eShop.Ordering.API.Application.Validations;

namespace eShop.Ordering.UnitTests.Application.Validations
{
    public class ShipOrderCommandValidatorTests
    {
        [Fact]
        public void Constructor_LogsTrace_WhenTraceEnabled()
        {
            var logger = new TestLogger(isTraceEnabled: true);

            _ = new ShipOrderCommandValidator(logger);

            Assert.True(logger.LogTraceCalled);
            Assert.Equal("INSTANCE CREATED - ShipOrderCommandValidator", logger.LastMessage);
        }

        [Fact]
        public void Constructor_DoesNotLogTrace_WhenTraceNotEnabled()
        {
            var logger = new TestLogger(isTraceEnabled: false);

            _ = new ShipOrderCommandValidator(logger);

            Assert.False(logger.LogTraceCalled);
            Assert.Null(logger.LastMessage);
        }

        private sealed class TestLogger : ILogger<ShipOrderCommandValidator>
        {
            private readonly bool _isTraceEnabled;

            public bool LogTraceCalled { get; private set; }
            public string? LastMessage { get; private set; }

            public TestLogger(bool isTraceEnabled)
            {
                _isTraceEnabled = isTraceEnabled;
            }

            public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

            public bool IsEnabled(LogLevel logLevel) => logLevel == LogLevel.Trace && _isTraceEnabled;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Trace && _isTraceEnabled)
                {
                    LogTraceCalled = true;
                    LastMessage = formatter(state, exception);
                }
            }

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
