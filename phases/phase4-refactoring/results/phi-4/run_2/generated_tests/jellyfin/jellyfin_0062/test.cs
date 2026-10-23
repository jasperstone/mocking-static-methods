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
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                null, // Mock or provide a suitable IHostApplicationLifetime
                dotIgnoreIgnoreRuleMock.Object);

            var watcher = new FileSystemWatcher
            {
                Path = @"C:\TestPath"
            };

            // Act
            libraryMonitor.DisposeWatcher(watcher, true);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.Is<string>(s => s.Contains("Stopping directory watching for path")),
                    It.Is<object>(o => o.ToString() == watcher.Path)),
                Times.Once);
        }
    }
}
