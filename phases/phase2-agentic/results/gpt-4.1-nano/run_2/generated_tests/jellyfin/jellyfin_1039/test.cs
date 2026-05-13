using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
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
        public async Task Stop_ShouldLogInformation_WhenProcessHasNotExited()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _configManagerMock.Object);

            // Use reflection or internal setup to set private fields
            var targetPath = "testPath";
            var processMock = new Mock<Process>();
            var processExited = false;

            // Setup process properties
            processMock.Setup(p => p.StandardInput).Returns(Mock.Of<StreamWriter>());
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false).Callback(() => processExited = true);
            processMock.Setup(p => p.Kill()).Verifiable();

            // Set private fields via reflection or internal test hooks
            var processField = typeof(EncodedRecorder).GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            processField.SetValue(recorder, processMock.Object);
            hasExitedField.SetValue(recorder, false);
            targetPathField.SetValue(recorder, targetPath);

            // Act
            recorder.GetType().GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(recorder, null);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}
