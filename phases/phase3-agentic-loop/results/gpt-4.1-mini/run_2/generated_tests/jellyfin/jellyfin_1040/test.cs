using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private class DummyMediaEncoder : IMediaEncoder
        {
            public string EncoderPath => "ffmpeg";
            public Version EncoderVersion => new Version(8, 0);
        }

        private class DummyServerApplicationPaths : IServerApplicationPaths
        {
            public string LogDirectoryPath => Path.GetTempPath();
        }

        private class DummyServerConfigurationManager : IServerConfigurationManager
        {
            public object GetEncodingOptions() => null;
        }

        [Fact]
        public void Stop_WhenWaitForExitThrows_LogsError()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoder = new DummyMediaEncoder();
            var appPaths = new DummyServerApplicationPaths();
            var serverConfig = new DummyServerConfigurationManager();

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoder, appPaths, serverConfig);

            // Setup a Process mock that throws on WaitForExit
            var processMock = new Mock<Process>();
            var stdInMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(stdInMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("WaitForExit failed"));
            processMock.Setup(p => p.Kill()).Verifiable();

            // Set private fields _process, _targetPath, _hasExited
            var processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance);

            processField.SetValue(recorder, processMock.Object);
            targetPathField.SetValue(recorder, "testpath.ts");
            hasExitedField.SetValue(recorder, false);

            // Act
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
            loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for testpath.ts")),
                    It.IsAny<InvalidOperationException>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
