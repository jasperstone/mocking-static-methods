using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using System.IO;
using Emby.Server.Implementations.Library;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StartWatchingPath_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreIgnoreRuleMock.Object);

            var path = "C:\\InvalidPath";

            // Act
            libraryMonitor.StartWatchingPath(path);

            // Assert
            loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error watching path: {Path}",
                    It.Is<object[]>(o => o.Length == 1 && o[0].Equals(path))
                ),
                Times.Once);
        }
    }
}
