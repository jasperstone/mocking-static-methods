using System;
using System.Diagnostics;
using System.IO;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests : IDisposable
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;
        private readonly TranscodingJob _job;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
            _job = new TranscodingJob(_loggerMock.Object);
        }

        public void Dispose()
        {
            _job?.Dispose();
        }

        [Fact]
        public void Stop_ProcessDoesNotExitAfterQCommand_LogsKillingFFmpegProcess()
        {
            // Arrange
            _job.Path = "/test/path/to/ffmpeg";
            _job.HasExited = false;

            var processMock = new Mock<Process>();
            var stdInMock = new Mock<StreamWriter>(new MemoryStream());
            processMock.Setup(p => p.StandardInput).Returns(stdInMock.Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
            processMock.Setup(p => p.Kill());
            _job.Process = processMock.Object;

            // Act
            _job.Stop();

            // Assert - Verifies coverage of line 262: _logger.LogInformation("Killing FFmpeg process for {Path}", Path);
            _loggerMock.Verify(
                x => x.LogInformation("Killing FFmpeg process for {Path}", "/test/path/to/ffmpeg"),
                Times.Once);
        }

        [Fact]
        public void Stop_ProcessExitsAfterQCommand_LogsStoppingFFmpegOnly()
        {
            // Arrange
            _job.Path = "/test/path/to/ffmpeg";
            _job.HasExited = false;

            var processMock = new Mock<Process>();
            var stdInMock = new Mock<StreamWriter>(new MemoryStream());
            processMock.Setup(p => p.StandardInput).Returns(stdInMock.Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(true);
            _job.Process = processMock.Object;

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Stopping ffmpeg process with q command for {Path}", "/test/path/to/ffmpeg"),
                Times.Once);

            _loggerMock.Verify(
                x => x.LogInformation("Killing FFmpeg process for {Path}", It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void Stop_HasExitedTrue_NoLoggingOccurs()
        {
            // Arrange
            _job.HasExited = true;

            // Act
            _job.Stop();

            // Assert - No LogInformation calls should occur
            _loggerMock.Verify(
                x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()),
                Times.Never);
        }
    }
}
