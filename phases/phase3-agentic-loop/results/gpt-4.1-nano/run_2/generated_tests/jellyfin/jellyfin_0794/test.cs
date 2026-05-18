using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests.MediaEncoding.Subtitles
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task GetSubtitles_Should_LogError_When_DeleteFileThrowsIOException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<IFileSystem>();
            var mediaSourceManagerMock = new Mock<IMediaSourceManager>();
            var subtitleParserMock = new Mock<ISubtitleParser>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
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

            var item = new Mock<MediaBrowser.Controller.Entities.BaseItem>().Object;
            var mediaSourceId = "sourceId";
            int subtitleStreamIndex = 0;
            string outputFormat = "srt";
            long startTimeTicks = 0;
            long endTimeTicks = 0;
            bool preserveOriginalTimestamps = false;
            var cancellationToken = CancellationToken.None;

            // Setup media source with subtitle stream
            var mediaSource = new MediaSourceInfo
            {
                Id = mediaSourceId,
                MediaStreams = new[]
                {
                    new MediaStream { Type = MediaStreamType.Subtitle, Index = subtitleStreamIndex }
                }
            };
            mediaSourceManagerMock.Setup(m => m.GetPlaybackMediaSources(It.IsAny<MediaBrowser.Controller.Entities.BaseItem>(), null, true, false, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { mediaSource });

            // Setup GetReadableFile to return a dummy SubtitleInfo
            var subtitleInfo = new SubtitleInfo
            {
                Path = "http://example.com/subtitle",
                Protocol = MediaProtocol.Http,
                Format = "srt"
            };
            // We need to mock GetReadableFile method, but it's private.
            // Instead, we can simulate the scenario by mocking the _fileSystem.DeleteFile to throw IOException
            // Since _fileSystem is a dependency, we can set it up to throw
            fileSystemMock.Setup(f => f.DeleteFile(It.IsAny<string>())).Throws(new IOException("Delete failed"));

            // Act & Assert
            await Assert.ThrowsAsync<IOException>(async () =>
            {
                await subtitleEncoder.GetSubtitles(
                    item,
                    mediaSourceId,
                    subtitleStreamIndex,
                    outputFormat,
                    startTimeTicks,
                    endTimeTicks,
                    preserveOriginalTimestamps,
                    cancellationToken);
            });

            // Verify that LogError was called with the expected message
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error deleting converted subtitle")),
                    It.IsAny<IOException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
