using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller;
using MediaBrowser.Common.Configuration;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;
        private readonly EncodedRecorder _recorder;
        private readonly FieldInfo _processField;
        private readonly FieldInfo _targetPathField;
        private readonly FieldInfo _hasExitedField;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
            
            _recorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigMock.Object);
            
            _processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            _targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            _hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        [Fact]
        public void Stop_WhenWaitForExitThrowsException_LogsError()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Process error"));
            
            _processField.SetValue(_recorder, mockProcess.Object);
            _targetPathField.SetValue(_recorder, "/test/path.mkv");
            _hasExitedField.SetValue(_recorder, false);

            // Act
            InvokeStopMethod();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object? state) => 
                        state?.ToString()?.Contains("Error waiting for recording process to exit for /test/path.mkv") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenStandardInputWriteThrowsException_LogsError()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            var mockStdIn = new Mock<StreamWriter>();
            mockStdIn.Setup(x => x.WriteLine("q")).Throws(new InvalidOperationException("Input error"));
            mockProcess.SetupGet(p => p.StandardInput).Returns(mockStdIn.Object);
            
            _processField.SetValue(_recorder, mockProcess.Object);
            _targetPathField.SetValue(_recorder, "/test/path.mkv");
            _hasExitedField.SetValue(_recorder, false);

            // Act
            InvokeStopMethod();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object? state) => 
                        state?.ToString()?.Contains("Error stopping recording transcoding job for /test/path.mkv") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenKillThrowsException_LogsError()
        {
            // Arrange
            var mockProcess = new Mock<Process>();
            mockProcess.Setup(p => p.WaitForExit(10000)).Returns(false);
            mockProcess.Setup(p => p.Kill()).Throws(new InvalidOperationException("Kill error"));
            
            _processField.SetValue(_recorder, mockProcess.Object);
            _targetPathField.SetValue(_recorder, "/test/path.mkv");
            _hasExitedField.SetValue(_recorder, false);

            // Act
            InvokeStopMethod();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((object? state) => 
                        state?.ToString()?.Contains("Error killing recording transcoding job for /test/path.mkv") == true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private void InvokeStopMethod()
        {
            typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(_recorder, null);
        }
    }
}
