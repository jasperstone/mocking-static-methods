using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_Should_LogInformation_When_ProcessIsActive()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var processMock = new Mock<Process>();

            // Setup StandardInput.WriteLine
            var standardInputMock = new Mock<StreamWriter>();
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);

            // Setup WaitForExit to return false to simulate process not exited
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);

            // Setup Kill method
            processMock.Setup(p => p.Kill());

            var job = new TranscodingJob(loggerMock.Object)
            {
                Process = processMock.Object,
                Path = "testPath",
                HasExited = false
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            // Verify that process.StandardInput.WriteLine("q") was called
            standardInputMock.Verify(s => s.WriteLine("q"), Times.Once);

            // Verify that process.Kill() was called
            processMock.Verify(p => p.Kill(), Times.Once);
        }
    }
}
