using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Database.Implementations;
using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        private readonly Mock<ILogger<TrickplayManager>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<EncodingHelper> _encodingHelperMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbProviderMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly TrickplayManager _trickplayManager;

        public TrickplayManagerTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayManager>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _fileSystemMock = new Mock<IFileSystem>();
            _encodingHelperMock = new Mock<EncodingHelper>();
            _configMock = new Mock<IServerConfigurationManager>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _dbProviderMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appPathsMock = new Mock<IApplicationPaths>();
            _pathManagerMock = new Mock<IPathManager>();

            _trickplayManager = new TrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _fileSystemMock.Object,
                _encodingHelperMock.Object,
                _configMock.Object,
                _imageEncoderMock.Object,
                _dbProviderMock.Object,
                _appPathsMock.Object,
                _pathManagerMock.Object);
        }

        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayInfoIsNotNull()
        {
            // Arrange
            var video = new Video();
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000 };
            var trickplayInfo = new TrickplayInfo();
            var cancellationToken = new CancellationToken();

            _configMock.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);
            _pathManagerMock.Setup(p => p.GetTrickplayDirectory(video, It.IsAny<bool>())).Returns("outputDir");
            _fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata> { new FileInfo("image.jpg") }.Select(f => new FileSystemMetadata { FullName = f.FullName }).ToList());
            _mediaEncoderMock.Setup(m => m.EncodeVideoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EncodingOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("imgTempDir");
            _trickplayManager.CreateTiles(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<TrickplayOptions>(), It.IsAny<string>())
                .Returns(trickplayInfo);

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogInformation("Finished creation of trickplay files for {0}", It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task RefreshTrickplayDataAsync_LogsError_WhenTrickplayInfoIsNull()
        {
            // Arrange
            var video = new Video();
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000 };
            var cancellationToken = new CancellationToken();

            _configMock.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);
            _pathManagerMock.Setup(p => p.GetTrickplayDirectory(video, It.IsAny<bool>())).Returns("outputDir");
            _fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new List<FileSystemMetadata> { new FileInfo("image.jpg") }.Select(f => new FileSystemMetadata { FullName = f.FullName }).ToList());
            _mediaEncoderMock.Setup(m => m.EncodeVideoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<EncodingOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync("imgTempDir");
            _trickplayManager.CreateTiles(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<TrickplayOptions>(), It.IsAny<string>())
                .Returns((TrickplayInfo)null);

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error while saving trickplay tiles info."), Times.Once);
        }
    }
}
