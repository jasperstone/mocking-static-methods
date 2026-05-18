using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingJobTests
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;
        private readonly Mock<TranscodingThrottler> _transcodingThrottlerMock;
        private readonly Mock<TranscodingSegmentCleaner> _transcodingSegmentCleanerMock;
        private readonly Mock<CancellationTokenSource> _cancellationTokenSourceMock;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
            _transcodingThrottlerMock = new Mock<TranscodingThrottler>();
            _transcodingSegmentCleanerMock = new Mock<TranscodingSegmentCleaner>();
            _cancellationTokenSourceMock = new Mock<CancellationTokenSource>();
        }

        [Fact]
        public void Stop_LogsInformation_WhenStoppingFfmpegProcess()
        {
            // Arrange
            var transcodingJob = new TranscodingJob(_loggerMock.Object);
            transcodingJob.Process = new Mock<Process>().Object;
            transcodingJob.Path = "path";

            // Act
            transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Stopping ffmpeg process with q command for path"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void Stop_LogsInformation_WhenKillingFfmpegProcess()
        {
            // Arrange
            var transcodingJob = new TranscodingJob(_loggerMock.Object);
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
            transcodingJob.Process = processMock.Object;
            transcodingJob.Path = "path";

            // Act
            transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString() == "Killing FFmpeg process for path"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()), Times.Once);
        }

        [Fact]
        public void Stop_DisposesTranscodingThrottler()
        {
            // Arrange
            var transcodingJob = new TranscodingJob(_loggerMock.Object);
            transcodingJob.TranscodingThrottler = _transcodingThrottlerMock.Object;

            // Act
            transcodingJob.Stop();

            // Assert
            _transcodingThrottlerMock.Verify(throttler => throttler.Stop(), Times.Once);
        }

        [Fact]
        public void Stop_StopsTranscodingSegmentCleaner()
        {
            // Arrange
            var transcodingJob = new TranscodingJob(_loggerMock.Object);
            transcodingJob.TranscodingSegmentCleaner = _transcodingSegmentCleanerMock.Object;

            // Act
            transcodingJob.Stop();

            // Assert
            _transcodingSegmentCleanerMock.Verify(cleaner => cleaner.Stop(), Times.Once);
        }
    }
}
