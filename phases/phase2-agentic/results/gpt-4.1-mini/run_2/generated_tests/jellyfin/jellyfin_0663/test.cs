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
        public void Stop_LogsInformationAndKillsProcessIfNotExitedAndWaitForExitTimesOut()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<TextWriter>();

            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());

            var job = new TranscodingJob(loggerMock.Object)
            {
                Process = processMock.Object,
                HasExited = false,
                Path = "testpath"
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            standardInputMock.Verify(s => s.WriteLine("q"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            processMock.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void Stop_DoesNotKillProcessIfWaitForExitReturnsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<TextWriter>();

            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var job = new TranscodingJob(loggerMock.Object)
            {
                Process = processMock.Object,
                HasExited = false,
                Path = "testpath"
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            standardInputMock.Verify(s => s.WriteLine("q"), Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_DoesNothingIfHasExitedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();

            var job = new TranscodingJob(loggerMock.Object)
            {
                Process = processMock.Object,
                HasExited = true,
                Path = "testpath"
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_CatchesInvalidOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<TextWriter>();

            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws<InvalidOperationException>();

            var job = new TranscodingJob(loggerMock.Object)
            {
                Process = processMock.Object,
                HasExited = false,
                Path = "testpath"
            };

            // Act & Assert
            var exception = Record.Exception(() => job.Stop());
            Assert.Null(exception);
        }
    }
}
