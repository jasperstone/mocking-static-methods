using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Model.Dto;

namespace Jellyfin.LiveTv.Tests.IO
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
            _recorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigMock.Object);
        }

        [Fact]
        public void Stop_WaitForExit_ThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/path/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Protected().Setup("WaitForExit", 10000).Throws(new InvalidOperationException("Process error"));
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            InvokeStop();

            // Assert - verifies LogError call on line 240
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.Contains("Error waiting for recording process to exit for " + targetPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_Kill_ThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/path/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Protected().Setup("WaitForExit", 10000).Returns(false);
            processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Kill error"));
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            InvokeStop();

            // Assert - verify kill error log
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.Contains("Error killing recording transcoding job for " + targetPath)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            typeof(EncodedRecorder).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(_recorder, value);
        }

        private void InvokeStop()
        {
            typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.Invoke(_recorder, null);
        }
    }
}
