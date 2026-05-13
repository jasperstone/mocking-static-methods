using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;

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

        private HttpContext CreateHttpContextWithRangeHeader(string rangeHeaderValue)
        {
            var context = new DefaultHttpContext();
            context.Request.Headers["Range"] = rangeHeaderValue;
            return context;
        }

        private RequestHeaders CreateRequestHeadersWithRange(HeaderRangeItem[] ranges)
        {
            var headers = new RequestHeaders();
            headers.Range = new RangeHeaderValue(ranges);
            return headers;
        }

        [Fact]
        public void ParseRange_HeaderMissing_ShouldLogTraceAndReturnFalseNull()
        {
            var context = new DefaultHttpContext();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, new RequestHeaders(), 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is empty.", logger.Logs);
        }

        [Fact]
        public void ParseRange_MultipleRanges_ShouldLogDebugAndReturnFalseNull()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499,500-999");
            var logger = new DummyLogger();

            var requestHeaders = new RequestHeaders
            {
                Range = new RangeHeaderValue(
                    new[] { new RangeItemHeaderValue(0, 499), new RangeItemHeaderValue(500, 999) }
                )
            };

            var result = RangeHelper.ParseRange(context, requestHeaders, 1000, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Multiple ranges are not supported.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangeHeaderNull_ShouldLogDebugAndReturnFalseNull()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var logger = new DummyLogger();

            var requestHeaders = new RequestHeaders
            {
                Range = null
            };

            var result = RangeHelper.ParseRange(context, requestHeaders, 1000, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is invalid.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangesCountZero_ShouldReturnTrueNull()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var logger = new DummyLogger();

            var requestHeaders = new RequestHeaders
            {
                Range = new RangeHeaderValue(new RangeItemHeaderValue[0])
            };

            var result = RangeHelper.ParseRange(context, requestHeaders, 1000, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_LengthZero_ShouldReturnTrueNull()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var logger = new DummyLogger();

            var requestHeaders = new RequestHeaders
            {
                Range = new RangeHeaderValue(new[] { new RangeItemHeaderValue(0, 499) })
            };

            var result = RangeHelper.ParseRange(context, requestHeaders, 0, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRange_ShouldReturnNormalizedRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-499");
            var logger = new DummyLogger();

            var requestHeaders = new RequestHeaders
            {
                Range = new RangeHeaderValue(new[] { new RangeItemHeaderValue(0, 499) })
            };

            var result = RangeHelper.ParseRange(context, requestHeaders, 1000, logger);

            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range.From);
            Assert.Equal(499, result.range.To);
        }

        [Fact]
        public void NormalizeRange_StartExceedsLength_ShouldReturnNull()
        {
            var range = new RangeItemHeaderValue(1500, 1600);
            var normalized = RangeHelper.NormalizeRange(range, 1000);
            Assert.Null(normalized);
        }

        [Fact]
        public void NormalizeRange_EndExceedsLength_ShouldAdjustEnd()
        {
            var range = new RangeItemHeaderValue(0, 2000);
            var normalized = RangeHelper.NormalizeRange(range, 1000);
            Assert.NotNull(normalized);
            Assert.Equal(0, normalized.From);
            Assert.Equal(999, normalized.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRange_ShouldCalculateStartAndEnd()
        {
            var range = new RangeItemHeaderValue(null, 500);
            var normalized = RangeHelper.NormalizeRange(range, 1000);
            Assert.NotNull(normalized);
            Assert.Equal(500, normalized.From);
            Assert.Equal(999, normalized.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRangeZeroBytes_ShouldReturnNull()
        {
            var range = new RangeItemHeaderValue(null, 0);
            var normalized = RangeHelper.NormalizeRange(range, 1000);
            Assert.Null(normalized);
        }
    }
}
