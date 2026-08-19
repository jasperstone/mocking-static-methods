using System;
using System.Diagnostics;
using System.Reflection;
using Jellyfin.Extensions;
using MediaBrowser.Common;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public sealed class EncodedRecorderTests : IDisposable
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;
        private readonly EncodedRecorder _recorder;
        private bool _disposed;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _mediaEncoderMock.Setup(m => m.EncoderPath).Returns("/path/to/ffmpeg");
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _appPathsMock.Setup(a => a.LogDirectoryPath).Returns("/logs");
            _serverConfigMock = new Mock<IServerConfigurationManager>();

            _recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _recorder?.Dispose();
            }

            _disposed = true;
        }

        [Fact]
        public void Stop_WhenProcessWaitForExitCalled_LogsWaitForExitMessage()
        {
            // Arrange
            SetPrivateField("_hasExited", false);
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(true);
            SetPrivateField("_process", processMock.Object);
            SetPrivateField("_targetPath", "/test/path.mkv");

            // Act
            InvokePrivateMethod("Stop");

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    "Calling recording process.WaitForExit for {Path}",
                    "/test/path.mkv"),
                Times.Once);
        }

        private void SetPrivateField(string fieldName, object value)
        {
            var field = typeof(EncodedRecorder).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(_recorder, value);
        }

        private void InvokePrivateMethod(string methodName)
        {
            var method = typeof(EncodedRecorder).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(_recorder, null);
        }
    }
}
