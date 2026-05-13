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
        public async Task Stop_LogsError_WhenWaitForExitThrowsException()
        {
            // Arrange
            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new InvalidOperationException("Test exception"));

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            // Act
            encodedRecorder.Stop();

            // Assert
            _loggerMock.Verify(
                logger => logger.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<Exception>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()),
                Times.Once);
        }
    }
}
