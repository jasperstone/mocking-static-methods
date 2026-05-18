using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
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
        private readonly SubtitleEncoder _subtitleEncoder;

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

            _subtitleEncoder = new SubtitleEncoder(
                _loggerMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                _httpClientFactoryMock.Object,
                _mediaSourceManagerMock.Object,
                _subtitleParserMock.Object,
                _pathManagerMock.Object,
                _serverConfigurationManagerMock.Object);
        }

        [Fact]
        public async Task ConvertSubtitleWithFfmpeg_Fails_LogsErrorMessage()
        {
            // Arrange
            var inputPath = "/path/to/input.srt";
            var outputPath = "/path/to/output.ass";
            var mediaSource = new MediaSourceInfo
            {
                Path = inputPath,
                MediaStreams = new List<MediaStream>
                {
                    new MediaStream { Index = 0, Type = MediaStreamType.Subtitle, Codec = "srt" }
                }
            };
            var cancellationToken = new CancellationToken();

            _mediaEncoderMock.Setup(m => m.EncodeVideo(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<VideoProcessingArguments>(),
                It.IsAny<EncodingJobInfo>(),
                It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("ffmpeg failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(
                () => _subtitleEncoder.ConvertSubtitleWithFfmpeg(mediaSource, 0, "ass", outputPath, null, null, false, cancellationToken));

            // Assert - Verify LogError was called with the specific message on line 457
            _loggerMock.Verify(
                x => x.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath),
                Times.Once);
        }

        [Fact]
        public async Task ConvertSubtitleWithFfmpeg_EmptyOutputFile_LogsErrorOnDeleteFailure()
        {
            // Arrange
            var inputPath = "/path/to/input.srt";
            var outputPath = "/path/to/output.ass";
            var mediaSource = new MediaSourceInfo
            {
                Path = inputPath,
                MediaStreams = new List<MediaStream> { new MediaStream { Index = 0, Type = MediaStreamType.Subtitle, Codec = "srt" } }
            };
            var cancellationToken = new CancellationToken();

            _mediaEncoderMock.Setup(m => m.EncodeVideo(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<VideoProcessingArguments>(),
                It.IsAny<EncodingJobInfo>(),
                It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _fileSystemMock.Setup(f => f.FileExists(outputPath)).Returns(true);
            var mockFileInfo = new Mock<FileSystemMetadata>();
            mockFileInfo.Setup(f => f.Length).Returns(0L);
            _fileSystemMock.Setup(f => f.GetFileInfo(outputPath)).Returns(mockFileInfo.Object);
            _fileSystemMock.Setup(f => f.DeleteFile(outputPath))
                .Throws(new IOException("Delete failed"));

            // Act
            await Assert.ThrowsAsync<Exception>(
                () => _subtitleEncoder.ConvertSubtitleWithFfmpeg(mediaSource, 0, "ass", outputPath, null, null, false, cancellationToken));

            // Assert - Verify both LogError calls
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<IOException>(), "Error deleting converted subtitle {Path}", outputPath),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogError("ffmpeg subtitle conversion failed for {Path}", inputPath),
                Times.Once);
        }
    }
}
