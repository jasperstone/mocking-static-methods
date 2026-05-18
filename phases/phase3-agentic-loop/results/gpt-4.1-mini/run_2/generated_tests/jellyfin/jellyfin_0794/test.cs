using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.MediaEncoding.Subtitles;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task GetSubtitles_LogsErrorAndThrows_WhenConversionFails()
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

            var baseItem = new Mock<BaseItem>().Object;
            var mediaSourceId = "mediaSourceId";
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

            mediaSourceManagerMock
                .Setup(m => m.GetPlaybackMediaSources(baseItem, null, true, false, cancellationToken))
                .ReturnsAsync(new[] { mediaSource });

            // Setup GetReadableFile to return a SubtitleInfo with a path and format
            var subtitleInfo = new SubtitleInfo
            {
                Path = "fakePath",
                Format = "srt",
                Protocol = MediaProtocol.File
            };

            // We need to mock GetReadableFile to return subtitleInfo
            // But GetReadableFile is private, so we cannot mock it directly.
            // Instead, we will mock GetSubtitleStream(SubtitleInfo, CancellationToken) to throw to simulate failure.

            // We will create a derived class to override GetReadableFile and GetSubtitleStream to simulate failure and trigger the error log.

            var testEncoder = new TestSubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigManagerMock.Object,
                subtitleInfo);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FfmpegException>(async () =>
                await ((ISubtitleEncoder)testEncoder).GetSubtitles(baseItem, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            Assert.Contains("ffmpeg subtitle conversion failed for", ex.Message);

            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("ffmpeg subtitle conversion failed for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class TestSubtitleEncoder : SubtitleEncoder
        {
            private readonly SubtitleInfo _subtitleInfo;

            public TestSubtitleEncoder(
                ILogger<SubtitleEncoder> logger,
                IFileSystem fileSystem,
                IMediaEncoder mediaEncoder,
                IHttpClientFactory httpClientFactory,
                IMediaSourceManager mediaSourceManager,
                ISubtitleParser subtitleParser,
                IPathManager pathManager,
                IServerConfigurationManager serverConfigurationManager,
                SubtitleInfo subtitleInfo)
                : base(logger, fileSystem, mediaEncoder, httpClientFactory, mediaSourceManager, subtitleParser, pathManager, serverConfigurationManager)
            {
                _subtitleInfo = subtitleInfo;
            }

            // Override GetReadableFile to return the provided SubtitleInfo
            protected override Task<SubtitleInfo> GetReadableFile(MediaSourceInfo mediaSource, MediaStream subtitleStream, CancellationToken cancellationToken)
            {
                return Task.FromResult(_subtitleInfo);
            }

            // Override GetSubtitleStream to simulate a failure that causes the conversion to fail and throw
            protected override Task<Stream> GetSubtitleStream(SubtitleInfo fileInfo, CancellationToken cancellationToken)
            {
                // Return a stream that will cause the subtitleParser.Parse to throw
                // We override ConvertSubtitles to throw to simulate failure
                return Task.FromResult<Stream>(new MemoryStream(new byte[] { 0x00 }));
            }

            // Override ConvertSubtitles to throw to simulate failure and trigger the error log
            protected override MemoryStream ConvertSubtitles(Stream stream, string inputFormat, string outputFormat, long startTimeTicks, long endTimeTicks, bool preserveOriginalTimestamps, CancellationToken cancellationToken)
            {
                throw new Exception("Simulated conversion failure");
            }
        }
    }
}
