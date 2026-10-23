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
            var fileSystemWatcher = new FileSystemWatcher("C:\\test\\path");

            var libraryMonitor = new LibraryMonitorTestFixture(logger.Object).Create();

            // Act
            libraryMonitor.CallDisposeWatcher(fileSystemWatcher, removeFromList: false);

            // Assert
            logger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t, _) => v.ToString().Contains("Stopping directory watching for path C:\\test\\path"))),
                Times.Once);
        }

        private class LibraryMonitorTestFixture
        {
            private readonly ILogger<LibraryMonitor> _logger;
            private readonly Mock<ILibraryManager> _libraryManager;
            private readonly Mock<IServerConfigurationManager> _configurationManager;
            private readonly Mock<IFileSystem> _fileSystem;
            private readonly Mock<IHostApplicationLifetime> _appLifetime;

            public LibraryMonitorTestFixture(ILogger<LibraryMonitor> logger)
            {
                _logger = logger;
                _libraryManager = new Mock<ILibraryManager>();
                _configurationManager = new Mock<IServerConfigurationManager>();
                _fileSystem = new Mock<IFileSystem>();
                _appLifetime = new Mock<IHostApplicationLifetime>();
            }

            public LibraryMonitor Create()
            {
                return new LibraryMonitor(
                    _logger,
                    _libraryManager.Object,
                    _configurationManager.Object,
                    _fileSystem.Object,
                    _appLifetime.Object,
                    null!);
            }
        }
    }

    public static class LibraryMonitorExtensions
    {
        public static void CallDisposeWatcher(this LibraryMonitor monitor, FileSystemWatcher watcher, bool removeFromList)
        {
            var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", 
                BindingFlags.NonPublic | BindingFlags.Instance);
            method!.Invoke(monitor, new object[] { watcher, removeFromList });
        }
    }
}
