using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Emby.Server.Implementations.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using Microsoft.Extensions.Hosting;
using System.IO;
using System;
using Emby.Server.Implementations.Library;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities;

namespace Emby.Server.Tests.IO
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
        public void Start_ShouldLogError_WhenExceptionIsThrown()
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

            var rootFolderMock = new Mock<Folder>();
            var folderMock = new Mock<Folder>();
            folderMock.Setup(f => f.PhysicalLocations).Returns(new List<string> { path });
            rootFolderMock.Setup(rf => rf.Children).Returns(new List<BaseItem> { folderMock.Object });

            _libraryManagerMock.Setup(lm => lm.RootFolder).Returns(rootFolderMock.Object);
            _fileSystemMock.Setup(fs => fs.DirectoryExists(It.IsAny<string>())).Throws(new UnauthorizedAccessException());

            // Act
            libraryMonitor.Start();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "Error watching path: {Path}",
                    It.IsAny<object[]>()
                ),
                Times.Once);
        }
    }
}
