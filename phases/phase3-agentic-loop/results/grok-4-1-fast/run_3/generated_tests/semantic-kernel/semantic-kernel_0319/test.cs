using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SemanticKernel.Plugins.MsGraph;
using Microsoft.SemanticKernel.Plugins.MsGraph.Models;
using Moq;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.MsGraph.Tests;

public class CalendarPluginTests
{
    private readonly Mock<ICalendarConnector> _mockConnector;
    private readonly TestLogger<CalendarPlugin> _testLogger;
    private readonly CalendarPlugin _plugin;

    public CalendarPluginTests()
    {
        _mockConnector = new Mock<ICalendarConnector>();
        _testLogger = new TestLogger<CalendarPlugin>();
        var loggerFactory = new LoggerFactory();
        loggerFactory.AddProvider(new TestLoggerProvider(_testLogger));
        _plugin = new CalendarPlugin(_mockConnector.Object, loggerFactory);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithDefaultParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(It.IsAny<int?>(), It.IsAny<int?>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((IEnumerable<CalendarEvent>?)null);

        // Act
        await _plugin.GetCalendarEventsAsync();

        // Assert
        Assert.Contains("Getting calendar events with query options top: '10', skip:'0'.", _testLogger.Messages);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithCustomParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(5, 3, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((IEnumerable<CalendarEvent>?)null);

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: 5, skip: 3);

        // Assert
        Assert.Contains("Getting calendar events with query options top: '5', skip:'3'.", _testLogger.Messages);
    }

    [Fact]
    public async Task GetCalendarEventsAsync_LogsDebugMessageWithNullParameters()
    {
        // Arrange
        _mockConnector.Setup(x => x.GetEventsAsync(null, null, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync((IEnumerable<CalendarEvent>?)null);

        // Act
        await _plugin.GetCalendarEventsAsync(maxResults: null, skip: null);

        // Assert
        Assert.Contains("Getting calendar events with query options top: '', skip:''.", _testLogger.Messages);
    }
}

public class TestLogger<T> : ILogger<T>
{
    public List<string> Messages { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) => null!;

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Messages.Add(formatter(state, exception));
    }
}

public class TestLoggerProvider : ILoggerProvider
{
    private readonly TestLogger<CalendarPlugin> _logger;

    public TestLoggerProvider(TestLogger<CalendarPlugin> logger)
    {
        _logger = logger;
    }

    public ILogger CreateLogger(string categoryName) => _logger;

    public void Dispose() { }
}
