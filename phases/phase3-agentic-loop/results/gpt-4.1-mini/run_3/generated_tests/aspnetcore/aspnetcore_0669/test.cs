using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Xunit;
using Microsoft.Extensions.Logging;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        private class TestLogger : ILogger
        {
            public LogLevel? LastLogLevel;
            public string? LastMessage;

            public IDisposable BeginScope<TState>(TState state) => null!;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                LastLogLevel = logLevel;
                LastMessage = formatter(state, exception);
            }
        }

        [Fact]
        public void ParseRange_EmptyRawRangeHeader_LogsTraceAndReturnsFalseNull()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = StringValues.Empty;

            var requestHeaders = new RequestHeaders();
            var logger = new TestLogger();

            var result = RangeHelper.ParseRange(context, requestHeaders, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Equal(LogLevel.Trace, logger.LastLogLevel);
            Assert.Equal("Range header's value is empty.", logger.LastMessage);
        }

        [Fact]
        public void ParseRange_MultipleRangesInRawRangeHeader_LogsDebugAndReturnsFalseNull()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = new StringValues(new[] { "bytes=0-1", "bytes=2-3" });

            var requestHeaders = new RequestHeaders();
            var logger = new TestLogger();

            var result = RangeHelper.ParseRange(context, requestHeaders, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
            Assert.Equal("Multiple ranges are not supported.", logger.LastMessage);
        }

        [Fact]
        public void ParseRange_NullRangeHeader_LogsDebugAndReturnsFalseNull()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = new StringValues("bytes=0-1");

            var requestHeaders = new RequestHeaders();
            requestHeaders.Range = null;

            var logger = new TestLogger();

            var result = RangeHelper.ParseRange(context, requestHeaders, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Equal(LogLevel.Debug, logger.LastLogLevel);
            Assert.Equal("Range header's value is invalid.", logger.LastMessage);
        }

        [Fact]
        public void ParseRange_ValidSingleRange_ReturnsTrueAndNormalizedRange()
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = new StringValues("bytes=0-10");

            var requestHeaders = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 10));
            requestHeaders.Range = rangeHeader;

            var logger = new TestLogger();

            var result = RangeHelper.ParseRange(context, requestHeaders, 100, logger);

            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range!.From);
            Assert.Equal(10, result.range.To);
            Assert.Null(logger.LastLogLevel);
            Assert.Null(logger.LastMessage);
        }
    }
}
