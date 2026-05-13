using System;
using System.Collections.Concurrent;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void StartWatchingPath_WhenExceptionThrown_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var libraryManagerMock = new Mock<ILibraryManager>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var appLifetimeMock = new Mock<IHostApplicationLifetime>();
            var dotIgnoreMock = new Mock<DotIgnoreIgnoreRule>();

            // Setup appLifetime to allow Start to be called without error
            appLifetimeMock.Setup(a => a.ApplicationStarted).Returns(new Mock<IHostApplicationLifetime>().Object);
            appLifetimeMock.Setup(a => a.ApplicationStopping).Returns(new Mock<IHostApplicationLifetime>().Object);

            var monitor = new LibraryMonitor(
                loggerMock.Object,
                libraryManagerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                appLifetimeMock.Object,
                dotIgnoreMock.Object);

            // We need to simulate the StartWatchingPath method throwing an exception inside the Task.Run
            // The StartWatchingPath method is private, but it is called from Start method.
            // We will simulate the _libraryManager.RootFolder.Children to contain a Folder with a PhysicalLocation that causes an exception.

            // Setup a Folder with a PhysicalLocation that causes an exception when FileSystemWatcher is created
            var folderMock = new Mock<Folder>();
            folderMock.Setup(f => f.PhysicalLocations).Returns(new[] { "invalid_path_that_throws" });

            var rootFolderMock = new Mock<BaseItem>();
            rootFolderMock.Setup(r => r.Children).Returns(new BaseItem[] { folderMock.Object });

            libraryManagerMock.Setup(l => l.RootFolder).Returns(rootFolderMock.Object);
            libraryManagerMock.Setup(l => l.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new LibraryOptions { EnableRealtimeMonitor = true });

            // We will override the FileSystemWatcher creation by mocking FileSystemWatcher constructor indirectly by replacing StartWatchingPath with a method that throws.
            // Since we cannot override private methods, we will simulate the exception by using a path that causes FileSystemWatcher to throw.

            // Act
            // Call Start which will call StartWatchingPath internally
            monitor.Start();

            // Wait a bit for the Task.Run to execute
            System.Threading.Thread.Sleep(1000);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error watching path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
