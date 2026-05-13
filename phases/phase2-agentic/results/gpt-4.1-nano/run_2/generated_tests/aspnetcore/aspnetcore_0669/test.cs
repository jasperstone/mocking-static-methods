using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Internal;

namespace RangeHelperTests
{
    public class RangeHelperTests
    {
        private class DummyLogger : ILogger
        {
            public List<string> Logs = new List<string>();
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel logLevel) => true;
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                Logs.Add(formatter(state, exception));
            }
        }

        private DefaultHttpContext CreateHttpContextWithRangeHeader(string rangeHeaderValue)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = rangeHeaderValue;
            return context;
        }

        [Fact]
        public void ParseRange_HeaderIsEmpty_LogsTraceAndReturnsFalse()
        {
            var context = CreateHttpContextWithRangeHeader(string.Empty);
            var headers = new RequestHeaders();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is empty.", logger.Logs);
        }

        [Fact]
        public void ParseRange_MultipleRanges_LogsDebugAndReturnsFalse()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499,500-999");
            var headers = new RequestHeaders();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 1000, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Multiple ranges are not supported.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangeHeaderNull_LogsDebugAndReturnsFalse()
        {
            var context = new DefaultHttpContext();
            var headers = new RequestHeaders(); // no Range header
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 1000, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is invalid.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangesCountZero_ReturnsTrueNullRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var headers = new RequestHeaders();
            headers.Range = new RangeHeaderValue();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 1000, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_LengthZero_ReturnsTrueNullRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var headers = new RequestHeaders();
            headers.Range = new RangeHeaderValue();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 0, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRange_ReturnsNormalizedRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var headers = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges.Add(new RangeItemHeaderValue(0, 499));
            headers.Range = rangeHeader;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 1000, logger);

            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range.From);
            Assert.Equal(499, result.range.To);
        }

        [Fact]
        public void ParseRange_SuffixRange_ReturnsNormalizedRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=-200");
            var headers = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges.Add(new RangeItemHeaderValue(null, 200));
            headers.Range = rangeHeader;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 1000, logger);

            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(800, result.range.From);
            Assert.Equal(999, result.range.To);
        }

        [Fact]
        public void NormalizeRange_StartExceedsLength_ReturnsNull()
        {
            var range = new RangeItemHeaderValue(1500, 1600);
            var length = 1000;

            var result = RangeHelper.NormalizeRange(range, length);

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeRange_EndExceedsLength_AdjustsEnd()
        {
            var range = new RangeItemHeaderValue(900, 2000);
            var length = 1000;

            var normalized = RangeHelper.NormalizeRange(range, length);

            Assert.NotNull(normalized);
            Assert.Equal(900, normalized.From);
            Assert.Equal(999, normalized.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRange_ReturnsCorrectRange()
        {
            var range = new RangeItemHeaderValue(null, 500);
            var length = 1000;

            var normalized = RangeHelper.NormalizeRange(range, length);

            Assert.NotNull(normalized);
            Assert.Equal(500, normalized.From);
            Assert.Equal(999, normalized.To);
        }
    }
}
