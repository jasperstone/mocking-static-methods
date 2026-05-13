using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Internal
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_MultipleRanges_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10,20-30";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = NullLogger.Instance;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_InvalidRangeHeader_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "invalid";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = NullLogger.Instance;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_EmptyRangeHeader_ReturnsFalse()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = NullLogger.Instance;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_SingleValidRange_ReturnsTrue()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = NullLogger.Instance;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
        }

        [Fact]
        public void ParseRange_LengthZero_ReturnsTrue()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10";
            var requestHeaders = context.Request.Headers;
            var length = 0;
            var logger = NullLogger.Instance;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void NormalizeRange_InvalidRange_ReturnsNull()
        {
            // Arrange
            var range = new RangeItemHeaderValue(100, 200);
            var length = 50;

            // Act
            var result = RangeHelper.NormalizeRange(range, length);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void NormalizeRange_ValidRange_ReturnsNormalizedRange()
        {
            // Arrange
            var range = new RangeItemHeaderValue(0, 10);
            var length = 100;

            // Act
            var result = RangeHelper.NormalizeRange(range, length);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.From);
            Assert.Equal(10, result.To);
        }
    }
}
