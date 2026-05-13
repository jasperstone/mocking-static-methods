using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_EmptyRawRangeHeader_LogsTraceAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = StringValues.Empty;

            var requestHeaders = new RequestHeaders();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Range header's value is empty."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ParseRange_MultipleRangesInRawRangeHeader_LogsDebugAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues(new[] { "bytes=0-1", "bytes=2-3" });

            var requestHeaders = new RequestHeaders();
            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Multiple ranges are not supported."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ParseRange_NullRangeHeader_LogsDebugAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues("bytes=0-10");

            var requestHeaders = new RequestHeaders
            {
                Range = null
            };

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Range header's value is invalid."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void ParseRange_NullRanges_LogsDebugAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues("bytes=0-10");

            var requestHeaders = new RequestHeaders
            {
                Range = new RangeHeaderValue()
            };
            // Set Ranges to null via reflection since it's normally readonly
            var rangesProperty = typeof(RangeHeaderValue).GetProperty("Ranges");
            rangesProperty.SetValue(requestHeaders.Range, null);

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Range header's value is invalid."),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
