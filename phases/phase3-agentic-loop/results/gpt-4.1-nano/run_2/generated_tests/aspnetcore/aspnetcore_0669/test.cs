using Xunit;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;
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

        private HttpContext CreateHttpContextWithRangeHeader(string rangeHeaderValue)
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
            var context = CreateHttpContextWithRangeHeader("bytes=0-1,2-3");
            var headers = new RequestHeaders();
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Multiple ranges are not supported.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangeHeaderNull_LogsDebugAndReturnsFalse()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-1");
            var headers = new RequestHeaders();
            headers.Range = null;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 100, logger);

            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            Assert.Contains("Range header's value is invalid.", logger.Logs);
        }

        [Fact]
        public void ParseRange_RangesCountZero_ReturnsTrueNullRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-1");
            var headers = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges = new List<RangeItemHeaderValue>();
            headers.Range = rangeHeader;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 100, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_LengthZero_ReturnsTrueNullRange()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-1");
            var headers = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges = new List<RangeItemHeaderValue> { new RangeItemHeaderValue(0, 1) };
            headers.Range = rangeHeader;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 0, logger);

            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRange_NormalizesCorrectly()
        {
            var context = CreateHttpContextWithRangeHeader("bytes=0-1");
            var headers = new RequestHeaders();
            var rangeHeader = new RangeHeaderValue();
            rangeHeader.Ranges = new List<RangeItemHeaderValue> { new RangeItemHeaderValue(0, 1) };
            headers.Range = rangeHeader;
            var logger = new DummyLogger();

            var result = RangeHelper.ParseRange(context, headers, 10, logger);

            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range.From);
            Assert.Equal(1, result.range.To);
        }

        [Fact]
        public void NormalizeRange_StartExceedsLength_ReturnsNull()
        {
            var range = new RangeItemHeaderValue(15, 20);
            var length = 10;

            var result = RangeHelper.NormalizeRange(range, length);

            Assert.Null(result);
        }

        [Fact]
        public void NormalizeRange_EndExceedsLength_AdjustsToLengthMinusOne()
        {
            var range = new RangeItemHeaderValue(0, 20);
            var length = 10;

            var result = RangeHelper.NormalizeRange(range, length);

            Assert.NotNull(result);
            Assert.Equal(0, result.From);
            Assert.Equal(9, result.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRange_CalculatesStartAndEnd()
        {
            var range = new RangeItemHeaderValue(null, 5);
            var length = 10;

            var result = RangeHelper.NormalizeRange(range, length);

            Assert.NotNull(result);
            Assert.Equal(5, result.From);
            Assert.Equal(9, result.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRangeZero_ReturnsNull()
        {
            var range = new RangeItemHeaderValue(null, 0);
            var length = 10;

            var result = RangeHelper.NormalizeRange(range, length);

            Assert.Null(result);
        }
    }
}
