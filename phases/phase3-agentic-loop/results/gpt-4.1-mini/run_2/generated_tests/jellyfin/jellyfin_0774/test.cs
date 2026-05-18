using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using System.Diagnostics;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Model.Globalization;
using Microsoft.Extensions.Configuration;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        // Dummy FfmpegException class for testing
        public class FfmpegException : Exception
        {
            public FfmpegException(string message) : base(message) { }
        }

        // We create a derived test class to override ExtractVideoImagesOnIntervalInternal to simulate throwing and returning.
        private class TestMediaEncoder : MediaEncoder
        {
            private readonly Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> _extractFunc;
            private readonly ILogger<MediaEncoder> _logger;

            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc)
                : base(
                    logger,
                    new Mock<IServerConfigurationManager>().Object,
                    new Mock<IFileSystem>().Object,
                    new Mock<IBlurayExaminer>().Object,
                    new Mock<ILocalizationManager>().Object,
                    new Mock<IConfiguration>().Object,
                    new Mock<IServerConfigurationManager>().Object)
            {
                _extractFunc = extractFunc;
                _logger = logger;
            }

            // Expose the method under test as public for testing
            public async Task<string> ExtractVideoImagesOnIntervalPublic(
                string inputFile,
                bool enableKeyFrameOnlyExtraction,
                string inputArg,
                string vidEncoder,
                int? threads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                CancellationToken cancellationToken)
            {
                var filterParam = "filterParam";

                if (string.IsNullOrWhiteSpace(filterParam))
                {
                    throw new InvalidOperationException("EncodingHelper returned empty or invalid filter parameters.");
                }

                try
                {
                    return await _extractFunc(
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

                return await _extractFunc(inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken).ConfigureAwait(false);
            }
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegException_WhenEnableKeyFrameOnlyExtraction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var inputFile = "input.mp4";
            var inputArg = "-inputArg";
            var vidEncoder = "encoder";
            int? threads = 2;
            int? qualityScale = 3;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            // Setup extractFunc to throw FfmpegException on first call, then return "success" on second call
            int callCount = 0;
            Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc = (ia, fp, ve, t, qs, p, ct) =>
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
            var result = await encoder.ExtractVideoImagesOnIntervalPublic(
                inputFile,
                enableKeyFrameOnlyExtraction: true,
                inputArg,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                cancellationToken);

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
        public async Task ExtractVideoImagesOnInterval_ThrowsOnFfmpegException_WhenNotEnableKeyFrameOnlyExtraction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var inputFile = "input.mp4";
            var inputArg = "-inputArg";
            var vidEncoder = "encoder";
            int? threads = 2;
            int? qualityScale = 3;
            ProcessPriorityClass? priority = ProcessPriorityClass.Normal;
            var cancellationToken = CancellationToken.None;

            Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc = (ia, fp, ve, t, qs, p, ct) =>
            {
                throw new FfmpegException("Simulated ffmpeg failure");
            };

            var encoder = new TestMediaEncoder(loggerMock.Object, extractFunc);

            // Act & Assert
            await Assert.ThrowsAsync<FfmpegException>(() => encoder.ExtractVideoImagesOnIntervalPublic(
                inputFile,
                enableKeyFrameOnlyExtraction: false,
                inputArg,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                cancellationToken));

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
