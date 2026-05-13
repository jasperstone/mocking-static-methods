using Xunit;
using Moq;
using System;
using System.Diagnostics;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;

namespace MediaBrowser.Controller.Tests
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
            transcodingJob.Path = "test_path";

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Stopping ffmpeg process with q command for {Path}", "test_path"), Times.Once);
        }

        [Fact]
        public void Stop_LogsInformation_WhenKillingFfmpegProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object);
            transcodingJob.Process = new Process();
            transcodingJob.Path = "test_path";
            //transcodingJob.Process.WaitForExit(5000); // Make WaitForExit return false

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Killing FFmpeg process for {Path}", "test_path"), Times.Once);
        }
    }
}
