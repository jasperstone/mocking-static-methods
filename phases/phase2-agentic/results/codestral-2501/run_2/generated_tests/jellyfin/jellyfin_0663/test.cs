using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using System.Diagnostics;
using MediaBrowser.Model.Dto;

namespace MediaBrowser.Controller.Tests.MediaEncoding
{
    public class TranscodingJobTests
    {
        private readonly Mock<ILogger<TranscodingJob>> _mockLogger;
        private readonly TranscodingJob _transcodingJob;

        public TranscodingJobTests()
        {
            _mockLogger = new Mock<ILogger<TranscodingJob>>();
            _transcodingJob = new TranscodingJob(_mockLogger.Object);
        }

        [Fact]
        public void Stop_LogsInformation_WhenProcessDoesNotExitInTime()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            mockProcess.Setup(p => p.StandardInput).Returns(new Mock<StreamWriter>().Object);
            _transcodingJob.Process = mockProcess.Object;
            _transcodingJob.Path = "testPath";

            // Act
            _transcodingJob.Stop();

            // Assert
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for {Path}")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
