using Moq;
using Microsoft.Extensions.Logging;
using System.IO;
using Xunit;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using Emby.Server.Implementations.Configuration;
using Emby.Server.Implementations;
using Microsoft.Extensions.Hosting;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationWhenStoppingWatcher()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var fileSystemWatcher = new FileSystemWatcher
            {
                Path = @"C:\TestPath"
            };

            var libraryMonitor = new LibraryMonitor(
                loggerMock.Object,
                Mock.Of<ILibraryManager>(),
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IHostApplicationLifetime>(),
                Mock.Of<DotIgnoreIgnoreRule>()
            );

            // Act
            libraryMonitor.DisposeWatcher(fileSystemWatcher, true);

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping directory watching for path C:\\TestPath")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }
    }
}
