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
        [Fact]
        public async Task Record_LogsError_WhenWaitingForProcessToExitFails()
        {
            // Arrange
            var loggerMock = new Mock<ILogger>();
            var mediaEncoderMock = new Mock<IMediaEncoder>();
            var appPathsMock = new Mock<IServerApplicationPaths>();
            var serverConfigurationManagerMock = new Mock<IServerConfigurationManager>();
            var directStreamProviderMock = new Mock<IDirectStreamProvider>();
            var mediaSource = new MediaSourceInfo { Path = "path" };
            var targetFile = "targetFile";
            var duration = TimeSpan.FromMinutes(1);
            var cancellationToken = new CancellationTokenSource().Token;

            var encodedRecorder = new EncodedRecorder(loggerMock.Object, mediaEncoderMock.Object, appPathsMock.Object, serverConfigurationManagerMock.Object);

            // Act and Assert
            await Assert.ThrowsAsync<Exception>(() => encodedRecorder.Record(directStreamProviderMock.Object, mediaSource, targetFile, duration, () => { }, cancellationToken));

            loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Error waiting for recording process to exit for {Path}", targetFile), Times.Once);
        }
    }
}
