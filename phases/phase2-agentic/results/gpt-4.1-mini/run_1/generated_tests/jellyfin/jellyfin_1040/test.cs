using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_WhenProcessWaitForExitThrows_LogsError()
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

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigMock.Object);

            // Setup a Process mock that throws on WaitForExit
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(Stream.Null));
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("WaitForExit failed"));
            processMock.Setup(p => p.Kill()).Verifiable();

            // Use reflection to set private fields _process, _targetPath, _hasExited
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            processField.SetValue(recorder, processMock.Object);
            targetPathField.SetValue(recorder, "testpath.ts");
            hasExitedField.SetValue(recorder, false);

            // Act
            // Call Stop method via reflection since it's private
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
            // Verify that LogError was called with the exception from WaitForExit
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
