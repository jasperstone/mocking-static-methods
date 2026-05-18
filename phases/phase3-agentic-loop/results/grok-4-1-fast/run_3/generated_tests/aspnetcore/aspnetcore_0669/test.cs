using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        private class TestLogger : ILogger
        {
            public List<string> DebugMessages { get; } = new();

            public bool IsEnabled(LogLevel logLevel) => true;

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (logLevel == LogLevel.Debug)
                {
                    DebugMessages.Add(formatter(state, exception));
                }
            }
        }

        [Fact]
        public void ParseRange_LogsDebugWhenRangesIsNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10";
            var requestHeaders = new RequestHeaders();
            requestHeaders.Range = new RangeHeaderValue("bytes")
            {
                Ranges = null
            };
            var length = 100L;
            var logger = new TestLogger();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is invalid.", logger.DebugMessages.Single());
        }
    }
}
