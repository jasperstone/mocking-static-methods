using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;
using MediaBrowser.Controller.MediaEncoding;
using System.Threading;
using System.Threading.Tasks;
using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Globalization;
using MediaBrowser.MediaEncoding.Probing;
using Microsoft.Extensions.Configuration;
using MediaBrowser.Model.Entities;
using System.Diagnostics;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Configuration;

public class MediaEncoderTests
{
    [Fact]
    public async Task ExtractVideoImagesOnInterval_LogsWarning_WhenIFrameTrickplayExtractionFails()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<MediaEncoder>>();
        var configurationManagerMock = new Mock<IServerConfigurationManager>();
        var fileSystemMock = new Mock<IFileSystem>();
        var blurayExaminerMock = new Mock<IBlurayExaminer>();
        var localizationMock = new Mock<ILocalizationManager>();
        var configMock = new Mock<IConfiguration>();
        var serverConfigMock = new Mock<IServerConfigurationManager>();

        var mediaEncoder = new MediaEncoder(
            loggerMock.Object,
            configurationManagerMock.Object,
            fileSystemMock.Object,
            blurayExaminerMock.Object,
            localizationMock.Object,
            configMock.Object,
            serverConfigMock.Object);

        var jobState = new JobState();
        var options = new EncodingOptions
        {
            HardwareAccelerationType = HardwareAccelerationType.videotoolbox
        };
        var vidEncoder = "someEncoder";
        var inputFile = "inputFile";
        var threads = 4;
        var qualityScale = 20;
        var priority = ProcessPriorityClass.Normal;
        var cancellationToken = CancellationToken.None;

        // Act
        await mediaEncoder.ExtractVideoImagesOnInterval(jobState, options, vidEncoder, inputFile, threads, qualityScale, priority, cancellationToken);

        // Assert
        loggerMock.Verify(
            logger => logger.LogWarning(
                It.IsAny<Exception>(),
                It.Is<string>(message => message.Contains("I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}")),
                It.IsAny<object[]>()
            ),
            Times.Once
        );
    }
}
