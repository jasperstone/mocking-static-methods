using Moq;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using Xunit;
using Emby.Server.Implementations.IO;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_ShouldLogInformation_WhenStoppingWatcher()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                null, // Mock IHostApplicationLifetime
                dotIgnoreIgnoreRuleMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = @"C:\TestPath"
            };

            var fileSystemWatchersField = typeof(LibraryMonitor).GetField("_fileSystemWatchers", BindingFlags.NonPublic | BindingFlags.Instance);
            var fileSystemWatchers = (ConcurrentDictionary<string, FileSystemWatcher>)fileSystemWatchersField.GetValue(libraryMonitor);
            fileSystemWatchers[watcher.Path] = watcher;

            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);

            // Act
            disposeWatcherMethod.Invoke(libraryMonitor, new object[] { watcher, true });

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Stopping directory watching for path {Path}")),
                    It.Is<object[]>(o => o[0].ToString() == watcher.Path)),
                Times.Once);
        }
    }
}
