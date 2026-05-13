using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Emby.Server.Implementations.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
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
        private readonly Mock<DotIgnoreIgnoreRule> _dotIgnoreIgnoreRuleMock;

        public LibraryMonitorTests()
        {
            _loggerMock = new Mock<ILogger<LibraryMonitor>>();
            _libraryManagerMock = new Mock<ILibraryManager>();
            _configurationManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _appLifetimeMock = new Mock<IHostApplicationLifetime>();
            _dotIgnoreIgnoreRuleMock = new Mock<DotIgnoreIgnoreRule>();
        }

        [Fact]
        public void StartWatchingPath_ShouldLogError_WhenExceptionIsThrown()
        {
            // Arrange
            var path = "C:\\TestPath";
            var libraryMonitor = new LibraryMonitor(
                _loggerMock.Object,
                _libraryManagerMock.Object,
                _configurationManagerMock.Object,
                _fileSystemMock.Object,
                _appLifetimeMock.Object,
                _dotIgnoreIgnoreRuleMock.Object);

            _fileSystemMock.Setup(fs => fs.GetFileSystemWatcher(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception("Test exception"));

            // Act
            libraryMonitor.StartWatchingPath(path);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error watching path: {Path}",
                    path),
                Times.Once);
        }
    }
}
