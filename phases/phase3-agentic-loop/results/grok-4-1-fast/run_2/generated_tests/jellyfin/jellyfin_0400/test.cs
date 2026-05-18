using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Trickplay;
using MediaBrowser.Model.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.Server.Implementations.Trickplay.Tests
{
    public class TrickplayManagerTests
    {
        private readonly Mock<ILogger<TrickplayManager>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<object> _fileSystemMock;
        private readonly Mock<object> _encodingHelperMock;
        private readonly Mock<object> _configMock;
        private readonly Mock<object> _imageEncoderMock;
        private readonly Mock<object> _dbProviderMock;
        private readonly Mock<object> _appPathsMock;
        private readonly Mock<object> _pathManagerMock;
        private readonly TrickplayManager _trickplayManager;

        public TrickplayManagerTests()
        {
            _loggerMock = new Mock<ILogger<TrickplayManager>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _fileSystemMock = new Mock<object>();
            _encodingHelperMock = new Mock<object>();
            _configMock = new Mock<object>();
            _imageEncoderMock = new Mock<object>();
            _dbProviderMock = new Mock<object>();
            _appPathsMock = new Mock<object>();
            _pathManagerMock = new Mock<object>();

            _trickplayManager = new TrickplayManager(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                (_fileSystemMock as object) as IFileSystem,
                (_encodingHelperMock as object) as EncodingHelper,
                (_configMock as object) as IServerConfigurationManager,
                (_imageEncoderMock as object) as IImageEncoder,
                (_dbProviderMock as object) as IDbContextFactory<object>,
                (_appPathsMock as object) as IApplicationPaths,
                (_pathManagerMock as object) as IPathManager);
        }

        [Fact]
        public void Constructor_InitializesSuccessfully()
        {
            Assert.NotNull(_trickplayManager);
        }

        [Fact]
        public async Task RefreshTrickplayDataAsync_SuccessfulFlow_LogsInformationMessage()
        {
            // Arrange
            var video = new Video 
            { 
                Id = "test-id",
                Path = "/path/to/video.mp4",
                VideoType = VideoType.VideoFile,
                RunTimeTicks = TimeSpan.FromMinutes(5).Ticks
            };
            var libraryOptions = new LibraryOptions 
            { 
                EnableTrickplayImageExtraction = true 
            };
            var cancellationToken = new CancellationToken();

            // Setup minimal mocks to avoid early returns
            _configMock.Setup(c => c.Configuration).Returns(new { TrickplayOptions = new { Interval = 10000, WidthResolutions = new[] { 320 } } });
            _mediaEncoderMock.Setup(m => m.ExtractVideoImages(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<object>(), cancellationToken))
                .ReturnsAsync("/valid/temp/dir");
            _fileSystemMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new[] { new FileInfo("image.jpg") });
            _dbProviderMock.Setup(p => p.CreateDbContextAsync(cancellationToken))
                .ReturnsAsync((object)null);

            // Act
            await _trickplayManager.RefreshTrickplayDataAsync(video, replace: true, libraryOptions, cancellationToken);

            // Assert - Verify LogInformation call from line 361 was executed
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Finished creation of trickplay files for {0}",
                    It.Is<string>(p => p.Contains("video.mp4"))),
                Times.Once);
        }
    }
}
