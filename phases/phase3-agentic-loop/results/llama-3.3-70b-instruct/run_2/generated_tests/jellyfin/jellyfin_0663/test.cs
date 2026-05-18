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
        private const string LogMessageTemplate = "Stopping ffmpeg process with q command for {Path}";
        private const string KillLogMessageTemplate = "Killing FFmpeg process for {Path}";

        [Fact]
        public void Stop_LogsInformation_WhenStoppingFfmpegProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object);
            transcodingJob.Path = "test_path";
            transcodingJob.Process = new Process();

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(l => l.LogInformation(LogMessageTemplate, "test_path"), Times.Once);
            loggerMock.Verify(l => l.LogInformation(KillLogMessageTemplate, "test_path"), Times.Once);
        }

        [Fact]
        public void Stop_DoesNotLogInformation_WhenProcessHasExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object);
            transcodingJob.Path = "test_path";
            transcodingJob.Process = new Process();
            transcodingJob.HasExited = true;

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s == LogMessageTemplate), It.Is<object[]>(o => o.Length == 1 && (string)o[0] == "test_path")), Times.Never);
            loggerMock.Verify(l => l.LogInformation(It.Is<string>(s => s == KillLogMessageTemplate), It.Is<object[]>(o => o.Length == 1 && (string)o[0] == "test_path")), Times.Never);
        }
    }
}
