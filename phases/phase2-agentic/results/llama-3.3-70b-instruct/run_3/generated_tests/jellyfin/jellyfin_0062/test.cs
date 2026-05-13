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
        public void DisposeWatcher_LogsInformation()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IHostApplicationLifetime>(),
                Mock.Of<DotIgnoreIgnoreRule>());

            var watcher = new FileSystemWatcher();

            // Act
            libraryMonitor.DisposeWatcher(watcher, true);

            // Assert
            loggerMock.Verify(
                l => l.LogInformation(It.Is<string>(s => s.Contains("Stopping directory watching for path")), It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
