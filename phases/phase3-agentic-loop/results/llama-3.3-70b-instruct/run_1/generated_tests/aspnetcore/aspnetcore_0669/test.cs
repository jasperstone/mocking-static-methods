using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_EmptyRangeHeader_ReturnsFalseAndNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            var requestHeaders = new HeaderDictionary();
            var length = 100;
            var logger = Mock.Of<ILogger>();

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_MultipleRangeEntries_ReturnsFalseAndNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10, bytes=20-30";
            var requestHeaders = new HeaderDictionary();
            var length = 100;
            var logger = Mock.Of<ILogger>();

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_InvalidRangeValue_ReturnsFalseAndNull()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "invalid";
            var requestHeaders = new HeaderDictionary();
            var length = 100;
            var logger = Mock.Of<ILogger>();

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.False(result.isRangeRequest);
            Assert.Null(result.range);
        }

        [Fact]
        public void ParseRange_ValidRangeValue_ReturnsTrueAndNormalizedRange()
        {
            // Arrange
            var context = new DefaultHttpContext();
            context.Request.Headers.Range = "bytes=0-10";
            var requestHeaders = new HeaderDictionary();
            var length = 100;
            var logger = Mock.Of<ILogger>();

            // Act
            var result = Microsoft.AspNetCore.Internal.RangeHelper.ParseRange(context, requestHeaders, length, logger);

            // Assert
            Assert.True(result.isRangeRequest);
            Assert.NotNull(result.range);
            Assert.Equal(0, result.range.From);
            Assert.Equal(10, result.range.To);
        }
    }
}
