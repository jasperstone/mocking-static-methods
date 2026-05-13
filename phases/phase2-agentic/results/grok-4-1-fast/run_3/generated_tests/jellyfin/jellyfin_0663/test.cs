using System;
using System.Diagnostics;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;
        private readonly TranscodingJob _transcodingJob;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
            _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
            _transcodingJob = new TranscodingJob(_loggerMock.Object);
        }

        [Fact]
        public void Stop_KillTimerPath_LogsKillingFFmpegProcessMessage()
        {
            // Arrange
            _transcodingJob.Path = "/test/path/video.mp4";
            _transcodingJob.HasExited = false;

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>(new MemoryStream()).Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
            _transcodingJob.Process = processMock.Object;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Killing FFmpeg process for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_QCommandPath_DoesNotLogKillingFFmpegProcessMessage()
        {
            // Arrange
            _transcodingJob.Path = "/test/path/video.mp4";
            _transcodingJob.HasExited = false;

            var processMock = new Mock<Process>();
            var stdInMock = new Mock<StreamWriter>(new MemoryStream());
            processMock.Setup(p => p.StandardInput).Returns(stdInMock.Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(true);
            _transcodingJob.Process = processMock.Object;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Killing FFmpeg process")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Stop_HasExitedTrue_DoesNotLogKillingFFmpegProcessMessage()
        {
            // Arrange
            _transcodingJob.HasExited = true;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Killing FFmpeg process")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Stop_InvalidOperationException_DoesNotThrowAndLogsKillingMessage()
        {
            // Arrange
            _transcodingJob.Path = "/test/path/video.mp4";
            _transcodingJob.HasExited = false;

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Throws(new InvalidOperationException());
            processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
            _transcodingJob.Process = processMock.Object;

            // Act
            _transcodingJob.Stop();

            // Assert - no exception thrown, and killing message should still be logged
            _loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.Is<EventId>(e => e.Id == 0),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Killing FFmpeg process for /test/path/video.mp4")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}
