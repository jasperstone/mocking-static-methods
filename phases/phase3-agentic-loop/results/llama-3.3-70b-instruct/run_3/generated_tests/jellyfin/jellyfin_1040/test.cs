using Xunit;
using Moq;
using System;
using System.Threading;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public void LogError_Called_When_WaitForExit_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(10000)).Throws(new Exception("Test exception"));

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder._process = processMock.Object;
            encodedRecorder._targetPath = "test_path";

            // Act
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", "test_path"), Times.Once);
        }

        [Fact]
        public void LogError_Called_When_Kill_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Kill()).Throws(new Exception("Test exception"));

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder._process = processMock.Object;
            encodedRecorder._targetPath = "test_path";

            // Act
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error killing recording transcoding job for {Path}", "test_path"), Times.Once);
        }

        [Fact]
        public void LogError_Called_When_StandardInput_WriteLine_Fails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput.WriteLine("q")).Throws(new Exception("Test exception"));

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);
            encodedRecorder._process = processMock.Object;
            encodedRecorder._targetPath = "test_path";

            // Act
            encodedRecorder.Stop();

            // Assert
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error stopping recording transcoding job for {Path}", "test_path"), Times.Once);
        }
    }
}
