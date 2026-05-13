using System;
using System.IO;
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
        public async Task Record_LogsErrorWhenStoppingRecordingTranscodingJobFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromSeconds(10);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error stopping recording transcoding job for {Path}", targetFile), Times.Once);
        }

        [Fact]
        public async Task Record_LogsErrorWhenWaitingForRecordingProcessToExitFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromSeconds(10);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", targetFile), Times.Once);
        }

        [Fact]
        public async Task Record_LogsErrorWhenKillingRecordingTranscodingJobFails()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromSeconds(10);
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var encodedRecorder = new EncodedRecorder(_loggerMock.Object, _mediaEncoderMock.Object, _appPathsMock.Object, _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, onStarted, cancellationToken);

            // Assert
            _loggerMock.Verify(logger => logger.LogError(It.IsAny<Exception>(), "Error killing recording transcoding job for {Path}", targetFile), Times.Once);
        }
    }
}
