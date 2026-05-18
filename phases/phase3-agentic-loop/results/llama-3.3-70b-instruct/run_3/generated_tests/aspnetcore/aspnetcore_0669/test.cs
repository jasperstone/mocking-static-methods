using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_EmptyRangeHeader_ReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = StringValues.Empty;
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = new LoggerFactory().CreateLogger("Test");

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_MultipleRanges_ReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10, bytes=20-30";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = new LoggerFactory().CreateLogger("Test");

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_InvalidRangeHeader_ReturnsFalseNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "invalid";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = new LoggerFactory().CreateLogger("Test");

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRangeHeader_ReturnsTrueRange()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10";
            var requestHeaders = context.Request.Headers;
            var length = 100;
            var logger = new LoggerFactory().CreateLogger("Test");

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
        }
    }
}
