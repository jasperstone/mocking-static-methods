using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Emby.Server.Implementations.IO;

namespace Emby.Server.Implementations.IO.Tests
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
                Mock.Of<DotIgnoreIgnoreRule>()
            );

            var watcher = new FileSystemWatcher
            {
                Path = "C:\\TestPath"
            };

            // Act
            libraryMonitor.DisposeWatcher(watcher, true);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
