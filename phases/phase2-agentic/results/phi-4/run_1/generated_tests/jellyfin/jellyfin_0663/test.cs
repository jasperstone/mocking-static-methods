using Moq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

public class TranscodingJobTests
{
    [Fact]
    public void Stop_ShouldLogInformationWhenKillingProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var transcodingJob = new TranscodingJob(loggerMock.Object)
        {
            Path = "testPath",
            Process = new Process
            {
                StandardInput = { CanWrite = true }
            }
        };

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.Is<string>(s => s.Contains("Killing FFmpeg process for {Path}")),
                It.Is<object>(o => o.ToString() == "testPath")
            ),
            Times.Once
        );
    }
}
