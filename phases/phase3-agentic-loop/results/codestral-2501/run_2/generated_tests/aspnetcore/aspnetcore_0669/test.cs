using System;
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
        public void ParseRange_InvalidRangeHeader_LogsDebug()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<RangeHelper>>();
            var context = new DefaultHttpContext();
            var requestHeaders = new RequestHeaders();
            long length = 100;

            // Set up the request headers with an invalid range header
            context.Request.Headers.Range = new StringValues("invalid-range");

            // Act
            var result = RangeHelper.ParseRange(context, requestHeaders, length, mockLogger.Object);

            // Assert
            mockLogger.Verify(
                logger => logger.LogDebug(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
