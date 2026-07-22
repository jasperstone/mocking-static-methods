using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationMessage()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryMonitor>>();
            var mockWatcher = new Mock<FileSystemWatcher>();
            mockWatcher.SetupGet(w => w.Path).Returns("/test/path");

            var libraryManager = new Mock<ILibraryManager>();
            var configManager = new Mock<IServerConfigurationManager>();
            var fileSystem = new Mock<IFileSystem>();
            var appLifetime = new Mock<IHostApplicationLifetime>();

            var monitor = new LibraryMonitor(
                logger.Object,
                libraryManager.Object,
                configManager.Object,
                fileSystem.Object,
                appLifetime.Object,
                Mock.Of<Emby.Server.Implementations.Library.DotIgnoreIgnoreRule>());

            // Use reflection to call private DisposeWatcher
            var disposeWatcherMethod = typeof(LibraryMonitor)
                .GetMethod("DisposeWatcher", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // Act
            disposeWatcherMethod.Invoke(monitor, [mockWatcher.Object, false]);

            // Assert
            logger.Verify(
                l => l.Log(
                    It.Is<LogLevel>(level => level == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stopping directory watching for path /test/path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
