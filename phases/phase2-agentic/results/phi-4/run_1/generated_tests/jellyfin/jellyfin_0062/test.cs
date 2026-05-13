using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationMessage()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<LibraryMonitor>>();
            var fileSystemWatcher = new FileSystemWatcher();
            var libraryMonitor = new LibraryMonitor(
                mockLogger.Object,
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IHostApplicationLifetime>(),
                Mock.Of<DotIgnoreIgnoreRule>());

            // Act
            libraryMonitor.DisposeWatcher(fileSystemWatcher, true);

            // Assert
            mockLogger.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(s => s.Contains("Stopping directory watching for path")),
                    It.Is<object>(o => o == fileSystemWatcher.Path)),
                Times.Once);
        }
    }
}
