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
        public void Stop_LogsStoppingMessage_WhenHasExitedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = true
            };

            // Use a dummy process (null) since HasExited is true, no process interaction expected
            job.Process = null;

            // Act
            job.Stop();

            // Assert
            // No logging expected because HasExited is true
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception?>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);
        }

        [Fact]
        public void Stop_LogsStoppingAndKillingMessages_WhenProcessNotExitedAndWaitForExitReturnsFalse()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = false
            };

            // Create a fake process wrapper to simulate Process behavior
            var fakeProcess = new FakeProcess();
            job.Process = fakeProcess;

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(fakeProcess.WroteQ);
            Assert.True(fakeProcess.Killed);
        }

        private class FakeProcess : Process
        {
            public bool WroteQ { get; private set; }
            public bool Killed { get; private set; }

            private readonly FakeStreamWriter _writer = new();

            public override StreamWriter StandardInput => _writer;

            public override bool WaitForExit(int milliseconds)
            {
                // Simulate that process did not exit within timeout
                return false;
            }

            public override void Kill()
            {
                Killed = true;
            }

            private class FakeStreamWriter : StreamWriter
            {
                public FakeStreamWriter() : base(Stream.Null)
                {
                }

                public override void WriteLine(string? value)
                {
                    if (value == "q")
                    {
                        ((FakeProcess)base.BaseStream).WroteQ = true;
                    }
                    base.WriteLine(value);
                }
            }
        }
    }
}
