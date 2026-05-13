using System;
using System.IO;
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
        public async Task GetSubtitles_LogsErrorAndThrowsFfmpegException_WhenConversionFailsAndFileDeletionFailsWithIOException()
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

            // Setup GetReadableFile to return a SubtitleInfo with a path
            var subtitleInfo = new SubtitleInfo
            {
                Path = "path/to/subtitle.srt",
                Format = "srt",
                Protocol = MediaProtocol.File
            };

            // We need to mock private method GetReadableFile and GetSubtitleStream, but since they are private,
            // we simulate the behavior by mocking GetSubtitleStream to return a stream and format.
            // We will override GetSubtitleStream by subclassing for test.

            var testSubtitleEncoder = new TestSubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigManagerMock.Object,
                subtitleInfo);

            // Setup file system to simulate file does not exist or length 0 to trigger failure
            fileSystemMock.Setup(f => f.GetFileInfo(subtitleInfo.Path))
                .Returns(new FileInfoStub(0)); // length 0 triggers failure

            fileSystemMock.Setup(f => f.DeleteFile(subtitleInfo.Path))
                .Throws(new IOException("Delete failed"));

            // Act & Assert
            var ex = await Assert.ThrowsAsync<FfmpegException>(() =>
                testSubtitleEncoder.GetSubtitles(baseItem, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken));

            Assert.Contains("ffmpeg subtitle conversion failed for", ex.Message);

            // Verify LogError was called with the expected message and exception
            loggerMock.Verify(l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting converted subtitle")),
                It.IsAny<IOException>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);

            loggerMock.Verify(l => l.Log(
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

            // Override GetSubtitleStream to return a stream and format from the provided SubtitleInfo
            private new async Task<(Stream Stream, string Format)> GetSubtitleStream(MediaSourceInfo mediaSource, MediaStream subtitleStream, CancellationToken cancellationToken)
            {
                return (new MemoryStream(new byte[] { 1, 2, 3 }), _subtitleInfo.Format);
            }

            // Override GetReadableFile to return the provided SubtitleInfo
            private new async Task<SubtitleInfo> GetReadableFile(MediaSourceInfo mediaSource, MediaStream subtitleStream, CancellationToken cancellationToken)
            {
                return _subtitleInfo;
            }

            // Expose the interface method for testing
            public new Task<Stream> GetSubtitles(BaseItem item, string mediaSourceId, int subtitleStreamIndex, string outputFormat, long startTimeTicks, long endTimeTicks, bool preserveOriginalTimestamps, CancellationToken cancellationToken)
            {
                return ((ISubtitleEncoder)this).GetSubtitles(item, mediaSourceId, subtitleStreamIndex, outputFormat, startTimeTicks, endTimeTicks, preserveOriginalTimestamps, cancellationToken);
            }
        }

        private class FileInfoStub : IFileInfo
        {
            public FileInfoStub(long length)
            {
                Length = length;
            }

            public bool Exists => true;
            public long Length { get; }
            public string Name => "stub";
            public string FullName => "stub";
            public DateTime LastWriteTimeUtc => DateTime.UtcNow;
        }
    }
}
