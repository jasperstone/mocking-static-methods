using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        private class TestFileSystemWatcher : FileSystemWatcher
        {
            public TestFileSystemWatcher(string path)
            {
                // Do not call base.Path setter to avoid directory existence check
                base.Path = path;
            }

            public new string Path
            {
                get => base.Path;
                set
                {
                    // Override to avoid directory existence check
                    typeof(FileSystemWatcher).GetField("path", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(this, value);
                }
            }
        }

        [Fact]
        public void DisposeWatcher_LogsStoppingDirectoryWatching()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var dotIgnore = new DotIgnoreIgnoreRule();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnore);

            var watcher = new TestFileSystemWatcher("C:\\TestPath");

            // Add dummy event handlers to avoid null reference on -=
            watcher.Created += (s, e) => { };
            watcher.Deleted += (s, e) => { };
            watcher.Renamed += (s, e) => { };
            watcher.Changed += (s, e) => { };
            watcher.Error += (s, e) => { };

            // Act
            // Use reflection to invoke private DisposeWatcher method
            var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            Assert.NotNull(method);
            method.Invoke(libraryMonitor, new object[] { watcher, false });

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\TestPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
