using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests.IO
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsInformation_WhenProcessExits()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<EncodedRecorder>>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var processMock = new Mock<Process>();

            var recorder = new EncodedRecorder(
                loggerMock.Object,
                mediaEncoderMock.Object,
                appPathsMock.Object,
                serverConfigurationManagerMock.Object);

            // Simulate the process exiting
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            // Inject the mock process into the recorder
            var recorderType = typeof(EncodedRecorder);
            var processField = recorderType.GetField("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            processField.SetValue(recorder, processMock.Object);

            // Act
            recorder.Stop(loggerMock.Object);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!),
                Times.Once);
        }
    }
}
