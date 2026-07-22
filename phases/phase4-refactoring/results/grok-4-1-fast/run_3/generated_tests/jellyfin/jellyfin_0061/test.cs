using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StartWatchingPath_FileSystemWatcherCreationFails_LogsError()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryMonitor>>();
            var libraryManager = Mock.Of<ILibraryManager>();
            var configurationManager = Mock.Of<IServerConfigurationManager>();
            var fileSystem = Mock.Of<IFileSystem>();
            var appLifetime = Mock.Of<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var monitor = new LibraryMonitor(logger.Object, libraryManager, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule);

            var invalidPath = @"\\.\InvalidDevicePath"; // Causes FileSystemWatcher ctor to throw

            // Act
            var startWatchingPathMethod = typeof(LibraryMonitor)
                .GetMethod("StartWatchingPath", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            startWatchingPathMethod.Invoke(monitor, new object[] { invalidPath });

            // Assert
            logger.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Error watching path: {Path}",
                    invalidPath),
                Times.Once);
        }

        [Fact]
        public void ReportFileSystemChangeComplete_ReportFileSystemChangedThrows_LogsError()
        {
            // Arrange
            var logger = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            libraryManagerMock.Setup(m => m.RootFolder).Throws(new InvalidOperationException("Simulated failure"));
            
            var configurationManager = Mock.Of<IServerConfigurationManager>();
            var fileSystem = Mock.Of<IFileSystem>();
            var appLifetime = Mock.Of<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();

            var monitor = new LibraryMonitor(logger.Object, libraryManagerMock.Object, configurationManager, fileSystem, appLifetime, dotIgnoreIgnoreRule);

            var path = @"C:\test";

            // Act
            monitor.ReportFileSystemChangeBeginning(path);
            
            var reportCompleteMethod = typeof(LibraryMonitor)
                .GetMethod("ReportFileSystemChangeComplete", BindingFlags.NonPublic | BindingFlags.Instance)!;
            
            // The async void method will execute synchronously for testing purposes
            reportCompleteMethod.Invoke(monitor, new object[] { path, true });

            // Assert
            logger.Verify(
                l => l.LogError(
                    It.IsAny<Exception>(),
                    "Error in ReportFileSystemChanged for {Path}",
                    path),
                Times.Once);
        }
    }
}
