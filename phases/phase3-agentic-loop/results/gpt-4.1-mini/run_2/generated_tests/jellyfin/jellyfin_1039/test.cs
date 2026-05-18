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
        // Minimal dummy class for IMediaEncoder with only used members
        private class DummyMediaEncoder : MediaBrowser.Controller.MediaEncoding.IMediaEncoder
        {
            public string EncoderPath => "ffmpeg.exe";
            public Version EncoderVersion => new Version(8, 0);

            // Implement interface members with default or dummy values
            public bool SupportsEncoder(string codec) => false;
            public bool SupportsDecoder(string codec) => false;
            public bool SupportsHwaccel(string hwaccel) => false;
            public bool SupportsFilter(string filter) => false;
            public bool SupportsFilterWithOption(MediaBrowser.Controller.MediaEncoding.FilterOptionType option) => false;
        }

        // Minimal dummy class for IServerApplicationPaths with only used member
        private class DummyServerApplicationPaths : MediaBrowser.Controller.Configuration.IServerApplicationPaths
        {
            public string LogDirectoryPath => Path.GetTempPath();
        }

        // Minimal dummy class for IServerConfigurationManager with only used member
        private class DummyServerConfigurationManager : MediaBrowser.Controller.Configuration.IServerConfigurationManager
        {
            public object GetEncodingOptions() => null;
        }

        [Fact]
        public void Stop_LogsInformationOnWaitForExitCall()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();

            var dummyMediaEncoder = new DummyMediaEncoder();
            var dummyAppPaths = new DummyServerApplicationPaths();
            var dummyServerConfig = new DummyServerConfigurationManager();

            var recorder = new EncodedRecorder(loggerMock.Object, dummyMediaEncoder, dummyAppPaths, dummyServerConfig);

            var processMock = new Mock<Process>();
            var standardInputMock = new Mock<StreamWriter>(Stream.Null);
            processMock.Setup(p => p.StandardInput).Returns(standardInputMock.Object);
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill());

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
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
