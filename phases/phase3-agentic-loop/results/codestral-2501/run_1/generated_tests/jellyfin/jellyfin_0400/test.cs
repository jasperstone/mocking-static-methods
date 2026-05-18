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
using MediaBrowser.Controller.Drawing;
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
        public async Task CreateTrickplayFiles_ShouldLogInformation_WhenFilesAreCreatedSuccessfully()
        {
            // Arrange
            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000, WidthResolutions = new int[] { 1920 } };
            var trickplayResolutions = new Dictionary<string, TrickplayResolution>
            {
                { "1920x1080", new TrickplayResolution { TileWidth = 1920, TileHeight = 1080 } }
            };

            _configMock.Setup(c => c.Configuration).Returns(new ServerConfiguration { TrickplayOptions = trickplayOptions });
            _fileSystemMock.Setup(fs => fs.MoveDirectory(It.IsAny<string>(), It.IsAny<string>())).Verifiable();
            _pathManagerMock.Setup(pm => pm.GetTrickplayDirectory(video, true)).Returns("TestPath");

            // Act
            await _trickplayManager.MoveGeneratedTrickplayDataAsync(video, libraryOptions, CancellationToken.None);

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Moved trickplay images for {ItemName} to {Location}", video.Name, It.IsAny<string>()),
                Times.Once);
        }
    }
}
