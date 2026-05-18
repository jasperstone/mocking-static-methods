using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Microsoft.Net.Http.Headers;

namespace Microsoft.AspNetCore.Internal.Tests;

public class RangeHelperTests
{
    [Fact]
    public void ParseRange_LogsDebugWhenRangesIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-9";

        var requestHeaders = new RequestHeaders();
        requestHeaders.Range = new RangeHeaderValue("bytes=0-9") { Ranges = null };

        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Debug)).Returns(true);

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 10, mockLogger.Object);

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                0,
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Range header's value is invalid.")),
                null,
                It.IsAny<Exception>()),
            Times.Once);

        Assert.True(result.isRangeRequest);
        Assert.Null(result.range);
    }
}
