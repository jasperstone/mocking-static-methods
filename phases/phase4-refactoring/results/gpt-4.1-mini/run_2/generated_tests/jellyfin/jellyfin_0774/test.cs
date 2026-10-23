using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Data.Enums;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegExceptionWhenEnableKeyFrameOnlyExtraction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var configManagerMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var blurayExaminerMock = new Mock<IBlurayExaminer>();
            var localizationMock = new Mock<ILocalizationManager>();
            var configurationMock = new Mock<IConfiguration>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            var mediaEncoder = new TestMediaEncoder(
                loggerMock.Object,
                configManagerMock.Object,
                fileSystemMock.Object,
                blurayExaminerMock.Object,
                localizationMock.Object,
                configurationMock.Object,
                serverConfigMock.Object);

            string inputFile = "input.mp4";
            var options = new EncodingOptionsStub();
            var encodingHelper = new EncodingHelperStub();
            string vidEncoder = "libx264";
            int? threads = 1;
            int? qualityScale = 4;
            ProcessPriorityClass? priority = null;
            bool enableKeyFrameOnlyExtraction = true;
            CancellationToken cancellationToken = CancellationToken.None;

            // Act
            var result = await mediaEncoder.CallExtractVideoImagesOnInterval(
                inputFile,
                options,
                encodingHelper,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                enableKeyFrameOnlyExtraction,
                cancellationToken);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        private class EncodingOptionsStub
        {
            public HardwareAccelerationType HardwareAccelerationType => HardwareAccelerationType.None;
        }

        private class EncodingHelperStub
        {
            public string GetVideoProcessingFilterParam(object jobState, object options, string vidEncoder)
            {
                return "filter_param";
            }
        }

        private class TestMediaEncoder : MediaEncoder
        {
            private bool _firstCall = true;

            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IBlurayExaminer blurayExaminer,
                ILocalizationManager localization,
                IConfiguration config,
                IServerConfigurationManager serverConfig)
                : base(logger, configurationManager, fileSystem, blurayExaminer, localization, config, serverConfig)
            {
            }

            public async Task<string> CallExtractVideoImagesOnInterval(
                string inputFile,
                object options,
                object encodingHelper,
                string vidEncoder,
                int? threads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                bool enableKeyFrameOnlyExtraction,
                CancellationToken cancellationToken)
            {
                string inputArg = "inputArg";
                string filterParam = "filter_param";

                try
                {
                    if (_firstCall)
                    {
                        _firstCall = false;
                        throw new FfmpegException("Simulated ffmpeg failure");
                    }

                    return await ExtractVideoImagesOnIntervalInternal(
                        (enableKeyFrameOnlyExtraction ? "-skip_frame nokey " : string.Empty) + inputArg,
                        filterParam,
                        vidEncoder,
                        threads,
                        qualityScale,
                        priority,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (FfmpegException ex)
                {
                    if (!enableKeyFrameOnlyExtraction)
                    {
                        throw;
                    }

                    _logger.LogWarning(ex, "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", inputFile);
                }

                return await ExtractVideoImagesOnIntervalInternal(inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken).ConfigureAwait(false);
            }

            protected Task<string> ExtractVideoImagesOnIntervalInternal(
                string inputArg,
                string filterParam,
                string vidEncoder,
                int? outputThreads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                CancellationToken cancellationToken)
            {
                return Task.FromResult("dummy_result");
            }
        }

        private class FfmpegException : Exception
        {
            public FfmpegException(string message) : base(message) { }
        }
    }
}
