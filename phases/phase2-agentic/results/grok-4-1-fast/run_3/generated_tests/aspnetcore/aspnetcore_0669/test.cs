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
    private static readonly MockLogger Logger = new();

    [Fact]
    public void ParseRange_LogsDebug_WhenRangesIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-10"; // Valid raw header

        var requestHeaders = new RequestHeaders();
        requestHeaders.Range = new RangeHeaderValue("bytes")
        {
            Ranges = null // This triggers the log on line 72
        };

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 100, Logger);

        // Assert
        Assert.Equal("Range header's value is invalid.", Logger.DebugMessage);
        Assert.Equal((false, null), result);
    }

    [Fact]
    public void ParseRange_LogsDebug_WhenRangeHeaderIsNull()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-10";

        var requestHeaders = new RequestHeaders();

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 100, Logger);

        // Assert
        Assert.Equal("Range header's value is invalid.", Logger.DebugMessage);
        Assert.Equal((false, null), result);
    }

    [Fact]
    public void ParseRange_LogsDebug_WhenMultipleRanges()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers.Range = "bytes=0-10,20-30"; // Multiple ranges

        var requestHeaders = new RequestHeaders();

        // Act
        var result = RangeHelper.ParseRange(context, requestHeaders, 100, Logger);

        // Assert
        Assert.Equal("Multiple ranges are not supported.", Logger.DebugMessage);
        Assert.Equal((false, null), result);
    }

    private class MockLogger : ILogger
    {
        public string? DebugMessage { get; private set; }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null!;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Debug)
            {
                DebugMessage = formatter(state, exception);
            }
        }
    }
}
