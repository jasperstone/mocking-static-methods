using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsError_WhenWaitForExitThrowsException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigMock = new Mock<IServerConfigurationManager>();

            mediaEncoderMock.SetupGet(m => m.EncoderPath).Returns("ffmpeg.exe");
            appPathsMock.SetupGet(a => a.LogDirectoryPath).Returns(Path.GetTempPath());

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigMock.Object);

            // Setup private fields via reflection
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("WaitForExit failed"));
            processMock.Setup(p => p.Kill());

            var targetPath = "test.ts";

            // Set private fields _process and _targetPath and _hasExited = false
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            processField.SetValue(recorder, processMock.Object);
            targetPathField.SetValue(recorder, targetPath);
            hasExitedField.SetValue(recorder, false);

            // Act
            // Call Stop method via reflection since it's private
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
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
