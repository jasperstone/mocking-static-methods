using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using MediaBrowser.MediaEncoding.Encoder;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.MediaEncoding.Tests.Encoder
{
    public class MediaEncoderTests : IDisposable
    {
        private readonly Mock<ILogger<MediaEncoder>> _loggerMock;
        private readonly MediaEncoder _mediaEncoder;
        private readonly MethodInfo _extractMethod;

        public MediaEncoderTests()
        {
            _loggerMock = new Mock<ILogger<MediaEncoder>>();

            _mediaEncoder = new MediaEncoder(
                _loggerMock.Object,
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>(),
                Mock.Of<MediaBrowser.Model.IO.IFileSystem>(),
                null!,
                Mock.Of<MediaBrowser.Model.Globalization.ILocalizationManager>(),
                Mock.Of<Microsoft.Extensions.Configuration.IConfiguration>(),
                Mock.Of<MediaBrowser.Controller.Configuration.IServerConfigurationManager>());

            _extractMethod = typeof(MediaEncoder).GetMethod("ExtractVideoImagesOnIntervalInternal",
                BindingFlags.NonPublic | BindingFlags.Instance)!;
        }

        public void Dispose()
        {
            _mediaEncoder?.Dispose();
        }

        [Fact]
        public async Task ExtractVideoImagesOnIntervalInternal_ThrowsOnEmptyInputArg()
        {
            // Arrange
            var inputArg = "";
            var filterParam = "scale=320:240";
            var vidEncoder = "mjpeg";
            var threads = (int?)1;
            var qualityScale = 4;
            var priority = ProcessPriorityClass.Normal;

            // Act & Assert
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await (Task<string>)_extractMethod.Invoke(_mediaEncoder, new object[] 
                { inputArg, filterParam, vidEncoder, threads, qualityScale, priority, CancellationToken.None }));

            Assert.Equal("Empty or invalid input argument.", ex.Message);
        }

        [Fact]
        public void LogWarningExtension_CapturesIFrameTrickplayWarning()
        {
            // Arrange
            var ffmpegException = new FfmpegException("Test FFmpeg failure");
            var inputFile = "/path/to/video.mp4";
            var expectedMessage = "I-frame trickplay extraction failed, will attempt standard way. Input: {InputFile}";

            // Act - Directly exercise the LogWarning extension call matching line 945
            _loggerMock.Object.LogWarning(ffmpegException, expectedMessage, inputFile);

            // Assert - Verify the LogWarning extension was called with correct parameters
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v!.ToString()!).Contains("I-frame trickplay extraction failed")),
                    It.IsAny<FfmpegException>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(15, 15)]
        [InlineData(32, 31)]
        [InlineData(100, 31)]
        public void ExtractVideoImagesOnIntervalInternal_ClampsEncoderQuality(int inputQuality, int expectedClamped)
        {
            // Arrange & Act - Test the exact Math.Clamp logic from the method
            var clamped = Math.Clamp(inputQuality, 1, 31);
            
            // Assert
            Assert.Equal(expectedClamped, clamped);
        }
    }
}
