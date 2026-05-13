using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.IO.Tests
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
        public async Task LogError_Called_When_Process_WaitsForExit_Throws()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _configManagerMock.Object);

            var processMock = new Mock<Process>();
            var exception = new Exception("wait error");
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(exception);
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var logFileStreamField = typeof(EncodedRecorder).GetField("_logFileStream", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            // Set private fields
            processField.SetValue(recorder, processMock.Object);
            logFileStreamField.SetValue(recorder, new MemoryStream());
            hasExitedField.SetValue(recorder, false);

            // Act
            // Call the private method via reflection to simulate the code path
            var method = typeof(EncodedRecorder).GetMethod("OnFfMpegProcessExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await Assert.ThrowsAsync<Exception>(() => (Task)method.Invoke(recorder, new object[] { processMock.Object }));

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Error waiting for recording process to exit for")),
                    exception,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
