using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public sealed class TranscodingJobTests : IDisposable
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
            _job.Dispose();
        }

        [Fact]
        public void Stop_ProcessNotExited_WritesQAndWaitsFiveSeconds_KillsProcessAndLogsInformation()
        {
            // Arrange
            _job.Path = "/test/path/ffmpeg.mp4";
            _job.HasExited = false;
            var process = new Mock<Process>();
            var stdIn = new Mock<StreamWriter>(new MemoryStream());
            process.Setup(p => p.StandardInput).Returns(stdIn.Object);
            _job.Process = process.Object;
            process.Setup(p => p.WaitForExit(5000)).Returns(false);

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Stopping ffmpeg process with q command for {Path}", "/test/path/ffmpeg.mp4"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Killing FFmpeg process for {Path}", "/test/path/ffmpeg.mp4"), Times.Once);
            process.Verify(p => p.Kill(), Times.Once);
            stdIn.Verify(sw => sw.WriteLine("q"), Times.Once);
        }

        [Fact]
        public void Stop_ProcessNotExited_WritesQAndProcessExitsWithinFiveSeconds_DoesNotKillProcess()
        {
            // Arrange
            _job.Path = "/test/path/ffmpeg.mp4";
            _job.HasExited = false;
            var process = new Mock<Process>();
            var stdIn = new Mock<StreamWriter>(new MemoryStream());
            process.Setup(p => p.StandardInput).Returns(stdIn.Object);
            _job.Process = process.Object;
            process.Setup(p => p.WaitForExit(5000)).Returns(true);

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(x => x.LogInformation("Stopping ffmpeg process with q command for {Path}", "/test/path/ffmpeg.mp4"), Times.Once);
            _loggerMock.Verify(x => x.LogInformation("Killing FFmpeg process for {Path}", "/test/path/ffmpeg.mp4"), Times.Never);
            process.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_HasExited_DoesNotLogOrAttemptToStopProcess()
        {
            // Arrange
            _job.HasExited = true;

            // Act
            _job.Stop();

            // Assert - No log calls
            _loggerMock.Verify(x => x.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()), Times.Never);
        }
    }
}
