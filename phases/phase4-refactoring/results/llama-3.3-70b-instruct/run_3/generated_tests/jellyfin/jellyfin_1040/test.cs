using Xunit;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediaBrowser.Model.Dto;

namespace Jellyfin.LiveTv.IO.Tests
{
    public class EncodedRecorderTests
    {
        [Fact]
        public async Task RecordFromFile_LogsErrorOnException()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaSource = new MediaSourceInfo();
            var inputFile = "inputFile";
            var targetFile = "targetFile";
            var onStarted = new Action(() => { });
            var cancellationToken = new CancellationToken();

            var mediaEncoderMock = new Mock<IMediaEncoder>();
            mediaEncoderMock.Setup(m => m.EncoderPath).Returns("ffmpeg");

            var appPathsMock = new Mock<IServerApplicationPaths>();
            appPathsMock.Setup(a => a.LogDirectoryPath).Returns("logDirectory");

            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<NotImplementedException>(async () =>
            {
                await encodedRecorder.RecordFromFile(mediaSource, inputFile, targetFile, onStarted, cancellationToken);
            });

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error stopping recording transcoding job for {Path}", targetFile), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", targetFile), Times.Once);
            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error killing recording transcoding job for {Path}", targetFile), Times.Once);
        }
    }
}
