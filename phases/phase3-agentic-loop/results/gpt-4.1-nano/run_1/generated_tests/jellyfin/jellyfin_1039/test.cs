using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using MediaBrowser.Controller.Configuration;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Controller;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.IO;

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
        public async Task Stop_ShouldLogInformation_WhenCalled()
        {
            // Arrange
            var recorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _configManagerMock.Object);

            // Use reflection to set private fields
            var targetPath = "testPath";
            var processMock = new Mock<Process>();
            var hasExited = false;

            // Setup process mock
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);
            processMock.Setup(p => p.Kill()).Verifiable();

            // Set private fields via reflection
            var recorderType = typeof(EncodedRecorder);
            var targetPathField = recorderType.GetField("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var processField = recorderType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var hasExitedField = recorderType.GetField("_hasExited", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            targetPathField.SetValue(recorder, targetPath);
            processField.SetValue(recorder, processMock.Object);
            hasExitedField.SetValue(recorder, false);

            // Invoke the private Stop method
            var stopMethod = recorderType.GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            await (Task)stopMethod.Invoke(recorder, null);

            // Assert that LogInformation was called with the expected message
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
