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
        public async Task Stop_LogsInformation_WhenStoppingProcess()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Returns(true);
            processMock.Setup(p => p.StandardInput.WriteLine("q"));

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);
            encodedRecorder.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogInformation(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Exactly(3));
        }

        [Fact]
        public async Task Stop_LogsError_WhenExceptionOccurs()
        {
            // Arrange
            var mediaSource = new MediaSourceInfo();
            var targetFile = "test.ts";
            var duration = TimeSpan.FromSeconds(10);
            var cancellationToken = new CancellationToken();

            var processMock = new Mock<Process>();
            processMock.Setup(p => p.WaitForExit(It.IsAny<int>())).Throws(new Exception("Test exception"));
            processMock.Setup(p => p.StandardInput.WriteLine("q"));

            var encodedRecorder = new EncodedRecorder(
                _loggerMock.Object,
                _mediaEncoderMock.Object,
                _appPathsMock.Object,
                _serverConfigurationManagerMock.Object);

            // Act
            await encodedRecorder.Record(null, mediaSource, targetFile, duration, () => { }, cancellationToken);
            encodedRecorder.Stop();

            // Assert
            _loggerMock.Verify(
                x => x.LogError(
                    It.IsAny<EventId>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>(),
                    It.IsAny<It.IsAnyType>()),
                Times.Exactly(2));
        }
    }
}
