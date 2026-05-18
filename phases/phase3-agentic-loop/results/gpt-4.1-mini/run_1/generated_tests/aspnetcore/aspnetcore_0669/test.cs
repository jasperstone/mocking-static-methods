using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Internal;

namespace RangeHelperTests
{
    public class RangeHelperLoggerTests
    {
        [Fact]
        public void ParseRange_EmptyRawRangeHeader_LogsTraceAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = StringValues.Empty;

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, new Microsoft.AspNetCore.Http.Headers.RequestHeaders(context.Request.Headers), 100, loggerMock.Object);

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
        public void ParseRange_MultipleRanges_LogsDebugAndReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues(new[] { "bytes=0-1", "bytes=2-3" });

            var loggerMock = new Mock<ILogger>();

            // Act
            var result = RangeHelper.ParseRange(context, new Microsoft.AspNetCore.Http.Headers.RequestHeaders(context.Request.Headers), 100, loggerMock.Object);

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
    }
}
