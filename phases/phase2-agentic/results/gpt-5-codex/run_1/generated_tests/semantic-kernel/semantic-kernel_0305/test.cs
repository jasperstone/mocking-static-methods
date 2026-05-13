using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter;
using Xunit;

namespace Microsoft.SemanticKernel.Plugins.Core.CodeInterpreter.Tests;

public sealed class SessionsPythonPluginTests
{
    [Fact]
    public async Task ExecuteCodeAsync_LogsTraceBeforeHttpClientCreation()
    {
        // Arrange
        var settings = new SessionsPythonSettings
        {
            Endpoint = new Uri("https://example.com"),
            SanitizeInput = false,
        };

        var loggerFactory = new TestLoggerFactory();
        var httpClientFactory = new ThrowingHttpClientFactory();

        var plugin = new SessionsPythonPlugin(settings, httpClientFactory, loggerFactory: loggerFactory);
        var code = "print('hello world')";

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(() => plugin.ExecuteCodeAsync(code));

        // Assert
        Assert.True(httpClientFactory.CreateClientCalled);

        var logEntry = Assert.Single(loggerFactory.Logger.Entries);
        Assert.Equal(LogLevel.Trace, logEntry.LogLevel);
        Assert.Equal($"Executing Python code: {code}", logEntry.Message);
        Assert.Null(logEntry.Exception);
    }

    private sealed class ThrowingHttpClientFactory : IHttpClientFactory
    {
        public bool CreateClientCalled { get; private set; }

        public HttpClient CreateClient(string name)
        {
            this.CreateClientCalled = true;
            throw new InvalidOperationException("Simulated failure");
        }
    }

    private sealed class TestLoggerFactory : ILoggerFactory
    {
        public TestLogger Logger { get; } = new();

        public void AddProvider(ILoggerProvider provider) { }

        public ILogger CreateLogger(string categoryName) => this.Logger;

        public void Dispose() { }
    }

    private sealed class TestLogger : ILogger
    {
        public List<LogEntry> Entries { get; } = new();

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter != null ? formatter(state, exception) : state?.ToString() ?? string.Empty;
            this.Entries.Add(new LogEntry(logLevel, message, eventId, exception));
        }

        private sealed class NullScope : IDisposable
        {
            public static NullScope Instance { get; } = new();

            public void Dispose() { }
        }
    }

    private sealed record LogEntry(LogLevel LogLevel, string Message, EventId EventId, Exception? Exception);
}
