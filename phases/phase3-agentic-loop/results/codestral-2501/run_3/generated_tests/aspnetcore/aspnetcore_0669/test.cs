using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_EmptyRangeHeader_LogsTrace()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var requestHeaders = new RequestHeaders();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Trace,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Range header's value is empty.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_MultipleRanges_LogsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues(new List<string> { "bytes=0-10", "bytes=20-30" });
            var requestHeaders = new RequestHeaders();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Multiple ranges are not supported.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_InvalidRangeHeader_LogsDebug()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues("invalid");
            var requestHeaders = new RequestHeaders();

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Range header's value is invalid.")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRangeHeader_ZeroLength()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues("bytes=0-10");
            var requestHeaders = new RequestHeaders();
            requestHeaders.Range = new RangeHeaderValue(new List<RangeItemHeaderValue> { new RangeItemHeaderValue(0, 10) });

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 0, loggerMock.Object);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRangeHeader_NonZeroLength()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = new StringValues("bytes=0-10");
            var requestHeaders = new RequestHeaders();
            requestHeaders.Range = new RangeHeaderValue(new List<RangeItemHeaderValue> { new RangeItemHeaderValue(0, 10) });

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, 100, loggerMock.Object);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range.From);
            Assert.Equal(10, result.range.To);
        }
    }
}
