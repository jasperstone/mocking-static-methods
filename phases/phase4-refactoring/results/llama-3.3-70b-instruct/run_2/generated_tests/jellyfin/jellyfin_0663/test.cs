using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MediaBrowser.Controller.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_LogsInformation_WhenStoppingFfmpegProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object);
            transcodingJob.Process = new Process();
            transcodingJob.Path = "testPath";

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == "Stopping ffmpeg process with q command for testPath"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }

        [Fact]
        public void Stop_KillsFfmpegProcess_WhenWaitForExitFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object);
            transcodingJob.Process = new Process();
            transcodingJob.Path = "testPath";

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(logger => logger.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((state, _) => state.ToString() == "Killing FFmpeg process for testPath"),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        }
    }
}
