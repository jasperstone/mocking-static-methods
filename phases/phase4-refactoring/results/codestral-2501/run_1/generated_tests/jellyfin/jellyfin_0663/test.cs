using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using System.Diagnostics;

public class TranscodingJobTests
{
    [Fact]
    public void Stop_LogsInformationWhenKillingProcess()
    {
        // Arrange
        var loggerMock = new Mock<ILogger<TranscodingJob>>();
        var transcodingJob = new TranscodingJob(loggerMock.Object)
        {
            Path = "testPath",
            Process = new Process()
        };

        transcodingJob.Process.StartInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            RedirectStandardInput = true,
            UseShellExecute = false
        };
        transcodingJob.Process.Start();

        // Act
        transcodingJob.Stop();

        // Assert
        loggerMock.Verify(
            x => x.LogInformation(
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<It.IsAnyType>()),
            Times.Exactly(2));
    }
}
