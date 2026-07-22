using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using System.IO;
using Emby.Server.Implementations.Library;
using System.Reflection;

namespace Emby.Server.Tests.Implementations.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreIgnoreRuleMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Act
            var disposeWatcherMethod = typeof(LibraryMonitor).GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance);
            disposeWatcherMethod.Invoke(libraryMonitor, new object[] { watcher, true });

            // Assert
            loggerMock.Verify(
                x => x.LogInformation("Stopping directory watching for path {Path}", It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
