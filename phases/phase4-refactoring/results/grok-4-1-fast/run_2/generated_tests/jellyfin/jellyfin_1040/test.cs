#nullable enable

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
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<object> _mediaEncoderMock;
        private readonly Mock<object> _appPathsMock;
        private readonly Mock<object> _serverConfigMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<object>();
            _appPathsMock = new Mock<object>();
            _serverConfigMock = new Mock<object>();
        }

        [Fact]
        public void Stop_WhenWaitForExitThrowsException_LogsError()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
            
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput.WriteLine("q")); // Allow first step to pass
            processMock.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Test exception"));
            
            SetPrivateField(recorder, "_process", processMock.Object);
            SetPrivateField(recorder, "_targetPath", "/test/path.mkv");
            SetPrivateField(recorder, "_hasExited", false);

            // Act
            InvokePrivateMethod(recorder, "Stop");

            // Assert - specifically targeting line 240 LogError call
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error waiting for recording process to exit for {Path}", 
                    "/test/path.mkv"),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenStandardInputWriteThrowsException_LogsError()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
            
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput.WriteLine("q")).Throws(new InvalidOperationException("Test exception"));
            
            SetPrivateField(recorder, "_process", processMock.Object);
            SetPrivateField(recorder, "_targetPath", "/test/path.mkv");
            SetPrivateField(recorder, "_hasExited", false);

            // Act
            InvokePrivateMethod(recorder, "Stop");

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error stopping recording transcoding job for {Path}", 
                    "/test/path.mkv"),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenKillThrowsException_LogsError()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
            
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput.WriteLine("q"));
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Test exception"));
            
            SetPrivateField(recorder, "_process", processMock.Object);
            SetPrivateField(recorder, "_targetPath", "/test/path.mkv");
            SetPrivateField(recorder, "_hasExited", false);

            // Act
            InvokePrivateMethod(recorder, "Stop");

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<Exception>(),
                    "Error killing recording transcoding job for {Path}", 
                    "/test/path.mkv"),
                Times.Once);
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(target, value);
        }

        private static void InvokePrivateMethod(object target, string methodName)
        {
            var method = target.GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(target, null);
        }
    }
}
