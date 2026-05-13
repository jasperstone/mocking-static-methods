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
        public async Task RefreshTrickplayDataAsync_LogsInformation_WhenTrickplayInfoIsSaved()
        {
            // Arrange
            var video = new Video { Id = Guid.NewGuid(), Name = "Test Video" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var trickplayOptions = new TrickplayOptions { Interval = 1000, WidthResolutions = new List<int> { 1920 } };
            var trickplayInfo = new TrickplayInfo { ItemId = video.Id };
            var dbContextMock = new Mock<JellyfinDbContext>();
            var dbSetMock = new Mock<DbSet<TrickplayInfo>>();
            var cancellationToken = new CancellationToken();

            _configMock.Setup(c => c.Configuration.TrickplayOptions).Returns(trickplayOptions);
            _dbProviderMock.Setup(d => d.CreateDbContextAsync(cancellationToken)).ReturnsAsync(dbContextMock.Object);
            _pathManagerMock.Setup(p => p.GetTrickplayDirectory(video, true)).Returns("test/trickplay/directory");
            _fileSystemMock.Setup(f => f.GetFiles("test/trickplay/directory", new[] { ".jpg" }, false, false))
                .Returns(new List<FileSystemMetadata> { new FileInfo("test/trickplay/directory/image.jpg") }.Select(f => new FileSystemMetadata { FullName = f.FullName }).ToList());

            dbContextMock.Setup(d => d.TrickplayInfos).Returns(dbSetMock.Object);
            dbSetMock.Setup(d => d.Where(It.IsAny<Func<TrickplayInfo, bool>>())).Returns(dbSetMock.Object);
            dbSetMock.Setup(d => d.ExecuteDeleteAsync(cancellationToken)).ReturnsAsync(1);

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, true, libraryOptions, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
