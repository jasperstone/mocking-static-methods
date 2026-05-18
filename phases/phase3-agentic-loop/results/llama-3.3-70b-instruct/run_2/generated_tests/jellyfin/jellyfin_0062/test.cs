using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace Emby.Server.Implementations.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var dotIgnoreIgnoreRuleMock = new Mock<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, null, dotIgnoreIgnoreRuleMock.Object);
            var fileSystemWatcher = new FileSystemWatcher();

            // Act
            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            disposeWatcherMethod.Invoke(libraryMonitor, new object[] { fileSystemWatcher, true });

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", fileSystemWatcher.Path), Times.Once);
        }
    }
}
