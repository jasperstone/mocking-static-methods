using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Model.MediaInfo;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task LogError_WhenSubtitleConversionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            var mediaSource = new MediaSourceInfo
            {
                MediaStreams = new[]
                {
                    new MediaStream
                    {
                        Type = MediaStreamType.Subtitle,
                        Index = 0,
                        Codec = "srt"
                    }
                }
            };

            var cancellationToken = CancellationToken.None;

            // Simulate failure by not creating the output file
            fileSystemMock.Setup(fs => fs.FileExists(It.IsAny<string>())).Returns(false);

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => subtitleEncoder.GetSubtitles(
                new BaseItem(), "mediaSourceId", 0, "srt", 0, 0, false, cancellationToken));

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "ffmpeg subtitle conversion failed for {Path}",
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
