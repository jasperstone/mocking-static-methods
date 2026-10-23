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
        transcodingJob.Path = "test_path";
        var processMock = new Mock<Process>();
        transcodingJob.Process = processMock.Object;

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() == "Stopping ffmpeg process with q command for test_path"),
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
        transcodingJob.Path = "test_path";
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
        transcodingJob.Process = processMock.Object;

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() == "Stopping ffmpeg process with q command for test_path"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
        loggerMock.Verify(logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString() == "Killing FFmpeg process for test_path"),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception, string>>()
            ), Times.Once);
    }
}
