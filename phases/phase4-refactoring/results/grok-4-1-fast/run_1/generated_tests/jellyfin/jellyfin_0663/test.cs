using System;
using System.Diagnostics;
using System.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
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
            _job.Dispose();
        }

        [Fact]
        public void Stop_WithNonExitedProcessAndTimeout_ShouldLogKillingFFmpegProcess()
        {
            // Arrange
            _job.Path = "/test/path/ffmpeg.mp4";
            _job.HasExited = false;
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>(new MemoryStream()).Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
            _job.Process = processMock.Object;

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Killing FFmpeg process for {Path}", "/test/path/ffmpeg.mp4"),
                Times.Once);
        }

        [Fact]
        public void Stop_WithNonExitedProcessAndQuickExit_ShouldNotLogKillingFFmpegProcess()
        {
            // Arrange
            _job.Path = "/test/path/ffmpeg.mp4";
            _job.HasExited = false;
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>(new MemoryStream()).Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(true);
            _job.Process = processMock.Object;

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Killing FFmpeg process for {Path}", It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public void Stop_WithExitedProcess_ShouldNotLogProcessOperations()
        {
            // Arrange
            _job.HasExited = true;

            // Act
            _job.Stop();

            // Assert - no LogInformation calls expected
            _loggerMock.VerifyNoOtherCalls();
        }

        [Fact]
        public void Stop_WithProcessAndQCommand_ShouldLogStoppingFFmpegProcess()
        {
            // Arrange
            _job.Path = "/test/path/ffmpeg.mp4";
            _job.HasExited = false;
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>(new MemoryStream()).Object);
            _job.Process = processMock.Object;

            // Act
            _job.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation("Stopping ffmpeg process with q command for {Path}", "/test/path/ffmpeg.mp4"),
                Times.Once);
        }
    }
}
