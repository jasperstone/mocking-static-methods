using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.LibraryTaskScheduler;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.LibraryTaskScheduler;

public class LimitedConcurrencyLibrarySchedulerTests
{
    [Fact]
    public async Task Enqueue_WhenSequentialOperationForced_LogsCompletionDebugMessage()
    {
        var logger = new TestLogger<LimitedConcurrencyLibraryScheduler>();
        using var hostLifetime = new TestHostApplicationLifetime();
        var configuration = new ServerConfiguration
        {
            LibraryScanFanoutConcurrency = 1
        };

        var configManager = new Mock<IServerConfigurationManager>();
        configManager.SetupGet(manager => manager.Configuration).Returns(configuration);

        await using var scheduler = new LimitedConcurrencyLibraryScheduler(hostLifetime, logger, configManager.Object);

        var workerExecuted = false;

        Func<int, IProgress<double>, Task> worker = (value, progressReporter) =>
        {
            workerExecuted = true;
            progressReporter.Report(50);
            return Task.CompletedTask;
        };

        await scheduler.Enqueue(new[] { 1 }, worker, new Progress<double>(_ => { }), CancellationToken.None);

        Assert.True(workerExecuted);
        Assert.Contains(logger.Logs, entry => entry.Level == LogLevel.Debug && entry.Message == "Process sequentially done.");
    }

    private sealed record LogEntry(LogLevel Level, string Message);

    private sealed class TestLogger<T> : ILogger<T>
    {
        private readonly List<LogEntry> _logs = new();

        public IReadOnlyList<LogEntry> Logs => _logs;

        public IDisposable BeginScope<TState>(TState state) => NullScope.Instance;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            var message = formatter?.Invoke(state, exception) ?? state?.ToString() ?? string.Empty;
            _logs.Add(new LogEntry(logLevel, message));
        }

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();

            public void Dispose()
            {
            }
        }
    }

    private sealed class TestHostApplicationLifetime : IHostApplicationLifetime, IDisposable
    {
        private readonly CancellationTokenSource _started = new();
        private readonly CancellationTokenSource _stopping = new();
        private readonly CancellationTokenSource _stopped = new();

        public CancellationToken ApplicationStarted => _started.Token;

        public CancellationToken ApplicationStopping => _stopping.Token;

        public CancellationToken ApplicationStopped => _stopped.Token;

        public void StopApplication()
        {
            if (!_stopping.IsCancellationRequested)
            {
                _stopping.Cancel();
            }

            if (!_stopped.IsCancellationRequested)
            {
                _stopped.Cancel();
            }
        }

        public void Dispose()
        {
            _started.Dispose();
            _stopping.Dispose();
            _stopped.Dispose();
        }
    }
}
