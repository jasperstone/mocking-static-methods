using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Jellyfin.LiveTv.IO;

namespace Jellyfin.Tests.LiveTv.IO
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _configManagerMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _configManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task LogError_IsCalled_When_Process_Waits_Throws()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _configManagerMock.Object);

            // Setup process with WaitForExit throwing exception
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("Wait failed"));
            processMock.Setup(p => p.StandardError).Returns(new StreamReader(new MemoryStream()));
            processMock.Setup(p => p.Kill());

            // Use reflection to set private fields
            var logFileStream = new MemoryStream();
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logFileStreamField = typeof(EncodedRecorder).GetField("_logFileStream", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set private fields
            hasExitedField.SetValue(recorder, false);
            processField.SetValue(recorder, processMock.Object);
            logFileStreamField.SetValue(recorder, logFileStream);
            targetPathField.SetValue(recorder, "testPath");

            // Act
            // Call the method that contains the try-catch with LogError
            var method = typeof(EncodedRecorder).GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)method.Invoke(recorder, null);

            // Assert
            _loggerMock.Verify(
                x => x.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", "testPath"),
                Times.Once);
        }
    }
}
