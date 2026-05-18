using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Common.Net;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;

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

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                Mock.Of<IMediaEncoder>(),
                Mock.Of<IHttpClientFactory>(),
                Mock.Of<IMediaSourceManager>(),
                Mock.Of<ISubtitleParser>(),
                Mock.Of<IPathManager>(),
                Mock.Of<IServerConfigurationManager>());

            var inputPath = "inputPath";
            var outputPath = "outputPath";

            // Simulate failure condition
            fileSystemMock.Setup(fs => fs.FileExists(outputPath)).Returns(true);
            fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath).Length).Returns(0);

            // Act
            await Assert.ThrowsAsync<FfmpegException>(async () =>
            {
                await subtitleEncoder.GetSubtitles(
                    Mock.Of<BaseItem>(),
                    "mediaSourceId",
                    0,
                    "srt",
                    0,
                    0,
                    false,
                    CancellationToken.None);
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
