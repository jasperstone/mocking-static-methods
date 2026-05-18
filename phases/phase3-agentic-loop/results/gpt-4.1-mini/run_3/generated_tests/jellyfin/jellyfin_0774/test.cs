using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Model.Configuration;
using MediaBrowser.Model.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Globalization;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests
    {
        // We will test the method that contains the call to _logger.LogWarning on line 945.
        // From the snippet, it is inside a try-catch around ExtractVideoImagesOnIntervalInternal.
        // We will simulate the FfmpegException thrown from ExtractVideoImagesOnIntervalInternal
        // when enableKeyFrameOnlyExtraction is true, so the catch block is hit and LogWarning is called.

        // We need to mock ILogger<MediaEncoder> to verify LogWarning call.
        // We also need to mock or subclass MediaEncoder to override ExtractVideoImagesOnIntervalInternal to throw.

        private class TestMediaEncoder : MediaEncoder
        {
            private readonly Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> _extractFunc;

            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                ILocalizationManager localization,
                Microsoft.Extensions.Configuration.IConfiguration config,
                IServerConfigurationManager serverConfig,
                Func<string, string, string, int?, int?, ProcessPriorityClass?, CancellationToken, Task<string>> extractFunc)
                : base(logger, configurationManager, fileSystem, null, localization, config, serverConfig)
            {
                _extractFunc = extractFunc;
            }

            // Override the internal method to call our delegate
            protected override Task<string> ExtractVideoImagesOnIntervalInternal(
                string inputArg,
                string filterParam,
                string vidEncoder,
                int? outputThreads,
                int? qualityScale,
                ProcessPriorityClass? priority,
                CancellationToken cancellationToken)
            {
                return _extractFunc(inputArg, filterParam, vidEncoder, outputThreads, qualityScale, priority, cancellationToken);
            }
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_LogsWarningOnFfmpegExceptionWhenEnableKeyFrameOnlyExtraction()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MediaEncoder>>();
            var configMock = new Mock<Microsoft.Extensions.Configuration.IConfiguration>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();
            var fileSystemMock = new Mock<IFileSystem>();
            var localizationMock = new Mock<ILocalizationManager>();
            var configurationManagerMock = new Mock<IServerConfigurationManager>();

            // Setup configuration to return empty string for ffmpeg path to avoid side effects
            configMock.Setup(c => c.GetValue<string>(It.IsAny<string>())).Returns(string.Empty);
            serverConfigMock.Setup(s => s.Configuration).Returns((ServerConfiguration)null);
            configurationManagerMock.Setup(c => c.GetEncodingOptions()).Returns(new EncodingOptions());

            var ffmpegException = new FfmpegException("Test exception");

            // The delegate will throw FfmpegException on first call, then return a string on second call
            int callCount = 0;
            Task<string> ExtractFunc(string inputArg, string filterParam, string vidEncoder, int? outputThreads, int? qualityScale, ProcessPriorityClass? priority, CancellationToken cancellationToken)
            {
                callCount++;
                if (callCount == 1)
                {
                    throw ffmpegException;
                }
                return Task.FromResult("success");
            }

            var encoder = new TestMediaEncoder(
                loggerMock.Object,
                configurationManagerMock.Object,
                fileSystemMock.Object,
                localizationMock.Object,
                configMock.Object,
                serverConfigMock.Object,
                ExtractFunc);

            // We need to call the public method that contains the try-catch and calls ExtractVideoImagesOnIntervalInternal.
            // The snippet is partial, so we must find the method name.
            // The snippet shows the call to ExtractVideoImagesOnIntervalInternal inside an async method.
            // The method is likely named ExtractVideoImagesOnInterval with parameters matching the snippet.

            var method = typeof(MediaEncoder).GetMethod("ExtractVideoImagesOnInterval", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(method);

            // Prepare parameters for the method call
            var enableKeyFrameOnlyExtraction = true;
            var inputArg = "inputArg";
            var inputFile = "inputFile";
            var vidEncoder = "vidEncoder";
            int? threads = null;
            int? qualityScale = null;
            ProcessPriorityClass? priority = null;
            var cancellationToken = CancellationToken.None;

            // Act
            var task = (Task<string>)method.Invoke(encoder, new object[] { enableKeyFrameOnlyExtraction, inputArg, inputFile, vidEncoder, threads, qualityScale, priority, cancellationToken });
            var result = await task;

            // Assert
            Assert.Equal("success", result);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed")),
                    ffmpegException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
