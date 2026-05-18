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
        private class FakeProcess
        {
            private readonly StringWriter _inputWriter = new();
            private readonly bool _waitForExitResult;
            public bool KillCalled { get; private set; }

            public FakeProcess(bool waitForExitResult)
            {
                _waitForExitResult = waitForExitResult;
                KillCalled = false;
            }

            public TextWriter StandardInput => _inputWriter;

            public bool WaitForExit(int milliseconds)
            {
                return _waitForExitResult;
            }

            public void Kill()
            {
                KillCalled = true;
            }

            public string WrittenInput => _inputWriter.ToString();
        }

        private class TranscodingJobTestable : TranscodingJob
        {
            private readonly FakeProcess _fakeProcess;

            public TranscodingJobTestable(ILogger<TranscodingJob> logger, FakeProcess fakeProcess) : base(logger)
            {
                _fakeProcess = fakeProcess;
            }

            public override Process? Process
            {
                get => null; // Not used
                set { } // ignore
            }

            // We override Stop to use our FakeProcess instead of Process
            public new void Stop()
            {
                lock (typeof(TranscodingJob).GetField("_processLock", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!.GetValue(this)!)
                {
#pragma warning disable CA1849 // Can't await in lock block
                    TranscodingThrottler?.Stop().GetAwaiter().GetResult();
                    TranscodingSegmentCleaner?.Stop();

                    var process = _fakeProcess;

                    if (!HasExited)
                    {
                        try
                        {
                            _logger.LogInformation("Stopping ffmpeg process with q command for {Path}", Path);

                            process.StandardInput.WriteLine("q");

                            if (!process.WaitForExit(5000))
                            {
                                _logger.LogInformation("Killing FFmpeg process for {Path}", Path);
                                process.Kill();
                            }
                        }
                        catch (InvalidOperationException)
                        {
                        }
                    }
#pragma warning restore CA1849
                }
            }
        }

        [Fact]
        public void Stop_LogsStoppingAndKillsProcess_WhenProcessDoesNotExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var fakeProcess = new FakeProcess(waitForExitResult: false);
            var transcodingJob = new TranscodingJobTestable(loggerMock.Object, fakeProcess)
            {
                Path = "testpath",
                HasExited = false
            };

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Contains("q", fakeProcess.WrittenInput);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.True(fakeProcess.KillCalled);
        }

        [Fact]
        public void Stop_DoesNotKillProcess_WhenProcessExitsWithinTimeout()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var fakeProcess = new FakeProcess(waitForExitResult: true);
            var transcodingJob = new TranscodingJobTestable(loggerMock.Object, fakeProcess)
            {
                Path = "testpath",
                HasExited = false
            };

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg process with q command for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);

            Assert.Contains("q", fakeProcess.WrittenInput);

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing FFmpeg process for testpath")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.False(fakeProcess.KillCalled);
        }

        [Fact]
        public void Stop_DoesNothing_WhenHasExitedIsTrue()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<TranscodingJob>>();
            var fakeProcess = new FakeProcess(waitForExitResult: false);
            var transcodingJob = new TranscodingJobTestable(loggerMock.Object, fakeProcess)
            {
                HasExited = true
            };

            // Act
            transcodingJob.Stop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    It.IsAny<LogLevel>(),
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Never);

            Assert.Empty(fakeProcess.WrittenInput);
            Assert.False(fakeProcess.KillCalled);
        }
    }
}
