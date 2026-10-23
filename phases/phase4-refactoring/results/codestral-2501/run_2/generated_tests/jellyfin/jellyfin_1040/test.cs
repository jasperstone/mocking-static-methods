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
        private readonly Mock<ILogger> _loggerMock;
        private readonly Mock<IMediaEncoder> _mediaEncoderMock;
        private readonly Mock<IServerApplicationPaths> _appPathsMock;
        private readonly Mock<IServerConfigurationManager> _serverConfigurationManagerMock;
        private readonly EncodedRecorder _encodedRecorder;

        public EncodedRecorderTests()
        {
            _loggerMock = new Mock<ILogger>();
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
        public async Task Record_ShouldLogError_WhenProcessWaitForExitThrowsException()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("Test exception"));

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }

        [Fact]
        public async Task Record_ShouldLogError_WhenProcessKillThrowsException()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(false);
            processMock.Setup(p => p.Kill()).Throws(new InvalidOperationException("Test exception"));

            // Act
            await _encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<object[]>()),
                Times.Once);
        }
    }
}
