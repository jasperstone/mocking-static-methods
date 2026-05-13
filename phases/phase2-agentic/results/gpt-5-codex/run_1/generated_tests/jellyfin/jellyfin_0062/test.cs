using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Tests.Emby.Server.Implementations.IO;

public class LibraryMonitorTests
{
    [Fact]
    public void DisposeWatcher_LogsStoppingDirectory()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<LibraryMonitor>>();

        var libraryManagerMock = new Mock<ILibraryManager>();
        var configurationManagerMock = new Mock<IServerConfigurationManager>();
        var fileSystemMock = new Mock<IFileSystem>();
        var appLifetimeMock = new Mock<IHostApplicationLifetime>();

        appLifetimeMock.SetupGet(x => x.ApplicationStarted).Returns(CancellationToken.None);
        appLifetimeMock.SetupGet(x => x.ApplicationStopping).Returns(CancellationToken.None);
        appLifetimeMock.SetupGet(x => x.ApplicationStopped).Returns(CancellationToken.None);

        var monitor = new LibraryMonitor(
            loggerMock.Object,
            libraryManagerMock.Object,
            configurationManagerMock.Object,
            fileSystemMock.Object,
            appLifetimeMock.Object,
            new DotIgnoreIgnoreRule());

        var watcher = new FileSystemWatcher(AppContext.BaseDirectory);
        var expectedPath = watcher.Path;

        var watchersDictionary = GetWatchersDictionary(monitor);
        watchersDictionary[expectedPath] = watcher;

        // Act
        InvokeDisposeWatcher(monitor, watcher, removeFromList: true);

        // Assert
        Assert.False(watchersDictionary.ContainsKey(expectedPath));

        loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => CheckLogState(state, expectedPath)),
                It.Is<Exception>(ex => ex == null),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
            Times.Once);
    }

    private static ConcurrentDictionary<string, FileSystemWatcher> GetWatchersDictionary(LibraryMonitor monitor)
    {
        var field = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(field);

        var value = field.GetValue(monitor);
        Assert.NotNull(value);

        return (ConcurrentDictionary<string, FileSystemWatcher>)value;
    }

    private static void InvokeDisposeWatcher(LibraryMonitor monitor, FileSystemWatcher watcher, bool removeFromList)
    {
        var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);

        method.Invoke(monitor, new object[] { watcher, removeFromList });
    }

    private static bool CheckLogState(object state, string expectedPath)
    {
        if (!string.Equals(state?.ToString(), $"Stopping directory watching for path {expectedPath}", StringComparison.Ordinal))
        {
            return false;
        }

        if (state is IReadOnlyList<KeyValuePair<string, object?>> values)
        {
            string? path = null;
            string? originalFormat = null;

            foreach (var kv in values)
            {
                if (kv.Key == "Path")
                {
                    path = kv.Value?.ToString();
                }
                else if (kv.Key == "{OriginalFormat}")
                {
                    originalFormat = kv.Value?.ToString();
                }
            }

            return path == expectedPath && originalFormat == "Stopping directory watching for path {Path}";
        }

        return true;
    }
}
