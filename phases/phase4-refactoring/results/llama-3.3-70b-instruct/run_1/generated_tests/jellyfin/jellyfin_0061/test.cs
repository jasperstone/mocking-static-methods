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
        public void StartWatchingPath_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<Emby.Server.Implementations.IO.DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, dotIgnoreIgnoreRuleMock.Object);

            // Act and Assert
            libraryMonitor.Start();
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void StartWatchingPath_DoesNotLogError_WhenNoExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<MediaBrowser.Controller.Library.ILibraryManager>();
            var configurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            var appLifetimeMock = new Mock<Microsoft.Extensions.Hosting.IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<Emby.Server.Implementations.IO.DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, dotIgnoreIgnoreRuleMock.Object);

            // Act
            libraryMonitor.Start();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", It.IsAny<string>()), Times.Never);
        }
    }
}
