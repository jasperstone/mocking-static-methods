using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common;
using MediaBrowser.Controller;
using MediaBrowser.Controller.MediaEncoding;

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
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            
            _recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
        }

        [Fact]
        public void Stop_WhenProcessHasNotExited_CallsWaitForExit_LogsCallingWaitForExitMessage()
        {
            // Arrange
            var targetPath = "/test/path/recording.ts";
            
            // Set private fields using reflection
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance);
            var processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            
            targetPathField?.SetValue(_recorder, targetPath);
            hasExitedField?.SetValue(_recorder, false);

            // Mock process to not exit within timeout
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            processField?.SetValue(_recorder, processMock.Object);

            // Act
            _recorder.Stop();

            // Assert - Verify the specific LogInformation call on line 231
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Calling recording process.WaitForExit for {Path}", 
                    targetPath),
                Times.Once);
        }
    }
}
