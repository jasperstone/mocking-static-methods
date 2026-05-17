using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Diagnostics;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock = new();
        private readonly Mock<IServerConfigurationManager> _configManagerMock = new();
        private readonly Mock<IFileSystem> _fileSystemMock = new();
        private readonly Mock<object> _blurayExaminerMock = new();
        private readonly Mock<object> _localizationMock = new();
        private readonly Mock<Microsoft.Extensions.Configuration.IConfiguration> _configurationMock = new();
        private readonly Mock<IServerConfigurationManager> _serverConfigMock = new();

        private class TestMediaEncoder : MediaEncoder
        {
            private readonly ILogger<MediaEncoder> _logger;

            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                object blurayExaminer,
                object localization,
                Microsoft.Extensions.Configuration.IConfiguration config,
                IServerConfigurationManager serverConfig)
                : base(logger, configurationManager, fileSystem, null, null, config, serverConfig)
            {
                _logger = logger;
            }

            public Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> ExtractVideoImagesOnIntervalInternalOverride { get; set; }

            public async Task<string> ExtractVideoImagesOnIntervalForTest(
                string inputFile,
                string inputArg,
                string vidEncoder,
                int? threads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                bool enableKeyFrameOnlyExtraction,
                CancellationToken cancellationToken)
            {
                var encodingHelper = new EncodingHelperStub();

                var filterParam = encodingHelper.GetVideoProcessingFilterParam(null, null, vidEncoder).Trim();
                if (string.IsNullOrWhiteSpace(filterParam))
                {
                    throw new InvalidOperationException("EncodingHelper returned empty or invalid filter parameters.");
                }

                try
                {
                    if (ExtractVideoImagesOnIntervalInternalOverride != null)
                    {
                        return await ExtractVideoImagesOnIntervalInternalOverride(
                            (enableKeyFrameOnlyExtraction ? "-skip_frame nokey " : string.Empty) + inputArg,
                            filterParam,
                            vidEncoder,
                            threads,
                            qualityScale,
                            priority,
                            cancellationToken).ConfigureAwait(false);
                    }
                    throw new NotImplementedException();
                }
                catch (FfmpegException ex)
                {
                    if (!enableKeyFrameOnlyExtraction)
                    {
                        throw;
                    }

                    _logger.LogWarning(ex, "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}", inputFile);
                }

                if (ExtractVideoImagesOnIntervalInternalOverride != null)
                {
                    return await ExtractVideoImagesOnIntervalInternalOverride(inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken).ConfigureAwait(false);
                }
                throw new NotImplementedException();
            }
        }

        private class EncodingHelperStub
        {
            public string GetVideoProcessingFilterParam(object jobState, object options, string vidEncoder)
            {
                return "filter_param";
            }
        }

        [Fact]
        public async Task ExtractVideoImagesOnIntervalForTest_LogsWarningOnFfmpegExceptionWhenEnableKeyFrameOnlyExtraction()
        {
            var encoder = new TestMediaEncoder(
                _loggerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _blurayExaminerMock.Object,
                _localizationMock.Object,
                _configurationMock.Object,
                _serverConfigMock.Object);

            var inputFile = "input.mp4";
            var inputArg = "-inputArg";
            var vidEncoder = "encoder";
            int? threads = 1;
            int? qualityScale = 2;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            bool enableKeyFrameOnlyExtraction = true;
            var cancellationToken = CancellationToken.None;

            var ffmpegException = new FfmpegException("Test ffmpeg exception");

            int callCount = 0;
            encoder.ExtractVideoImagesOnIntervalInternalOverride = (inputArgParam, filterParam, vidEnc, thr, qual, prio, ct) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw ffmpegException;
                }
                return Task.FromResult("result");
            };

            var result = await encoder.ExtractVideoImagesOnIntervalForTest(
                inputFile,
                inputArg,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                enableKeyFrameOnlyExtraction,
                cancellationToken);

            Assert.Equal("result", result);
            _loggerMock.Verify(
                x => x.LogWarning(
                    ffmpegException,
                    "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}",
                    inputFile),
                Times.Once);
        }

        [Fact]
        public async Task ExtractVideoImagesOnIntervalForTest_ThrowsOnFfmpegExceptionWhenNotEnableKeyFrameOnlyExtraction()
        {
            var encoder = new TestMediaEncoder(
                _loggerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _blurayExaminerMock.Object,
                _localizationMock.Object,
                _configurationMock.Object,
                _serverConfigMock.Object);

            var inputFile = "input.mp4";
            var inputArg = "-inputArg";
            var vidEncoder = "encoder";
            int? threads = 1;
            int? qualityScale = 2;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            bool enableKeyFrameOnlyExtraction = false;
            var cancellationToken = CancellationToken.None;

            var ffmpegException = new FfmpegException("Test ffmpeg exception");

            encoder.ExtractVideoImagesOnIntervalInternalOverride = (inputArgParam, filterParam, vidEnc, thr, qual, prio, ct) =>
            {
                throw ffmpegException;
            };

            var ex = await Assert.ThrowsAsync<FfmpegException>(() => encoder.ExtractVideoImagesOnIntervalForTest(
                inputFile,
                inputArg,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                enableKeyFrameOnlyExtraction,
                cancellationToken));

            Assert.Same(ffmpegException, ex);
            _loggerMock.Verify(
                x => x.LogWarning(
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Never);
        }
    }

    public class FfmpegException : Exception
    {
        public FfmpegException(string message) : base(message) { }
    }
}
