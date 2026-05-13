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

            var subtitleStream = new MediaStream
            {
                Codec = "ass",
                Index = 0
            };

            var mediaSource = new MediaSourceInfo
            {
                MediaStreams = new[] { subtitleStream }
            };

            var outputPath = "outputPath";
            var inputPath = "inputPath";

            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath))
                .Returns(new FileInfo
                {
                    Exists = false,
                    Length = 0
                });

            fileSystemMock.Setup(fs => fs.DeleteFile(outputPath))
                .Throws(new IOException("Simulated IO exception"));

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<ISubtitleParser>(),
                Mock.Of<IPathManager>(),
                Mock.Of<IServerConfigurationManager>());

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() =>
                subtitleEncoder.GetSubtitles(
                    null, // BaseItem
                    mediaSource.Id,
                    subtitleStream.Index,
                    "srt",
                    0,
                    0,
                    false,
                    CancellationToken.None));

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
