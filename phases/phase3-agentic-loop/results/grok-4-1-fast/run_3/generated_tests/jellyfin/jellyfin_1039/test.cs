using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common;
using MediaBrowser.Controller;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller.Configuration;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests : IDisposable
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

        public void Dispose()
        {
            _recorder?.Dispose();
        }

        [Fact]
        public void Stop_LogsWaitForExitInformationMessage()
        {
            // Arrange
            const string targetPath = "/path/to/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(true);

            // Set private fields using reflection
            typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_recorder, targetPath);
            typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_recorder, processMock.Object);
            typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(_recorder, false);

            // Act - Call private Stop method using reflection
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance)!;
            stopMethod.Invoke(_recorder, null);

            // Assert - Verify the specific LogInformation call on line 231
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Calling recording process.WaitForExit for {Path}",
                    targetPath),
                Times.Once);
        }
    }
}
