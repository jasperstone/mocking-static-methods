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
            _transcodingJob = new TranscodingJob(_loggerMock.Object);
        }

        [Fact]
        public void Stop_ProcessNotExited_TriesQCommand_WaitFails_LogsKillingFFmpegProcess()
        {
            // Arrange
            _transcodingJob.Path = "/test/path/ffmpeg.mp4";
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
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(format => format.ToString().Contains("Killing FFmpeg process for {Path}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            processMock.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void Stop_ProcessNotExited_TriesQCommand_WaitSucceeds_DoesNotLogKillingFFmpegProcess()
        {
            // Arrange
            _transcodingJob.Path = "/test/path/ffmpeg.mp4";
            _transcodingJob.HasExited = false;

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>(new MemoryStream()).Object);
            processMock.Setup(p => p.WaitForExit(5000)).Returns(true);
            _transcodingJob.Process = processMock.Object;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(format => format.ToString().Contains("Killing FFmpeg process for {Path}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_HasExited_DoesNotLogKillingFFmpegProcess()
        {
            // Arrange
            _transcodingJob.HasExited = true;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyFormat>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Stop_ProcessNull_DoesNotThrow()
        {
            // Arrange
            _transcodingJob.HasExited = false;
            _transcodingJob.Process = null;

            // Act
            _transcodingJob.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyFormat>(format => format.ToString().Contains("Killing FFmpeg process")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }
    }
}
