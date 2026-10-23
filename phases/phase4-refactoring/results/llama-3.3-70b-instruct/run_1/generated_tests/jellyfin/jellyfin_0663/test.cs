using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace MediaBrowser.Controller.MediaEncoding;

public class TranscodingJobTests
{
    [Fact]
    public void Stop_LogsInformation_WhenStoppingFfmpegProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var transcodingJob = new TranscodingJob(loggerMock.Object);
        transcodingJob.Path = "testPath";
        transcodingJob.Process = new Process();

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Stopping ffmpeg process with q command for testPath"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }

    [Fact]
    public void Stop_LogsInformation_WhenKillingFfmpegProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var transcodingJob = new TranscodingJob(loggerMock.Object);
        transcodingJob.Path = "testPath";
        var process = new Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.Start();
        transcodingJob.Process = process;

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString() == "Killing FFmpeg process for testPath"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
