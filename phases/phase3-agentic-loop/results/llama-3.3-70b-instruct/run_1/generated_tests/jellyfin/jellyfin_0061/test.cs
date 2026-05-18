using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Emby.Server.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public async Task StartWatchingPath_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, null);

            // Act and Assert
            libraryMonitor.Start();
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ReportFileSystemChangeComplete_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, null);

            // Act and Assert
            libraryMonitor.ReportFileSystemChangeComplete("path", true);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error in ReportFileSystemChanged for {Path}", "path"), Times.Once);
        }
    }
}
