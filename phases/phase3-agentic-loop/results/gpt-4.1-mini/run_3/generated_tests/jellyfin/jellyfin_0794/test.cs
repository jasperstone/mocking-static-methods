using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task GetSubtitles_LogsErrorOnConversionFailureAndFileDeletionIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<System.Net.Http.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigMock.Object);

            var baseItem = new BaseItem { Id = "item1" };
            var mediaSourceId = "source1";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            var mediaStream = new MediaStream
            {
                Type = MediaStreamType.Subtitle,
                Index = subtitleStreamIndex,
                Codec = "srt"
            };

            var mediaSource = new MediaSourceInfo
            {
                Id = mediaSourceId,
                MediaStreams = new[] { mediaStream }
            };

            mediaSourceManagerMock.Setup(m => m.GetPlaybackMediaSources(baseItem, null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSource });

            // Setup GetReadableFile and GetSubtitleStream indirectly by mocking subtitleParser.Parse to throw to simulate conversion failure
            subtitleParserMock.Setup(p => p.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new Exception("Parse failed"));

            // Setup fileSystem.GetFileInfo to return a file with length 0 to trigger the failure branch that deletes the file
            var fileInfoMock = new Mock<IFileInfo>();
            fileInfoMock.Setup(f => f.Length).Returns(0);
            fileSystemMock.Setup(f => f.GetFileInfo(It.IsAny<string>())).Returns(fileInfoMock.Object);

            // Setup fileSystem.DeleteFile to throw IOException to trigger LogError call on deletion failure
            fileSystemMock.Setup(f => f.DeleteFile(It.IsAny<string>())).Throws(new IOException("Delete failed"));

            // Setup mediaSourceManager to return mediaSource with MediaStreams
            // Setup mediaSource.MediaStreams to contain the subtitleStream

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FfmpegException>(async () =>
            {
                await ((ISubtitleEncoder)subtitleEncoder).GetSubtitles(
                    baseItem,
                    mediaSourceId,
                    subtitleStreamIndex,
                    outputFormat,
                    startTimeTicks,
                    endTimeTicks,
                    preserveOriginalTimestamps,
                    cancellationToken);
            });

            Assert.Contains("ffmpeg subtitle conversion failed for", ex.Message);

            // Verify that LogError was called with IOException about deleting converted subtitle
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting converted subtitle")),
                It.IsAny<IOException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            // Verify that LogError was called with the final ffmpeg subtitle conversion failed message
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed for")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
