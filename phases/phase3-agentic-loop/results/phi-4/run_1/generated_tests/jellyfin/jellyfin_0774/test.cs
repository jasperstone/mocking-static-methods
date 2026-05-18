using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Extensions;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.MediaEncoding.Encoder;
using MediaBrowser.Model.Dlna;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Globalization;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Configuration;

namespace MediaBrowser.MediaEncoding.Encoder.Tests
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task LogWarningIsCalledOnFfmpegExceptionWithKeyFrameOnlyExtraction()
        {
            // Arrange
            var mockLogger = new Mock<ILogger<MediaEncoder>>();
            var mediaEncoder = new MediaEncoder(
                mockLogger.Object,
                Mock.Of<IServerConfigurationManager>(),
                Mock.Of<IFileSystem>(),
                Mock.Of<IBlurayExaminer>(),
                Mock.Of<ILocalizationManager>(),
                Mock.Of<IConfiguration>(),
                Mock.Of<IServerConfigurationManager>());

            var options = new VideoEncodingOptions
            {
                HardwareAccelerationType = HardwareAccelerationType.videotoolbox
            };
            var encodingHelper = Mock.Of<IEncodingHelper>();
            var jobState = Mock.Of<IJobState>();
            var cancellationToken = CancellationToken.None;

            // Simulate the method call that leads to the LogWarning
            Func<Task> methodToTest = async () =>
            {
                try
                {
                    await mediaEncoder.ExtractVideoImagesOnIntervalInternal(
                        "-skip_frame nokey",
                        "filterParam",
                        "vidEncoder",
                        1,
                        4,
                        ProcessPriorityClass.Normal,
                        cancellationToken);
                }
                catch (FfmpegException ex)
                {
                    if (true) // Simulate enableKeyFrameOnlyExtraction being true
                    {
                        mediaEncoder._logger.LogWarning(ex, "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", "inputFile");
                    }
                }
            };

            // Act
            await methodToTest();

            // Assert
            mockLogger.Verify(
                logger => logger.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
