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
        public void Stop_ShouldLogInformationWhenKillingProcess()
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
                logger => logger.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Exactly(2));
        }
    }
}
