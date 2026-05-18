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
        [Fact]
        public void Stop_LogsInformationOnWaitForExitCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            mediaEncoderMock.SetupGet(m => m.EncoderPath).Returns("ffmpeg");
            mediaEncoderMock.SetupGet(m => m.EncoderVersion).Returns(new Version(8, 0));
            appPathsMock.SetupGet(a => a.LogDirectoryPath).Returns(Path.GetTempPath());
            serverConfigMock.Setup(s => s.GetEncodingOptions()).Returns((object)null);

            var recorder = new TestEncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigMock.Object);

            // Setup the process mock to simulate WaitForExit behavior
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(Stream.Null));
            processMock.Setup(p => p.Kill());

            recorder.SetProcess(processMock.Object);
            recorder.SetTargetPath("test.ts");
            recorder.SetHasExited(false);

            // Act
            recorder.InvokeStop();

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for test.ts")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        // Helper subclass to expose private members for testing
        private class TestEncodedRecorder : EncodedRecorder
        {
            public TestEncodedRecorder(ILogger logger, IMediaEncoder mediaEncoder, IServerApplicationPaths appPaths, IServerConfigurationManager serverConfigurationManager)
                : base(logger, mediaEncoder, appPaths, serverConfigurationManager)
            {
            }

            public void InvokeStop()
            {
                // Call the private Stop method via reflection
                var method = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method.Invoke(this, null);
            }

            public void SetProcess(Process process)
            {
                var field = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, process);
            }

            public void SetTargetPath(string path)
            {
                var field = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, path);
            }

            public void SetHasExited(bool hasExited)
            {
                var field = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field.SetValue(this, hasExited);
            }
        }
    }
}
