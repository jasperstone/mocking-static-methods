using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private class DummyProcess
        {
            public bool HasExited { get; set; }
            public bool ThrowOnStandardInputWriteLine { get; set; }
            public bool ThrowOnWaitForExit { get; set; }
            public bool ThrowOnKill { get; set; }

            public StreamWriter StandardInput { get; }

            public DummyProcess()
            {
                StandardInput = new DummyStreamWriter(this);
            }

            public bool WaitForExit(int milliseconds)
            {
                if (ThrowOnWaitForExit)
                    throw new InvalidOperationException("WaitForExit exception");
                return false;
            }

            public void Kill()
            {
                if (ThrowOnKill)
                    throw new InvalidOperationException("Kill exception");
            }

            private class DummyStreamWriter : StreamWriter
            {
                private readonly DummyProcess _parent;

                public DummyStreamWriter(DummyProcess parent) : base(Stream.Null)
                {
                    _parent = parent;
                }

                public override void WriteLine(string value)
                {
                    if (_parent.ThrowOnStandardInputWriteLine)
                        throw new InvalidOperationException("WriteLine exception");
                }
            }
        }

        private class TestEncodedRecorder : EncodedRecorder
        {
            public TestEncodedRecorder(ILogger logger) : base(logger,
                new Mock<IMediaEncoder>().Object,
                new Mock<IServerApplicationPaths>().Object,
                new Mock<IServerConfigurationManager>().Object)
            {
            }

            public void SetProcess(Mock<Process> mockProcess, DummyProcess dummyProcess)
            {
                var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                mockProcess.Setup(p => p.StandardInput).Returns(dummyProcess.StandardInput);
                mockProcess.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(() =>
                {
                    if (dummyProcess.ThrowOnWaitForExit)
                        throw new InvalidOperationException("WaitForExit exception");
                    return false;
                });
                mockProcess.Setup(p => p.Kill()).Callback(() =>
                {
                    if (dummyProcess.ThrowOnKill)
                        throw new InvalidOperationException("Kill exception");
                });
                mockProcess.Setup(p => p.HasExited).Returns(dummyProcess.HasExited);

                processField.SetValue(this, mockProcess.Object);
                targetPathField.SetValue(this, "testpath.ts");
                hasExitedField.SetValue(this, false);
            }

            public void CallStop()
            {
                var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                stopMethod.Invoke(this, null);
            }
        }

        [Fact]
        public void Stop_LogsError_WhenStandardInputWriteLineThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var recorder = new TestEncodedRecorder(loggerMock.Object);

            var dummyProcess = new DummyProcess
            {
                ThrowOnStandardInputWriteLine = true,
                HasExited = false
            };

            var mockProcess = new Mock<Process>();

            recorder.SetProcess(mockProcess, dummyProcess);

            recorder.CallStop();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error stopping recording transcoding job for testpath.ts")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_LogsError_WhenWaitForExitThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var recorder = new TestEncodedRecorder(loggerMock.Object);

            var dummyProcess = new DummyProcess
            {
                ThrowOnWaitForExit = true,
                HasExited = false
            };

            var mockProcess = new Mock<Process>();

            recorder.SetProcess(mockProcess, dummyProcess);

            recorder.CallStop();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for testpath.ts")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_LogsError_WhenKillThrows()
        {
            var loggerMock = new Mock<ILogger>();
            var recorder = new TestEncodedRecorder(loggerMock.Object);

            var dummyProcess = new DummyProcess
            {
                ThrowOnKill = true,
                HasExited = false
            };

            var mockProcess = new Mock<Process>();

            recorder.SetProcess(mockProcess, dummyProcess);

            recorder.CallStop();

            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error killing recording transcoding job for testpath.ts")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
