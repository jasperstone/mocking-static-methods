using System;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Common;
using MediaBrowser.Common.Configuration;
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
            _mediaEncoderMock.Setup(m => m.EncoderPath).Returns("/path/to/ffmpeg");
            _mediaEncoderMock.Setup(m => m.EncoderVersion).Returns(new Version(4, 0));

            _appPathsMock = new Mock<IServerApplicationPaths>();
            _appPathsMock.Setup(m => m.LogDirectoryPath).Returns("/logs");

            _serverConfigMock = new Mock<IServerConfigurationManager>();
            _serverConfigMock.Setup(m => m.GetEncodingOptions()).Returns(new Mock<IEncodingOptions>().Object);

            _recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
        }

        [Fact]
        public void Stop_WhenProcessNotExited_CallsWaitForExitAndLogsInformationMessage()
        {
            // Arrange
            SetPrivateField("_hasExited", false);
            SetPrivateField("_targetPath", "/test/path.mkv");

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(true);
            SetPrivateField("_process", processMock.Object);

            // Act
            _recorder.Stop();

            // Assert - Verify the specific LogInformation call on line 231
            _loggerMock.Verify(
                m => m.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => 
                        ((string)v.ToString()!).Contains("Calling recording process.WaitForExit for /test/path.mkv")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(EncodedRecorder).GetField(fieldName, 
                BindingFlags.NonPublic | BindingFlags.Instance)!;
            field.SetValue(_recorder, value);
        }
    }
}
