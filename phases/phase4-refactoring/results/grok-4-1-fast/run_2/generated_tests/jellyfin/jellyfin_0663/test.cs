using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            _job.Path = "/test/path/video.mp4";
        }

        public void Dispose()
        {
            _job?.Dispose();
        }

        [Fact]
        public void Stop_ProcessNotExited_WritesQAndWaitsForExit_SuccessfullyExits()
        {
            // Arrange
            var process = new Mock<Process>();
            process.Setup(p => p.WaitForExit(5000)).Returns(true);
            _job.Process = process.Object;
            _job.HasExited = false;

            // Act
            _job.Stop();

            // Assert
            process.Verify(p => p.StandardInput.WriteLine("q"), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Stop_ProcessNotExited_WritesQButDoesNotExitWithinTimeout_LogsKillAndKillsProcess()
        {
            // Arrange
            var process = new Mock<Process>();
            process.Setup(p => p.WaitForExit(5000)).Returns(false);
            process.Setup(p => p.Kill());
            _job.Process = process.Object;
            _job.HasExited = false;

            // Act
            _job.Stop();

            // Assert
            process.Verify(p => p.StandardInput.WriteLine("q"), Times.Once);
            process.Verify(p => p.Kill(), Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    0,
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_ProcessAlreadyExited_DoesNotAttemptToStopProcess()
        {
            // Arrange
            _job.HasExited = true;

            // Act
            _job.Stop();

            // Assert - no LogInformation calls
            _loggerMock.Verify(
                x => x.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
