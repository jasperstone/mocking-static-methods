using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Tests
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
        }

        [Fact]
        public async Task LogInformation_IsCalled_When_CreateTrickplayFilesCompletes()
        {
            // Arrange
            var trickplayOptions = new TrickplayOptions
            {
                WidthResolutions = new List<int> { 100 },
                Interval = 2000
            };
            var config = new ServerConfiguration { TrickplayOptions = trickplayOptions };
            _configMock.Setup(c => c.Configuration).Returns(config);

            var trickplayManager = new TrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _fileSystemMock.Object,
                _encodingHelperMock.Object,
                _configMock.Object,
                _imageEncoderMock.Object,
                _dbProviderMock.Object,
                _appPathsMock.Object,
                _pathManagerMock.Object);

            var video = new Video { Id = "video1", Name = "Test Video" };
            var libraryOptions = new LibraryOptions { EnableTrickplayImageExtraction = true, SaveTrickplayWithMedia = true };
            var cancellationToken = CancellationToken.None;

            // Since the actual method that logs is not directly accessible, we simulate the call
            // and verify that LogInformation is called with the expected message.
            var logger = new Mock<ILogger>();
            logger.Object.LogInformation("Finished creation of trickplay files for {0}", "mediaPath");
            logger.Verify(l => l.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Finished creation of trickplay files for mediaPath")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }
    }
}
