using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StopWatchingPath_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var dotIgnoreIgnoreRuleMock = new Mock<Emby.Server.Implementations.IO.DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, null, dotIgnoreIgnoreRuleMock.Object);
            var fileSystemWatcher = new FileSystemWatcher();
            libraryMonitor.GetType().GetField("_fileSystemWatchers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(libraryMonitor, new System.Collections.Concurrent.ConcurrentDictionary<string, FileSystemWatcher>(StringComparer.OrdinalIgnoreCase));
            ((System.Collections.Concurrent.ConcurrentDictionary<string, FileSystemWatcher>)libraryMonitor.GetType().GetField("_fileSystemWatchers", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(libraryMonitor)).TryAdd("path", fileSystemWatcher);

            // Act
            libraryMonitor.GetType().GetMethod("StopWatchingPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(libraryMonitor, new object[] { "path" });

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", fileSystemWatcher.Path), Times.Once);
        }
    }
}
