using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

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

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigMock.Object);

            // Setup private fields via reflection
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());

            // Set private fields _process and _targetPath and _hasExited
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            processField.SetValue(recorder, processMock.Object);
            targetPathField.SetValue(recorder, "testpath.ts");
            hasExitedField.SetValue(recorder, false);

            // Act
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for testpath.ts")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
