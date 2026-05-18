using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

namespace MediaBrowser.Tests
{
    public class TranscodingSegmentCleanerTests
    {
        private readonly Mock<ILogger<TranscodingSegmentCleaner>> _loggerMock;
        private readonly Mock<MediaBrowser.Common.Configuration.IConfigurationManager> _configMock;
        private readonly Mock<MediaBrowser.Model.IO.IFileSystem> _fileSystemMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;

        public TranscodingSegmentCleanerTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingSegmentCleaner>>();
            _configMock = new Mock<MediaBrowser.Common.Configuration.IConfigurationManager>();
            _fileSystemMock = new Mock<MediaBrowser.Model.IO.IFileSystem>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
        }

        [Fact]
        public async Task TimerCallback_Should_LogDebug_When_Called()
        {
            // Arrange
            var job = new TranscodingJob
            {
                Path = "some/path",
                Type = TranscodingJobType.Hls,
                DownloadPositionTicks = TimeSpan.FromSeconds(100).Ticks
            };

            var options = new MediaBrowser.Model.Configuration.EncodingOptions
            {
                EnableSegmentDeletion = true,
                SegmentKeepSeconds = 20
            };

            _configMock.Setup(c => c.GetConfiguration<MediaBrowser.Model.Configuration.EncodingOptions>())
                .Returns(options);

            var cleaner = new TranscodingSegmentCleaner(
                job,
                _loggerMock.Object,
                _configMock.Object,
                _fileSystemMock.Object,
                _mediaEncoderMock.Object,
                segmentLength: 10);

            // Use reflection to invoke the private TimerCallback method
            var methodInfo = typeof(TranscodingSegmentCleaner).GetMethod("TimerCallback", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            Assert.NotNull(methodInfo);

            // Act
            await (Task)methodInfo.Invoke(cleaner, new object[] { null });

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Deleting segment file(s) index")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.AtLeastOnce);
        }
    }
}
