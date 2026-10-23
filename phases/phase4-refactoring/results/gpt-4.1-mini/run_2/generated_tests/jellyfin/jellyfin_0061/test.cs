using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Emby.Server.Implementations.IO;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Emby.Server.Implementations.IO.Tests
{
    public class LibraryMonitorTests
    {
        private readonly Mock<ILogger<LibraryMonitor>> _loggerMock;
        private readonly Mock<ILibraryManager> _libraryManagerMock;
        private readonly Mock<IServerConfigurationManager> _configurationManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IHostApplicationLifetime> _appLifetimeMock;
        private readonly DotIgnoreIgnoreRule _dotIgnoreIgnoreRule;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRule = new DotIgnoreIgnoreRule();
        }

        [Fact]
        public void Start_WhenFileSystemWatcherCreationThrows_LogsError()
        {
            // Arrange
            var folder = new Folder
            {
                PhysicalLocations = new List<string> { "?:\\invalid_path" }
            };

            var rootFolderMock = new Mock<BaseItem>();
            rootFolderMock.Setup(r => r.Children).Returns(new List<BaseItem> { folder });

            _libraryManagerMock.Setup(m => m.RootFolder).Returns(rootFolderMock.Object);
            _libraryManagerMock.Setup(m => m.GetLibraryOptions(It.IsAny<BaseItem>())).Returns(new LibraryOptions { EnableRealtimeMonitor = true });

            var monitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreIgnoreRule);

            // Act
            monitor.Start();

            // Wait a short time for any async code to run
            Task.Delay(100).Wait();

            // Assert
            _loggerMock.Verify(
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
