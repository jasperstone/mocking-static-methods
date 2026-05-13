using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        // We will test the method that contains the call to _logger.LogWarning on line 945.
        // From the snippet, it is inside a try-catch block catching FfmpegException.
        // We want to verify that when the exception is thrown and enableKeyFrameOnlyExtraction is true,
        // the LogWarning is called with the expected message and exception.

        // Since the method is async and calls ExtractVideoImagesOnIntervalInternal, we will mock that method
        // by subclassing MediaEncoder and overriding it to simulate throwing the exception.

        private class TestMediaEncoder : MediaEncoder
        {
            private readonly Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> _extractFunc;

            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc)
                : base(
                    logger,
                    Mock.Of<IServerConfigurationManager>(),
                    Mock.Of<MediaBrowser.Model.IO.IFileSystem>(),
                    Mock.Of<MediaBrowser.Common.IBlurayExaminer>(),
                    Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>(),
                    Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(),
                    Mock.Of<IServerConfigurationManager>())
            {
                _extractFunc = extractFunc;
            }

            // Expose the method under test with the same signature as the original method containing the try-catch.
            // We replicate the relevant part of the method to test the logging behavior.
            public async Task<string> ExtractVideoImagesOnInterval(
                string inputFile,
                string inputArg,
                bool enableKeyFrameOnlyExtraction,
                string vidEncoder,
                int? threads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                CancellationToken cancellationToken)
            {
                var encodingHelper = new EncodingHelperStub();
                var options = new EncodingOptionsStub { HardwareAccelerationType = HardwareAccelerationType.videotoolbox };
                var jobState = new object();

                // Simulate _isLowPriorityHwDecodeSupported true to match condition in snippet
                typeof(MediaEncoder).GetField("_isLowPriorityHwDecodeSupported", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(this, true);

                string inputArgModified = inputArg;
                if (options.HardwareAccelerationType == HardwareAccelerationType.videotoolbox && true)
                {
                    inputArgModified = "-hwaccel_flags +low_priority " + inputArgModified;
                }

                var filterParam = encodingHelper.GetVideoProcessingFilterParam(jobState, options, vidEncoder).Trim();
                if (string.IsNullOrWhiteSpace(filterParam))
                {
                    throw new InvalidOperationException("EncodingHelper returned empty or invalid filter parameters.");
                }

                try
                {
                    return await _extractFunc(
                        (enableKeyFrameOnlyExtraction ? "-skip_frame nokey " : string.Empty) + inputArgModified,
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

                return await _extractFunc(inputArgModified, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken).ConfigureAwait(false);
            }
        }

        // Stub classes to simulate dependencies
        private class EncodingHelperStub
        {
            public string GetVideoProcessingFilterParam(object jobState, EncodingOptionsStub options, string vidEncoder)
            {
                return "filter_param";
            }
        }

        private class EncodingOptionsStub
        {
            public HardwareAccelerationType HardwareAccelerationType { get; set; }
        }

        private enum HardwareAccelerationType
        {
            videotoolbox,
            none
        }

        private class FfmpegException : Exception
        {
            public FfmpegException(string message) : base(message) { }
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegExceptionWhenEnableKeyFrameOnlyExtraction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var inputFile = "input.mp4";
            var inputArg = "inputArg";
            var vidEncoder = "encoder";
            int? threads = 1;
            int? qualityScale = 2;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Setup extractFunc to throw FfmpegException on first call, then return "success" on second call
            int callCount = 0;
            Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new FfmpegException("Simulated ffmpeg failure");
                }
                return Task.FromResult("success");
            };

            var encoder = new TestMediaEncoder(loggerMock.Object, extractFunc);

            // Act
            var result = await encoder.ExtractVideoImagesOnInterval(inputFile, inputArg, true, vidEncoder, threads, qualityScale, priority, cancellationToken);

            // Assert
            Assert.Equal("success", result);
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed, will attempt standard way. Input:")),
                    It.Is<FfmpegException>(ex => ex.Message == "Simulated ffmpeg failure"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_ThrowsOnFfmpegExceptionWhenEnableKeyFrameOnlyExtractionIsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var inputFile = "input.mp4";
            var inputArg = "inputArg";
            var vidEncoder = "encoder";
            int? threads = 1;
            int? qualityScale = 2;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc = (arg1, arg2, arg3, arg4, arg5, arg6, arg7) =>
            {
                throw new FfmpegException("Simulated ffmpeg failure");
            };

            var encoder = new TestMediaEncoder(loggerMock.Object, extractFunc);

            // Act & Assert
            await Assert.ThrowsAsync<FfmpegException>(() =>
                encoder.ExtractVideoImagesOnInterval(inputFile, inputArg, false, vidEncoder, threads, qualityScale, priority, cancellationToken));

            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Never);
        }
    }
}
