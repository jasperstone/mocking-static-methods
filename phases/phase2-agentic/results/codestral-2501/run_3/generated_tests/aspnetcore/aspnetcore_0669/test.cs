using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_EmptyRangeHeader_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestHeadersMock = new Mock<RequestHeaders>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers.Range).Returns(StringValues.Empty);

            // Act
            var result = RangeHelper.ParseRange(contextMock.Object, requestHeadersMock.Object, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(logger => logger.LogTrace("Range header's value is empty."), Times.Once);
        }

        [Fact]
        public void ParseRange_MultipleRanges_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestHeadersMock = new Mock<RequestHeaders>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers.Range).Returns(new StringValues(new[] { "bytes=0-10", "bytes=20-30" }));

            // Act
            var result = RangeHelper.ParseRange(contextMock.Object, requestHeadersMock.Object, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(logger => logger.LogDebug("Multiple ranges are not supported."), Times.Once);
        }

        [Fact]
        public void ParseRange_InvalidRangeHeader_ReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestHeadersMock = new Mock<RequestHeaders>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers.Range).Returns(new StringValues("invalid"));

            // Act
            var result = RangeHelper.ParseRange(contextMock.Object, requestHeadersMock.Object, 100, loggerMock.Object);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
            loggerMock.Verify(logger => logger.LogDebug("Range header's value is invalid."), Times.Once);
        }

        [Fact]
        public void ParseRange_ValidRangeHeader_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestHeadersMock = new Mock<RequestHeaders>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers.Range).Returns(new StringValues("bytes=0-10"));
            requestHeadersMock.Setup(r => r.Range).Returns(new RangeHeaderValue(new RangeItemHeaderValue(0, 10)));

            // Act
            var result = RangeHelper.ParseRange(contextMock.Object, requestHeadersMock.Object, 100, loggerMock.Object);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range?.From);
            Assert.Equal(10, result.range?.To);
        }

        [Fact]
        public void ParseRange_ZeroLength_ReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var requestHeadersMock = new Mock<RequestHeaders>();
            var contextMock = new Mock<HttpContext>();
            contextMock.Setup(c => c.Request.Headers.Range).Returns(new StringValues("bytes=0-10"));
            requestHeadersMock.Setup(r => r.Range).Returns(new RangeHeaderValue(new RangeItemHeaderValue(0, 10)));

            // Act
            var result = RangeHelper.ParseRange(contextMock.Object, requestHeadersMock.Object, 0, loggerMock.Object);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void NormalizeRange_StartGreaterThanLength_ReturnsNull()
        {
            // Arrange
            var range = new RangeItemHeaderValue(100, 150);

            // Act
            var result = RangeHelper.NormalizeRange(range, 50);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void NormalizeRange_EndGreaterThanLength_ReturnsEndAtLengthMinusOne()
        {
            // Arrange
            var range = new RangeItemHeaderValue(0, 100);

            // Act
            var result = RangeHelper.NormalizeRange(range, 50);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result?.From);
            Assert.Equal(49, result?.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRange_ReturnsCorrectRange()
        {
            // Arrange
            var range = new RangeItemHeaderValue(null, 50);

            // Act
            var result = RangeHelper.NormalizeRange(range, 100);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(50, result?.From);
            Assert.Equal(99, result?.To);
        }

        [Fact]
        public void NormalizeRange_SuffixRangeZero_ReturnsNull()
        {
            // Arrange
            var range = new RangeItemHeaderValue(null, 0);

            // Act
            var result = RangeHelper.NormalizeRange(range, 100);

            // Assert
            Assert.Null(result);
        }
    }
}
