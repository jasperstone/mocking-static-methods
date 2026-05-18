using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using System.Diagnostics;
using System.Threading;
using MediaBrowser.Model.Dto;

public class TranscodingJobTests
{
    [Fact]
    public void Stop_LogsInformation_WhenProcessDoesNotExitInTime()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var processMock = new Mock<Process>();
        var mediaSourceInfo = new MediaSourceInfo { Path = "testPath" };

        processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
        processMock.Setup(p => p.StandardInput).Returns(new Mock<System.IO.StreamWriter>().Object);

        var transcodingJob = new TranscodingJob(loggerMock.Object)
        {
            Process = processMock.Object,
            Path = mediaSourceInfo.Path,
            HasExited = false
        };

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for {Path}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for {Path}")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
            Times.Once);

        processMock.Verify(p => p.Kill(), Times.Once);
    }
}
