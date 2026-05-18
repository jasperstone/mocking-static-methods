using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _serverConfigMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public void Stop_LogsError_WhenWaitForExitThrowsException()
        {
            // Arrange
            var recorder = CreateRecorder();
            SetPrivateField(recorder, "_hasExited", false);
            SetPrivateField(recorder, "_targetPath", "/test/path");
            
            var processMock = new Mock<Process>();
            SetPrivateField(recorder, "_process", processMock.Object);
            processMock.Setup(p => p.WaitForExit(10000)).Throws(new InvalidOperationException("Test exception"));

            // Act
            recorder.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => t.ToString().Contains("Error waiting for recording process to exit for {Path}")),
                    It.Is<Exception>(ex => ex is InvalidOperationException && ex.Message == "Test exception"),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        private EncodedRecorder CreateRecorder()
        {
            return new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigMock.Object);
        }

        private void SetPrivateField(object target, string fieldName, object value)
        {
            var field = target.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(target, value);
        }
    }
}
