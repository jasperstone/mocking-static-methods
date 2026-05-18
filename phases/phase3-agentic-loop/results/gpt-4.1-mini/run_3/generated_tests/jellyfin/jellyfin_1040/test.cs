using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Model.Configuration;

namespace Jellyfin.LiveTv.Tests.IO
{
    // Minimal stub implementations for dependencies
    class StubMediaEncoder : MediaBrowser.Controller.MediaEncoding.IMediaEncoder
    {
        public string EncoderPath => "ffmpeg";
        public Version EncoderVersion => new Version(8, 0);
    }

    // We will mock IServerApplicationPaths and IServerConfigurationManager instead of stubs
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsError_WhenWaitForExitThrows()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoder = new StubMediaEncoder();

            var appPathsMock = new Mock<MediaBrowser.Common.IServerApplicationPaths>();
            appPathsMock.SetupGet(a => a.LogDirectoryPath).Returns(Path.GetTempPath());

            var serverConfigMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            serverConfigMock.Setup(s => s.GetEncodingOptions()).Returns((EncodingOptions)null);

            var recorder = new EncodedRecorder(loggerMock.Object, mediaEncoder, appPathsMock.Object, serverConfigMock.Object);

            // Setup private fields via reflection to simulate the state before Stop is called
            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>();
            processMock.SetupGet(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("WaitForExit failed"));
            processMock.Setup(p => p.Kill()).Verifiable();

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
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for")),
                    It.Is<InvalidOperationException>(ex => ex.Message == "WaitForExit failed"),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
