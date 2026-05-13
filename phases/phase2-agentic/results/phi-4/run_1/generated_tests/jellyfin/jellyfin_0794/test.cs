using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task FfmpegSubtitleConversion_Failure_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = Mock.Of<IMediaEncoder>();
            var httpClientFactoryMock = Mock.Of<IHttpClientFactory>();
            var mediaSourceManagerMock = Mock.Of<IMediaSourceManager>();
            var subtitleParserMock = Mock.Of<ISubtitleParser>();
            var pathManagerMock = Mock.Of<IPathManager>();
            var serverConfigurationManagerMock = Mock.Of<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock,
                httpClientFactoryMock,
                mediaSourceManagerMock,
                subtitleParserMock,
                pathManagerMock,
                serverConfigurationManagerMock);

            var outputPath = "outputPath";
            var inputPath = "inputPath";

            fileSystemMock.Setup(fs => fs.FileExists(outputPath)).Returns(false);
            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfo { Length = 0 });

            // Act
            await Assert.ThrowsAsync<FfmpegException>(async () =>
            {
                await subtitleEncoder.ConvertSubtitlesAsync(outputPath, inputPath, CancellationToken.None);
            });

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    "ffmpeg subtitle conversion failed for {Path}",
                    inputPath),
                Times.Once);
        }
    }
}
