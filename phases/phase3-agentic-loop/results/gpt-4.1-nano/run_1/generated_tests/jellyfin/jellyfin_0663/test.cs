using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MediaBrowser.Tests
{
    public class TranscodingJobTests
    {
        private readonly Mock<ILogger<TranscodingJob>> _loggerMock;

        public TranscodingJobTests()
        {
            _loggerMock = new Mock<ILogger<TranscodingJob>>();
        }

        [Fact]
        public void Stop_ShouldLogInformationAndKillProcess_WhenProcessIsRunning()
        {
            // Arrange
            var processMock = new Mock<Process>();
            var standardInputStream = new MemoryStream();
            var streamWriter = new StreamWriter(standardInputStream);
            processMock.Setup(p => p.StandardInput).Returns(streamWriter);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());

            var job = new TranscodingJob(_loggerMock.Object)
            {
                Path = "testPath",
                Process = processMock.Object,
                HasExited = false
            };

            // Act
            job.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testPath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }

        [Fact]
        public void ChangeKillTimerIfStarted_ShouldChangeTimer_WhenTimerIsStarted()
        {
            // Arrange
            var job = new TranscodingJob(_loggerMock.Object)
            {
                Id = "job1",
                PlaySessionId = "session1",
                PingTimeout = 1000,
                HasExited = false
            };
            var timer = new Timer((state) => { }, null, 1000, Timeout.Infinite);
            // Use reflection to set private _killTimer
            var field = typeof(TranscodingJob).GetField("_killTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(job, timer);

            // Act
            job.ChangeKillTimerIfStarted();

            // Verify that log was called
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Debug,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Changing kill timer to 1000ms")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        }
    }
}
