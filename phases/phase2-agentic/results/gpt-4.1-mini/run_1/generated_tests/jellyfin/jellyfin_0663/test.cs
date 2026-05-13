using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_WhenProcessNotExited_LogsStoppingAndKillsProcessIfNotExitedAfterWait()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = false,
                Path = "testpath",
                Process = new Mock<Process>().Object
            };

            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());
            job.Process = processMock.Object;

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            standardInputMock.Verify(s => s.WriteLine("q"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            processMock.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void Stop_WhenProcessExited_DoesNotLogKillOrCallKill()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);

            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = true,
                Path = "testpath",
                Process = processMock.Object
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            processMock.Verify(p => p.StandardInput.WriteLine(It.IsAny<string>()), Times.Never);
            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_WhenWaitForExitReturnsTrue_DoesNotCallKill()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = false,
                Path = "testpath",
                Process = processMock.Object
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            processMock.Verify(p => p.StandardInput.WriteLine("q"), Times.Once);
            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_WhenProcessThrowsInvalidOperationException_DoesNotThrow()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws<InvalidOperationException>();

            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = false,
                Path = "testpath",
                Process = processMock.Object
            };

            // Act & Assert
            var exception = Record.Exception(() => job.Stop());
            Assert.Null(exception);
        }
    }
}
