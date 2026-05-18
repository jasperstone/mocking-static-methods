using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace MediaBrowser.MediaEncoding.Subtitles
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public async Task LogError_Called_When_Subtitle_Conversion_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<MediaBrowser.Common.Net.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            // Act
            await Assert.ThrowsAsync<FfmpegException>(() => subtitleEncoder.GetSubtitles(
                new MediaBrowser.Model.Entities.BaseItem(),
                string.Empty,
                0,
                string.Empty,
                0,
                0,
                false,
                CancellationToken.None));

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }

        [Fact]
        public async Task LogError_Called_When_Deleting_Converted_Subtitle_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<SubtitleEncoder>>();
            var fileSystemMock = new Mock<MediaBrowser.Controller.IO.IFileSystem>();
            var mediaEncoderMock = new Mock<MediaBrowser.Controller.MediaEncoding.IMediaEncoder>();
            var httpClientFactoryMock = new Mock<MediaBrowser.Common.Net.IHttpClientFactory>();
            var mediaSourceManagerMock = new Mock<MediaBrowser.Controller.Library.IMediaSourceManager>();
            var subtitleParserMock = new Mock<MediaBrowser.MediaEncoding.Subtitles.ISubtitleParser>();
            var pathManagerMock = new Mock<MediaBrowser.Controller.IO.IPathManager>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();

            fileSystemMock.Setup(f => f.DeleteFile(It.IsAny<string>())).Throws(new IOException());

            var subtitleEncoder = new SubtitleEncoder(
                loggerMock.Object,
                fileSystemMock.Object,
                mediaEncoderMock.Object,
                httpClientFactoryMock.Object,
                mediaSourceManagerMock.Object,
                subtitleParserMock.Object,
                pathManagerMock.Object,
                serverConfigurationManagerMock.Object);

            // Act
            try
            {
                await subtitleEncoder.GetSubtitles(
                    new MediaBrowser.Model.Entities.BaseItem(),
                    string.Empty,
                    0,
                    string.Empty,
                    0,
                    0,
                    false,
                    CancellationToken.None);
            }
            catch (FfmpegException)
            {
            }

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()), Times.Once);
        }
    }
}
