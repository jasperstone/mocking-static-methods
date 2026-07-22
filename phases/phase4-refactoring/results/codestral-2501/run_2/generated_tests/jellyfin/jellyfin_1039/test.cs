using System;
using System.Diagnostics;
using System.Reflection;
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
        public void Stop_LogsInformation_WhenProcessHasNotExited()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var recorder = new EncodedRecorder(
                loggerMock.Object,
                mediaEncoderMock.Object,
                appPathsMock.Object,
                serverConfigurationManagerMock.Object);

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var processField = typeof(EncodedRecorder).GetField("_process", BindingFlags.NonPublic | BindingFlags.Instance);
            processField.SetValue(recorder, processMock.Object);

            var targetPathField = typeof(EncodedRecorder).GetField("_targetPath", BindingFlags.NonPublic | BindingFlags.Instance);
            targetPathField.SetValue(recorder, "testPath");

            var hasExitedField = typeof(EncodedRecorder).GetField("_hasExited", BindingFlags.NonPublic | BindingFlags.Instance);
            hasExitedField.SetValue(recorder, false);

            // Act
            var stopMethod = typeof(EncodedRecorder).GetMethod("Stop", BindingFlags.NonPublic | BindingFlags.Instance);
            stopMethod.Invoke(recorder, null);

            // Assert
            loggerMock.Verify(
                logger => logger.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for testPath")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)),
                Times.Once);
        }
    }
}
