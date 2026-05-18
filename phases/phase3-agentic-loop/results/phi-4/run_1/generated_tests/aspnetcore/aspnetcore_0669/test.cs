using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.AspNetCore.Internal; // Ensure this namespace is included

namespace Microsoft.AspNetCore.Internal.Tests
{
    public class RangeHelperTests
    {
        [Fact]
        public void ParseRange_LogsDebugMessage_WhenRangeHeaderIsNull()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var context = new DefaultHttpContext();
            var requestHeaders = new RequestHeaders();
            long length = 100;

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, loggerMock.Object);

            // Assert
            loggerMock.Verify(
                l => l.LogDebug("Range header's value is invalid."),
                Times.Once);
            Assert.Equal((false, null), result);
        }
    }
}
