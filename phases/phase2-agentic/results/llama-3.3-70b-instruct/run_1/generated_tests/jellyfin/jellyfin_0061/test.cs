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
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, null, null, null, null, null);

            // Act and Assert
            libraryMonitor.StartWatchingPath("path");
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error watching path: {Path}", "path"), Times.Once);
        }

        [Fact]
        public void StartWatchingPath_DisposesWatcher_WhenExceptionOccurs()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, null, null, null, null, null);
            var watcherMock = new Mock<FileSystemWatcher>();

            // Act and Assert
            libraryMonitor.StartWatchingPath("path");
            watcherMock.Verify(w => w.Dispose(), Times.Once);
        }
    }
}
