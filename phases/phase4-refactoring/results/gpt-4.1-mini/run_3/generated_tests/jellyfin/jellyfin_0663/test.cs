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
        public void Stop_LogsInformationAndKillsProcessIfNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = false
            };

            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.StandardInput).Returns(new FakeStreamWriter());
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());

            job.SetProcess(processMock.Object);

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

            processMock.Verify(p => p.StandardInput.WriteLine("q"), Times.Once);
            processMock.Verify(p => p.WaitForExit(5000), Times.Once);
            processMock.Verify(p => p.Kill(), Times.Once);
        }

        [Fact]
        public void Stop_DoesNotKillProcessIfHasExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = true
            };

            var processMock = new Mock<IProcess>();
            job.SetProcess(processMock.Object);

            // Act
            job.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            processMock.Verify(p => p.StandardInput.WriteLine(It.IsAny<string>()), Times.Never);
            processMock.Verify(p => p.WaitForExit(It.IsAny<int>()), Times.Never);
            processMock.Verify(p => p.Kill(), Times.Never);
        }

        [Fact]
        public void Stop_CatchesInvalidOperationException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var job = new TranscodingJob(loggerMock.Object)
            {
                Path = "testpath",
                HasExited = false
            };

            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.StandardInput).Returns(new FakeStreamWriter());
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws<InvalidOperationException>();

            job.SetProcess(processMock.Object);

            // Act & Assert
            var ex = Record.Exception(() => job.Stop());
            Assert.Null(ex);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private class FakeStreamWriter : TextWriter
        {
            public override Encoding Encoding => Encoding.UTF8;
            public bool WriteLineCalled { get; private set; }

            public override void WriteLine(string? value)
            {
                WriteLineCalled = true;
            }
        }
    }

    // Interface to abstract Process for testing
    public interface IProcess : IDisposable
    {
        TextWriter StandardInput { get; }
        bool WaitForExit(int milliseconds);
        void Kill();
    }

    // Extension method to set IProcess on TranscodingJob for testing
    public static class TranscodingJobTestExtensions
    {
        public static void SetProcess(this TranscodingJob job, IProcess process)
        {
            // Use reflection to set the private Process property to a wrapper that delegates to IProcess
            var wrapper = new ProcessWrapper(process);
            var processField = typeof(TranscodingJob).GetProperty("Process");
            processField?.SetValue(job, wrapper);
        }

        private class ProcessWrapper : Process
        {
            private readonly IProcess _inner;

            public ProcessWrapper(IProcess inner)
            {
                _inner = inner;
            }

            public override TextWriter StandardInput => _inner.StandardInput;

            public override bool WaitForExit(int milliseconds) => _inner.WaitForExit(milliseconds);

            public override void Kill() => _inner.Kill();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _inner.Dispose();
                }
                base.Dispose(disposing);
            }
        }
    }
}
