using System;
using System.Diagnostics;
using System.IO;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;
        private readonly TranscodingJob _job;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
            _job = new TranscodingJob(_loggerMock.Object);
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

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Killing FFmpeg process for {Path}", "/test/path/to/ffmpeg"),
                Times.Once);
        }

        [Fact]
        public void Stop_ProcessExitsAfterQCommand_OnlyLogsStoppingFFmpegProcess()
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
        public void Stop_HasExitedTrue_NoProcessInteractionOccurs()
        {
            // Arrange
            _job.HasExited = true;

            // Act
            _job.Stop();

            // Assert - no logging should occur for process operations
            _loggerMock.VerifyNoOtherCalls();
        }
    }
}
