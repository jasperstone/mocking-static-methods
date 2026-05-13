using System;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.LiveTv.IO;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Jellyfin.LiveTv.Tests
{
    public class EncodedRecorderTests
    {
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger>();
            _mediaEncoderMock = new Mock<IMediaEncoder>();
            _appPathsMock = new Mock<IServerApplicationPaths>();
            _serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
        }

        [Fact]
        public async Task Record_LogsError_WhenWaitingForProcessToExitFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "targetFile.ts";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Simulate an exception when waiting for the process to exit
            encodedRecorder._process = new Process();
            encodedRecorder._process.WaitForExit += (sender, args) => throw new Exception("Test exception");

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", targetFile), Times.Once);
        }

        [Fact]
        public async Task Record_LogsError_WhenKillingProcessFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "targetFile.ts";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Simulate an exception when killing the process
            encodedRecorder._process = new Process();
            encodedRecorder._process.Kill += (sender, args) => throw new Exception("Test exception");

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error killing recording transcoding job for {Path}", targetFile), Times.Once);
        }

        [Fact]
        public async Task Record_LogsError_WhenStoppingProcessFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "targetFile.ts";
            var duration = TimeSpan.FromMinutes(1);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Simulate an exception when stopping the process
            encodedRecorder._process = new Process();
            encodedRecorder._process.StandardInput.WriteLine += (sender, args) => throw new Exception("Test exception");

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error stopping recording transcoding job for {Path}", targetFile), Times.Once);
        }
    }
}
