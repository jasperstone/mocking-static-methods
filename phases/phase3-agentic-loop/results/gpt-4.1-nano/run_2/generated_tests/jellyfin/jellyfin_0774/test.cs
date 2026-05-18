using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.MediaEncoding.Encoder;

namespace MediaEncoderTests
{
    public class MediaEncoderTests
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;
        private readonly Mock<IFileSystem> _fileSystemMock;
        private readonly Mock<IBlurayExaminer> _blurayExaminerMock;
        private readonly Mock<ILocalizationManager> _localizationMock;
        private readonly Mock<IConfiguration> _configMock;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
            _fileSystemMock = new Mock<IFileSystem>();
            _blurayExaminerMock = new Mock<IBlurayExaminer>();
            _localizationMock = new Mock<ILocalizationManager>();
            _configMock = new Mock<IConfiguration>();
        }

        [Fact]
        public async Task ExtractVideoImagesOnInterval_Internal_CallsLogWarning_OnFfmpegException()
        {
            // Arrange
            var encoder = new TestMediaEncoder(
                _loggerMock.Object,
                _configManagerMock.Object,
                _fileSystemMock.Object,
                _blurayExaminerMock.Object,
                _localizationMock.Object,
                _configMock.Object);

            // Setup the method to throw FfmpegException when called
            var inputArg = "-i input.mp4";
            var filterParam = "filter";
            var vidEncoder = "h264";
            int threads = 1;
            int qualityScale = 4;
            ProcessPriorityClass? priority = null;
            var cancellationToken = CancellationToken.None;

            // Act
            await testEncoder.SetupAndInvokeExtractVideoImagesOnIntervalInternal(
                inputArg,
                filterParam,
                vidEncoder,
                threads,
                qualityScale,
                priority,
                cancellationToken,
                throwFfmpegException: true);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("I-frame trickplay extraction failed, will attempt standard way.")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()),
                Times.Once);
        }

        // Helper subclass to expose private method for testing
        public class TestMediaEncoder : MediaEncoder
        {
            public TestMediaEncoder(
                ILogger<MediaEncoder> logger,
                IServerConfigurationManager configurationManager,
                IFileSystem fileSystem,
                IBlurayExaminer blurayExaminer,
                ILocalizationManager localization,
                IConfiguration config)
                : base(logger, configurationManager, fileSystem, blurayExaminer, localization, config)
            {
            }

            public async Task SetupAndInvokeExtractVideoImagesOnIntervalInternal(
                string inputArg,
                string filterParam,
                string vidEncoder,
                int threads,
                int qualityScale,
                ProcessPriorityClass? priority,
                CancellationToken cancellationToken,
                bool throwFfmpegException)
            {
                var method = typeof(MediaEncoder).GetMethod("ExtractVideoImagesOnIntervalInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (method == null)
                    throw new InvalidOperationException("Method not found");

                try
                {
                    var task = (Task<string>)method.Invoke(this, new object[] { inputArg, filterParam, vidEncoder, threads, qualityScale, priority, cancellationToken });
                    if (throwFfmpegException)
                    {
                        // simulate exception
                        throw new FfmpegException("Simulated ffmpeg error");
                    }
                    await task;
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is FfmpegException)
                    {
                        if (throwFfmpegException)
                            throw ex.InnerException;
                    }
                    throw;
                }
            }
        }

        public class FfmpegException : Exception
        {
            public FfmpegException(string message) : base(message) { }
        }
    }
}
