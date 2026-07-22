using Xunit;
using Moq;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.MediaInfo;
using System.IO;
using System;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task ConvertSubtitles_Failure_LogsError()
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

            var inputPath = "inputPath";
            var outputPath = "outputPath";
            var cancellationToken = CancellationToken.None;

            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfo(outputPath));
            fileSystemMock.Setup(fs => fs.DeleteFile(outputPath)).Throws(new IOException());

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => subtitleEncoder.ConvertSubtitles(inputPath, outputPath, cancellationToken));

            // Assert
            loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(s => s.Contains("Error deleting converted subtitle")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );

            loggerMock.Verify(
                logger => logger.LogError(
                    It.Is<string>(s => s.Contains("ffmpeg subtitle conversion failed for")),
                    It.IsAny<object[]>()
                ),
                Times.Once
            );
        }
    }
}
