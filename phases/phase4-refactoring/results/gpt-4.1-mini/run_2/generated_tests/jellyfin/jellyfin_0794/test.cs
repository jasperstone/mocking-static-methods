using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    // Minimal concrete subclass of BaseItem for testing
    public class TestBaseItem : BaseItem
    {
    }

    // Minimal stub classes for MediaStream and MediaSourceInfo
    public class TestMediaStream
    {
        public string Codec { get; set; }
        public int Index { get; set; }
        public MediaStreamType Type { get; set; }
    }

    public class TestMediaSourceInfo
    {
        public string Id { get; set; }
        public TestMediaStream[] MediaStreams { get; set; }
    }

    // Enum stub for MediaStreamType
    public enum MediaStreamType
    {
        Subtitle
    }

    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task GetSubtitles_LogsErrorAndThrowsFfmpegException_WhenConversionFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var pathManagerMock = new Mock<IPathManager>();
            var serverConfigManagerMock = new Mock<IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigManagerMock.Object);

            var baseItem = new TestBaseItem();
            var mediaSourceId = "mediaSourceId";
            var subtitleStreamIndex = 0;
            var outputFormat = "srt";
            var startTimeTicks = 0L;
            var endTimeTicks = 0L;
            var preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            var mediaStream = new TestMediaStream
            {
                Type = MediaStreamType.Subtitle,
                Index = subtitleStreamIndex,
                Codec = "srt"
            };

            var mediaSource = new TestMediaSourceInfo
            {
                Id = mediaSourceId,
                MediaStreams = new[] { mediaStream }
            };

            mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(baseItem, null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSource });

            // Setup GetSubtitleStream to throw to simulate failure and trigger the error log
            // We simulate failure by throwing from GetSubtitleStream method indirectly by mocking subtitleParser.Parse to throw
            subtitleParserMock
                .Setup(p => p.Parse(It.IsAny<Stream>(), It.IsAny<string>()))
                .Throws(new Exception("Parse failure"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(async () =>
                await ((ISubtitleEncoder)subtitleEncoder).GetSubtitles(
                    baseItem,
                    mediaSourceId,
                    subtitleStreamIndex,
                    outputFormat,
                    startTimeTicks,
                    endTimeTicks,
                    preserveOriginalTimestamps,
                    cancellationToken));

            // Verify that LogError was called with the expected message containing "ffmpeg subtitle conversion failed"
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.AtLeastOnce);
        }
    }
}
