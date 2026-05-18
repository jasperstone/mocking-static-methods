using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Subtitles;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class SubtitleEncoderTests
{
    [Fact]
    public async Task ConvertSubtitles_ShouldLogError_WhenFfmpegConversionFails()
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

        fileSystemMock.Setup(fs => fs.GetFileInfo(outputPath)).Returns(new FileInfoWrapper { Length = 0 });

        // Act
        await Assert.ThrowsAsync<FfmpegException>(() => subtitleEncoder.ConvertSubtitles(
            new MemoryStream(),
            "inputFormat",
            "outputFormat",
            0,
            0,
            false,
            CancellationToken.None));

        // Assert
        loggerMock.Verify(
            logger => logger.LogError(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()),
            Times.Once);
    }
}
