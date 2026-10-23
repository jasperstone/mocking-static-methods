using System;
using System.IO;
using System.Reflection;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.Tests.IO
{
    public class LibraryMonitorTests
    {
        [Fact]
        public void DisposeWatcher_LogsInformationMessage()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<LibraryMonitor>>();
            var mockWatcher = new Mock<FileSystemWatcher>();
            mockWatcher.SetupGet(w => w.Path).Returns(@"C:\test\path");

            var libraryMonitor = CreateLibraryMonitor(loggerMock.Object);

            // Act
            InvokeDisposeWatcher(libraryMonitor, mockWatcher.Object, removeFromList: false);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Stopping directory watching for path C:\\test\\path")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private static LibraryMonitor CreateLibraryMonitor(ILogger<LibraryMonitor> logger)
        {
            var mockLibraryManager = new Mock<ILibraryManager>();
            var mockConfigManager = new Mock<IServerConfigurationManager>();
            var mockFileSystem = new Mock<IFileSystem>();
            var mockAppLifetime = new Mock<IHostApplicationLifetime>();
            var mockDotIgnore = new Mock<DotIgnoreIgnoreRule>();

            return new LibraryMonitor(
                logger,
                mockLibraryManager.Object,
                mockConfigManager.Object,
                mockFileSystem.Object,
                mockAppLifetime.Object,
                mockDotIgnore.Object);
        }

        private static void InvokeDisposeWatcher(LibraryMonitor monitor, FileSystemWatcher watcher, bool removeFromList)
        {
            var method = typeof(LibraryMonitor).GetMethod("DisposeWatcher", 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            method.Invoke(monitor, new object[] { watcher, removeFromList });
        }
    }
}
