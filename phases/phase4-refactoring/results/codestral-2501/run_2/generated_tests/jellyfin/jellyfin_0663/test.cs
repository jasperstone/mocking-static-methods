using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using System.Diagnostics;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_ShouldLogInformationWhenProcessDoesNotExitInTime()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                Path = "testPath",
                Process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "ffmpeg",
                        RedirectStandardInput = true,
                        UseShellExecute = false
                    }
                }
            };

            transcodingJob.Process.Start();

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for {Path}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for {Path}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
