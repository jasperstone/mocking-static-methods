#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<object> _mediaEncoderMock;
        private readonly Mock<object> _appPathsMock;
        private readonly Mock<object> _serverConfigMock;
        private readonly EncodedRecorder _recorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<object>();
            _appPathsMock = new Mock<object>();
            _serverConfigMock = new Mock<object>();
            
            // Use reflection to create instance bypassing constructor dependencies
            var constructor = typeof(EncodedRecorder).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance)[0];
            _recorder = (EncodedRecorder)constructor.Invoke(new object[] { _loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigMock.Object });
        }

        [Fact]
        public void Stop_WhenWaitForExitThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StringWriter());
            processMock.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Process error"));
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            _recorder.Stop();

            // Assert - Verifies LogError on line 240
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("Error waiting for recording process to exit for ") && msg.Contains(targetPath)),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenStandardInputWriteThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/recording.ts";
            var processMock = new Mock<Process>();
            var failingStream = new Mock<StreamWriter>();
            failingStream.Setup(s => s.WriteLine("q")).Throws(new InvalidOperationException("Input error"));
            processMock.Setup(p => p.StandardInput).Returns(failingStream.Object);
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            _recorder.Stop();

            // Assert - Verifies first LogError call
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("Error stopping recording transcoding job for ") && msg.Contains(targetPath)),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenKillThrowsException_LogsError()
        {
            // Arrange
            var targetPath = "/test/recording.ts";
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StringWriter());
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Kill error"));
            
            SetPrivateField("_targetPath", targetPath);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_hasExited", false);

            // Act
            _recorder.Stop();

            // Assert - Verifies LogError when killing process
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    It.Is<string>(msg => msg.Contains("Error killing recording transcoding job for ") && msg.Contains(targetPath)),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(EncodedRecorder).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_recorder, value);
        }
    }
}
