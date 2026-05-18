using Xunit;
using Moq;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_LogsInformation_WhenProcessDoesNotExitInTime()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(new MemoryStream()));

            var transcodingJob = new TranscodingJob(loggerMock.Object)
            {
                Path = "testPath",
                Process = processMock.Object
            };

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testPath")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Information),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testPath")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
