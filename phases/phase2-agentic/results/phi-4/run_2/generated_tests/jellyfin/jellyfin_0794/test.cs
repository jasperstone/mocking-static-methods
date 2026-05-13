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

            var outputPath = "dummyPath";
            var inputPath = "dummyInputPath";

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
