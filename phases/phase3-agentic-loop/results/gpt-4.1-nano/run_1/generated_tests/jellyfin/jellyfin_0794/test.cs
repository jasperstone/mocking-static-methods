using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Subtitles;

namespace MediaBrowser.Tests.MediaEncoding.Subtitles
{
    public class SubtitleEncoderTests
    {
        private readonly Mock<ILogger<SubtitleEncoder>> _loggerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IMediaSourceManager> _mediaSourceManagerMock;
        private readonly Mock<ISubtitleParser> _subtitleParserMock;
        private readonly Mock<IPathManager> _pathManagerMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;

        public SubtitleEncoderTests()
        {
            _loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            _fileSystemMock = new Mock<IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            _subtitleParserMock = new Mock<ISubtitleParser>();
            _pathManagerMock = new Mock<IPathManager>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task GetSubtitles_Should_LogError_When_DeleteFileThrowsIOException()
        {
            // Arrange
            var encoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);

            var item = new Mock<BaseItem>().Object;
            var mediaSourceId = "sourceId";
            int subtitleStreamIndex = 0;
            string outputFormat = "srt";
            long startTimeTicks = 0;
            long endTimeTicks = 0;
            bool preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            var mediaSource = new MediaSourceInfo { Id = mediaSourceId, MediaStreams = new[] { new MediaStream { Type = MediaStreamType.Subtitle, Index = subtitleStreamIndex } } };
            _mediaSourceManagerMock.Setup(m => m.GetPlaybackMediaSources(It.IsAny<BaseItem>(), null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSource });
            _fileSystemMock.Setup(fs => fs.GetFileInfo(It.IsAny<string>())).Returns(new FileInfo { Length = 1 });
            _fileSystemMock.Setup(fs => fs.DeleteFile(It.IsAny<string>())).Throws(new IOException("delete error"));

            // Act
            await Assert.ThrowsAsync<IOException>(async () =>
            {
                await encoder.GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken);
            });

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<IOException>(), "Error deleting converted subtitle {Path}", It.IsAny<string>()),
                Times.Once);
        }
    }
}
