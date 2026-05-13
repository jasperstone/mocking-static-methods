using System;
using System.Diagnostics;
using System.IO;
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
        private readonly Mock<ILogger<EncodedRecorder>> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;
        private readonly EncodedRecorder _encodedRecorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger<EncodedRecorder>>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            _encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);
        }

        [Fact]
        public async Task Record_ShouldLogInformation_WhenRecordingCompleted()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var onStarted = () => { };
            var cancellationToken = CancellationToken.None;

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.Start()).Verifiable();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            _mediaEncoderMock.Setup(m => m.EncoderPath).Returns("ffmpeg");
            _appPathsMock.Setup(a => a.LogDirectoryPath).Returns("logs");

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Recording completed to file")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
        }

        [Fact]
        public void Stop_ShouldLogInformation_WhenStoppingRecordingProcess()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(new MemoryStream()));
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            var privateObject = new PrivateObject(encodedRecorder);
            privateObject.SetFieldOrProperty("_process", processMock.Object);
            privateObject.SetFieldOrProperty("_targetPath", "test.ts");

            // Act
            privateObject.Invoke("Stop");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Stopping ffmpeg recording process for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
        }

        [Fact]
        public void Stop_ShouldLogInformation_WhenCallingProcessWaitForExit()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(new MemoryStream()));
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            var privateObject = new PrivateObject(encodedRecorder);
            privateObject.SetFieldOrProperty("_process", processMock.Object);
            privateObject.SetFieldOrProperty("_targetPath", "test.ts");

            // Act
            privateObject.Invoke("Stop");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Calling recording process.WaitForExit for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
        }

        [Fact]
        public void Stop_ShouldLogInformation_WhenKillingRecordingProcess()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.StandardInput).Returns(new StreamWriter(new MemoryStream()));
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            var privateObject = new PrivateObject(encodedRecorder);
            privateObject.SetFieldOrProperty("_process", processMock.Object);
            privateObject.SetFieldOrProperty("_targetPath", "test.ts");

            // Act
            privateObject.Invoke("Stop");

            // Assert
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Killing ffmpeg recording process for")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)!), Times.Once);
        }
    }
}
