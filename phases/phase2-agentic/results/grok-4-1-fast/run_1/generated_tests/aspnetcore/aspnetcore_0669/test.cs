using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Internal.Tests;

public class RangeHelperTests
{
    private static readonly Mock<ILogger> MockLogger = new();

    [Fact]
    public void ParseRange_LogsDebugWhenRangesIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-10"; // Valid raw header

        var requestHeaders = new RequestHeaders();
        requestHeaders.Range = new RangeHeaderValue("bytes")
        {
            Ranges = null // This triggers the specific LogDebug call on line ~72
        };

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 100, MockLogger.Object);

        // Assert
        MockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyFormat<string>>((v, t) => v.ToString().Contains("Range header's value is invalid.")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyFormat<string>, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public void ParseRange_RangeHeaderParsedButRangesNull_ReturnsExpected()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-10";

        var requestHeaders = new RequestHeaders();
        requestHeaders.Range = new RangeHeaderValue("bytes")
        {
            Ranges = null
        };

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 100, MockLogger.Object);

        // Assert
        Assert.False(result.isRangeRequest);
        Assert.Null(result.range);
        MockLogger.Verify(l => l.LogDebug("Range header's value is invalid."), Times.Once);
    }
}
