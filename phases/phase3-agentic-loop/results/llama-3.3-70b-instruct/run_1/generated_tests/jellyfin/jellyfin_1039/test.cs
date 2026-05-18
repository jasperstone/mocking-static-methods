using Xunit;
using Moq;
using System;
using System.Threading;
using Microsoft.Extensions.Logging;
using MediaBrowser.Controller.MediaEncoding;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void Stop_LogsInformation_WhenWaitingForExit()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder.GetType().GetProperty("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, new System.Diagnostics.Process());
            encodedRecorder.GetType().GetProperty("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, "test_path");

            // Act
            encodedRecorder.GetType().GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(encodedRecorder, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Calling recording process.WaitForExit for {Path}", "test_path"), Times.Once);
        }

        [Fact]
        public void Stop_LogsInformation_WhenKillingProcess()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder.GetType().GetProperty("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, new System.Diagnostics.Process());
            encodedRecorder.GetType().GetProperty("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, "test_path");

            // Act
            encodedRecorder.GetType().GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(encodedRecorder, null);

            // Assert
            loggerMock.Verify(l => l.LogInformation("Killing ffmpeg recording process for {Path}", "test_path"), Times.Once);
        }

        [Fact]
        public void Stop_LogsError_WhenStoppingFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<MediaBrowser.Common.Configuration.IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<MediaBrowser.Controller.Configuration.IServerConfigurationManager>();
            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder.GetType().GetProperty("_process", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, new System.Diagnostics.Process());
            encodedRecorder.GetType().GetProperty("_targetPath", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(encodedRecorder, "test_path");

            // Act
            encodedRecorder.GetType().GetMethod("Stop", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(encodedRecorder, null);

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error stopping recording transcoding job for {Path}", "test_path"), Times.Once);
        }
    }
}
