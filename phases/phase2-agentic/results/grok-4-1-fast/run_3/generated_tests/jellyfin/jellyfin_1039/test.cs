#nullable enable

using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.LiveTv.Tests.IO
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
        public void Stop_WhenProcessWaitForExitCalled_LogsInformationMessage()
        {
            // Arrange
            var recorder = CreateRecorder();
            var targetPath = "/path/to/recording.ts";
            recorder.SetPrivateField("_targetPath", targetPath);
            recorder.SetPrivateField("_hasExited", false);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            recorder.SetPrivateField("_process", processMock.Object);

            // Act
            recorder.CallPrivateMethod("Stop");

            // Assert
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(msg => msg.Contains("Calling recording process.WaitForExit")),
                    It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == targetPath)),
                Times.Once);
        }

        [Fact]
        public void Stop_WhenProcessDoesNotExitWithinTimeout_LogsKillMessage()
        {
            // Arrange
            var recorder = CreateRecorder();
            var targetPath = "/path/to/recording.ts";
            recorder.SetPrivateField("_targetPath", targetPath);
            recorder.SetPrivateField("_hasExited", false);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Returns(false);
            recorder.SetPrivateField("_process", processMock.Object);

            // Act
            recorder.CallPrivateMethod("Stop");

            // Assert - Verifies the flow reaches the kill logging
            _loggerMock.Verify(
                logger => logger.LogInformation(
                    It.Is<string>(msg => msg.Contains("Killing ffmpeg recording process")),
                    It.Is<object[]>(args => args.Length == 1 && args[0].ToString() == targetPath)),
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
    }

    // Extension methods for testing private members
    public static class EncodedRecorderTestExtensions
    {
        public static void SetPrivateField<T>(this EncodedRecorder recorder, string fieldName, T value)
        {
            var field = typeof(EncodedRecorder).GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(recorder, value);
        }

        public static void CallPrivateMethod(this EncodedRecorder recorder, string methodName)
        {
            var method = typeof(EncodedRecorder).GetMethod(methodName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            method?.Invoke(recorder, null);
        }
    }
}
