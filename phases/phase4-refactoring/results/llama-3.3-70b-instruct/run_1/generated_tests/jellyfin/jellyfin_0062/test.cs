using Emby.Server.Implementations.IO;
using Microsoft.Extensions.Logging;
using Moq;
using System.IO;
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
            var libraryMonitor = new LibraryMonitor(loggerMock.Object, null, null, null, null, null);
            var fileSystemWatcher = new FileSystemWatcher();

            // Act
            libraryMonitor.DisposeWatcher(fileSystemWatcher, true);

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Stopping directory watching for path {Path}", fileSystemWatcher.Path), Times.Once);
        }
    }
}
