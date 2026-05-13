using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;

namespace MediaBrowser.Controller.MediaEncoding.Tests
{
    public class TranscodingJobTests
    {
        [Fact]
        public void Stop_WhenProcessNotExited_LogsStoppingAndKillsIfNotExitedAfterWait()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = false,
                Path = "testpath",
                Process = new MockProcess()
            };

            var processMock = (MockProcess)job.Process;

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(processMock.KillCalled);
            Assert.True(processMock.StandardInputWritten);
        }

        [Fact]
        public void Stop_WhenProcessExited_DoesNotLogOrKill()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                HasExited = true,
                Path = "testpath",
                Process = new MockProcess()
            };

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        private class MockProcess : Process
        {
            public bool KillCalled { get; private set; }
            public bool StandardInputWritten { get; private set; }

            private readonly MockStreamWriter _standardInput = new MockStreamWriter();

            public override StreamWriter StandardInput => _standardInput;

            public override bool WaitForExit(int milliseconds)
            {
                // Simulate process not exiting within timeout
                return false;
            }

            public override void Kill()
            {
                KillCalled = true;
            }

            private class MockStreamWriter : StreamWriter
            {
                public bool WriteLineCalled { get; private set; }

                public MockStreamWriter() : base(new MemoryStream())
                {
                }

                public override void WriteLine(string? value)
                {
                    WriteLineCalled = true;
                    base.WriteLine(value);
                }
            }
        }
    }
}
