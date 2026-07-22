using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Common;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;
        private readonly EncodedRecorder _recorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _appPathsMock.Setup(x => x.LogDirectoryPath).Returns("/logs");
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            _recorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigMock.Object);
        }

        [Fact]
        public void Stop_WhenProcessHasNotExitedAndWaitForExitTimesOut_ShouldLogCallingWaitForExit()
        {
            // Arrange
            var targetPath = "/path/to/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            InvokePrivateMethod("Stop");

            // Assert - verify the specific LogInformation call on line 231
            _loggerMock.Verify(
                x => x.LogInformation(
                    "Calling recording process.WaitForExit for {Path}", 
                    targetPath),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(EncodedRecorder).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_recorder, value);
        }

        private void InvokePrivateMethod(string methodName)
        {
            var method = typeof(EncodedRecorder).GetMethod(methodName, 
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(_recorder, null);
        }
    }
}
