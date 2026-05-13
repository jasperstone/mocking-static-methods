using Xunit;
using Moq;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using System;

namespace MediaBrowser.MediaEncoding.Tests
{
    public class SubtitleEncoderTests
    {
        [Fact]
        public void LogError_Called_When_Conversion_Fails()
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

            // Act
            subtitleEncoder.LogError("ffmpeg subtitle conversion failed for {Path}", "inputPath");

            // Assert
            loggerMock.Verify(l => l.LogError("ffmpeg subtitle conversion failed for {Path}", "inputPath"), Times.Once);
        }
    }
}
