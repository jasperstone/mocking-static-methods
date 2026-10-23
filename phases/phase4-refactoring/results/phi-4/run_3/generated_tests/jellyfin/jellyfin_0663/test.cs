using System;
using System.Diagnostics;
using System.Threading;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

public class TranscodingJobTests
{
    [Fact]
    public void Stop_ShouldLogInformationWhenKillingProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var processMock = new Mock<Process>();
        processMock.Setup(p => p.WaitForExit(5000)).Returns(false);
        processMock.Setup(p => p.Kill());

        var transcodingJob = new TranscodingJob(loggerMock.Object)
        {
            Path = "testPath",
            Process = processMock.Object,
            HasExited = false
        };

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(
            l => l.LogInformation(
                "Killing FFmpeg process for {Path}",
                It.Is<object>(o => o.ToString() == "testPath")
            ),
            Times.Once
        );
    }
}
