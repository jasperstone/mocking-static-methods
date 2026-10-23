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
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, dotIgnoreIgnoreRuleMock.Object);

            // Act and Assert
            libraryManagerMock.Setup(l => l.RootFolder).Throws(new Exception());
            libraryMonitor.Start();
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void StartWatchingPath_DoesNotLogError_WhenNoExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, libraryManagerMock.Object, configurationManagerMock.Object, fileSystemMock.Object, appLifetimeMock.Object, dotIgnoreIgnoreRuleMock.Object);

            // Act
            libraryManagerMock.Setup(l => l.RootFolder).Returns(new Folder());
            libraryMonitor.Start();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", It.IsAny<string>()), Times.Never);
        }
    }
}
