using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding.Tests
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
            transcodingJob.Process.HasExited = false;

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(logger => logger.LogInformation("Killing FFmpeg process for {Path}", "test_path"), Times.Once);
        }
    }
}
