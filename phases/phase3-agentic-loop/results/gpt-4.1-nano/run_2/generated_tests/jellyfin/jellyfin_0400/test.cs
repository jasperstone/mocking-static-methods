using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.IO;

namespace Jellyfin.Tests
{
    public class TrickplayManagerTests
    {
        private readonly Mock<ILogger<TrickplayManager>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IServerConfigurationManager> _configMock;
        private readonly Mock<IImageEncoder> _imageEncoderMock;
        private readonly Mock<IDbContextFactory<JellyfinDbContext>> _dbFactoryMock;
        private readonly Mock<IApplicationPaths> _appPathsMock;
        private readonly Mock<IPathManager> _pathManagerMock;

        public TrickplayManagerTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayManager>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _fileSystemMock = new Mock<IFileSystem>();
            _configMock = new Mock<IServerConfigurationManager>();
            _imageEncoderMock = new Mock<IImageEncoder>();
            _dbFactoryMock = new Mock<IDbContextFactory<JellyfinDbContext>>();
            _appPathsMock = new Mock<IApplicationPaths>();
            _pathManagerMock = new Mock<IPathManager>();
        }

        [Fact]
        public async Task LogInformation_Called_OnSuccessfulCreateTiles()
        {
            // Arrange
            var trickplayOptions = new TrickplayOptions
            {
                WidthResolutions = new List<int> { 100 },
                Interval = 2000
            };
            var config = new ServerConfiguration { TrickplayOptions = trickplayOptions };
            _configMock.Setup(c => c.Configuration).Returns(config);

            var manager = new TrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _fileSystemMock.Object,
                new EncodingHelper(),
                _configMock.Object,
                _imageEncoderMock.Object,
                _dbFactoryMock.Object,
                _appPathsMock.Object,
                _pathManagerMock.Object);

            var video = new Video { Id = Guid.NewGuid(), Name = "TestVideo" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var cancellationToken = CancellationToken.None;

            // Mock dependencies
            var dummyDir = new DirectoryInfo("dummy");
            _pathManagerMock.Setup(p => p.GetTrickplayDirectory(video, true)).Returns("dummy");
            _fileSystemMock.Setup(fs => fs.MoveDirectory(It.IsAny<string>(), It.IsAny<string>()));
            // Simulate CreateTiles returning non-null trickplayInfo
            var dummyTrickplayInfo = new TrickplayInfo { ItemId = Guid.Empty };
            var mockCreateTilesMethod = new Moq.Mock<TrickplayManager> { CallBase = true };
            mockCreateTilesMethod.Setup(m => m.CreateTiles(It.IsAny<List<string>>(), It.IsAny<int>(), It.IsAny<LibraryOptions>(), It.IsAny<string>()))
                .Returns(dummyTrickplayInfo);

            // Act
            await manager.RefreshTrickplayDataAsync(video, false, libraryOptions, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
